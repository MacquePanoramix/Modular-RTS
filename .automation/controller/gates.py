"""Read-only gates for a shadow plan. This module grants no execution authority."""

from __future__ import annotations

import hashlib
import json
import re
import subprocess
from datetime import datetime, timezone
from pathlib import Path

from contracts import ContractError, validate_record, validate_state


def digest(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":"),
                                     allow_nan=False).encode("utf-8")).hexdigest()


def git(repo, *args):
    try:
        result = subprocess.run(["git", "-C", str(repo), *args], capture_output=True,
                                timeout=15, check=False)
    except subprocess.TimeoutExpired as exc:
        raise ContractError("Git inspection timed out") from exc
    if result.returncode:
        # Git stderr may contain credentials/remote URLs. Do not reflect it into receipts.
        raise ContractError("required Git object is unavailable")
    return result.stdout


def head_sha(repo):
    return git(repo, "rev-parse", "HEAD").decode("ascii").strip()


def path_parts(value):
    if not isinstance(value, str) or not value or "\\" in value or ":" in value:
        raise ContractError("expected a repository-relative POSIX path")
    parts = value.split("/")
    for part in parts:
        if part in ("", ".", "..") or part[-1:] in (" ", "."):
            raise ContractError("unsafe path component")
        if re.search(r'[\x00-\x1f<>"|?*]', part):
            raise ContractError("unsupported path character")
        if re.fullmatch(r"(?i)(con|prn|aux|nul|com[0-9]|lpt[0-9])(?:\..*)?", part):
            raise ContractError("reserved path component")
    return parts


def matches(path, scope):
    """Only exact files and directory/** scopes, with case-independent protection."""
    path_parts(path)
    prefix = scope[:-3] if scope.endswith("/**") else scope
    path_parts(prefix)
    path = path.casefold()
    prefix = prefix.casefold()
    return path == prefix or (scope.endswith("/**") and path.startswith(prefix + "/"))


def scopes_overlap(left, right):
    a = left[:-3] if left.endswith("/**") else left
    b = right[:-3] if right.endswith("/**") else right
    return matches(a, right) or matches(b, left)


def safe_file(repo, value):
    parts = path_parts(value)
    root = Path(repo).resolve()
    target = root
    for part in parts:
        target = target / part
        if target.is_symlink() or (hasattr(target, "is_junction") and target.is_junction()):
            raise ContractError("linked paths are not allowed")
    if not target.resolve().is_relative_to(root) or not target.is_file():
        raise ContractError("artifact is missing or outside the repository")
    return target


def check_scope(task, policy):
    for allowed in task["allowed_paths"]:
        for protected in policy["protected_paths"]:
            if scopes_overlap(allowed, protected):
                raise ContractError("task scope overlaps protected control paths")
    for artifact in task["expected_artifacts"]:
        if not any(matches(artifact, scope) for scope in task["allowed_paths"]):
            raise ContractError("expected artifact is outside allowed paths")


def source_contents(repo, task):
    """Load canonical Git blobs, not mutable working-copy files or symlink targets."""
    contents = []
    for source in task["source_manifest"]:
        path_parts(source["path"])
        revision = source["revision"]
        if not re.fullmatch(r"[0-9a-f]{40}", revision):
            raise ContractError("source revision must be an immutable commit SHA")
        if git(repo, "cat-file", "-t", revision).strip() != b"commit":
            raise ContractError("source revision must name a commit, not a tree or tag")
        entry = git(repo, "ls-tree", revision, "--", source["path"]).decode("utf-8")
        if not entry.startswith("100644 blob ") and not entry.startswith("100755 blob "):
            raise ContractError("source must be a regular Git blob")
        content = git(repo, "show", revision + ":" + source["path"])
        if hashlib.sha256(content).hexdigest() != source["sha256"]:
            raise ContractError("source content hash mismatch")
        try:
            text = content.decode("utf-8")
        except UnicodeDecodeError as exc:
            raise ContractError("context sources must be UTF-8 text") from exc
        contents.append({**source, "content": text})
    return contents


def validate_bundle(policy, capabilities, state):
    validate_record(policy, "policy")
    validate_record(capabilities, "capabilities")
    validate_state(state)
    if policy["mode"] != "shadow" or any(policy["authority"].values()):
        raise ContractError("this controller supports shadow mode only")
    if state["project_id"] != policy["project_id"] or capabilities["project_id"] != policy["project_id"]:
        raise ContractError("project identity mismatch")
    if state["policy_version"] != policy["policy_version"]:
        raise ContractError("state policy is stale")
    if state["stopped"] and not state["paused"]:
        raise ContractError("stopped state must also be paused")
    for task in state["tasks"]:
        check_scope(task, policy)


def plan_tick(repo, policy, capabilities, state):
    validate_bundle(policy, capabilities, state)
    result = {"mode": "shadow", "external_effects": False, "selected_task": None,
              "dispatch_key": None, "decision": "idle", "blockers": []}
    if state["stopped"] or state["paused"]:
        result["decision"] = "stopped" if state["stopped"] else "paused"
        return result
    active = [task for task in state["tasks"]
              if task["status"] in ("RUNNING", "VERIFYING", "INTEGRATING")]
    if active:
        result.update(decision="reconciliation_required", blockers=[
            "Existing active work requires a trusted supervisor; shadow mode never reclaims it."])
        return result
    done = {t["id"] for t in state["tasks"] if t["status"] == "DONE"}
    ready = [t for t in state["tasks"] if t["status"] == "READY"
             and set(t["dependencies"]).issubset(done) and t["attempts"] < t["max_attempts"]]
    ready.sort(key=lambda t: (-t["priority"], -t["critical_path_rank"], t["id"]))
    if not ready:
        return result
    task = ready[0]
    result["selected_task"] = task["id"]
    if head_sha(repo) != task["base_sha"]:
        result.update(decision="stale_base", blockers=["Replan against the current base before dispatch."])
        return result
    source_contents(repo, task)
    result.update(decision="would_select", dispatch_key=digest({
        "task": task, "policy": policy, "capabilities": capabilities,
    }), blockers=["Live dispatch is not implemented in this draft.",
                  "Remote claims, billing controls, host isolation and broker commissioning are required."])
    return result


def expired_leases(state, now=None):
    now = now or datetime.now(timezone.utc)
    return [task["id"] for task in state["tasks"] if task["lease"] is not None
            and datetime.fromisoformat(task["lease"]["expires_at"].replace("Z", "+00:00")) <= now]


def inspect_proposal(repo, task, policy, result):
    """Verify structure/files only. Worker-reported checks can never award acceptance."""
    validate_record(result, "worker_result")
    if result["task_id"] != task["id"]:
        raise ContractError("worker result task mismatch")
    check_scope(task, policy)
    artifacts = []
    for path in result["artifact_refs"]:
        if not any(matches(path, scope) for scope in task["allowed_paths"]):
            raise ContractError("worker artifact is outside allowed paths")
        if any(matches(path, scope) for scope in policy["protected_paths"]):
            raise ContractError("worker artifact targets a protected path")
        content = safe_file(repo, path).read_bytes()
        artifacts.append({"path": path, "sha256": hashlib.sha256(content).hexdigest()})
    if result["outcome"] == "candidate":
        if not set(task["expected_artifacts"]).issubset(result["artifact_refs"]):
            raise ContractError("candidate is missing expected artifacts")
        if not set(task["checks"]).issubset(result["checks_reported"]):
            raise ContractError("candidate is missing reported checks")
        if result["unresolved"] or result["failure_class"] is not None:
            raise ContractError("candidate has unresolved work or failure")
    source_contents(repo, task)
    return {"mode": "shadow", "task_id": task["id"], "outcome": result["outcome"],
            "artifacts": artifacts, "accepted": False, "external_effects": False,
            "limitations": ["No authenticated run, immutable candidate or trusted check execution is established.",
                            "Reported checks are worker claims. No state transition or integration is permitted."]}
