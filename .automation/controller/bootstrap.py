"""Offline shadow bootstrap CLI. No model, network, scheduler or integration calls."""

from __future__ import annotations

import argparse
import copy
import importlib.metadata
import json
import platform
import shutil
import sys
import uuid
from pathlib import Path

try:
    from contracts import ContractError, load_json
    from gates import (digest, expired_leases, inspect_proposal, plan_tick,
                       source_contents, validate_bundle)
    from journal import Journal, JournalError
except ModuleNotFoundError:
    print(json.dumps({"ok": False, "error": "Install .automation/requirements.txt using Python 3.12."}))
    raise SystemExit(3)


ROOT = Path(__file__).resolve().parents[2]


def configuration(repo):
    return (load_json(repo / ".automation/policy.json"),
            load_json(repo / ".automation/capabilities.json"),
            load_json(repo / ".automation/examples/state.json"))


def task_by_id(state, task_id):
    for task in state["tasks"]:
        if task["id"] == task_id:
            return task
    raise ContractError("unknown task ID")


def status(snapshot):
    state = snapshot["state"]
    return {"mode": "shadow", "initialized": True, "revision": snapshot["revision"],
            "state_hash": snapshot["state_hash"], "paused": state["paused"],
            "stopped": state["stopped"], "external_effects": False,
            "tasks": [{"id": t["id"], "status": t["status"]} for t in state["tasks"]]}


def run(args):
    repo = args.repo.resolve()
    policy, capabilities, seed = configuration(repo)
    validate_bundle(policy, capabilities, seed)
    state_dir = args.state_dir or repo / ".automation-local"
    # This journal is local demonstration state. It is never a substitute for a remote claim.
    journal = Journal(state_dir / "journal.sqlite3")
    if args.command == "doctor":
        return {"mode": "shadow", "read_only": True, "external_effects": False,
                "python_version": platform.python_version(),
                "jsonschema_version": importlib.metadata.version("jsonschema"),
                "git_on_path": shutil.which("git") is not None,
                "codex_on_path": shutil.which("codex") is not None,
                "configuration_valid": True, "dispatch_supported": False,
                "limitations": capabilities["limitations"],
                "note": "Executable presence is not an authentication, billing, model or host probe."}
    if args.command == "init":
        return status(journal.initialize(seed))
    if args.command == "status" and not (state_dir / "journal.sqlite3").exists():
        return {"mode": "shadow", "initialized": False, "external_effects": False,
                "next_action": "Run init to create a local shadow journal."}
    snapshot = journal.read()
    state = snapshot["state"]
    validate_bundle(policy, capabilities, state)
    if args.command == "status":
        return status(snapshot)
    if args.command in ("pause", "resume", "stop"):
        updated = copy.deepcopy(state)
        updated["paused"] = args.command != "resume"
        updated["stopped"] = args.command == "stop" or (state["stopped"] and args.command != "resume")
        if updated == state:
            return status(snapshot)
        validate_bundle(policy, capabilities, updated)
        return status(journal.apply(snapshot["revision"], str(uuid.uuid4()), args.command,
                                    {"local_operator_command": args.command}, updated))
    if args.command == "reconcile":
        return {**status(snapshot), "journal_events": len(journal.events()),
                "expired_leases": expired_leases(state), "reclaimed_leases": [],
                "note": "Local integrity verified; process/remote reconciliation is not implemented."}
    if args.command == "tick":
        return plan_tick(repo, policy, capabilities, state)
    if args.command == "export-context":
        task = task_by_id(state, args.task_id)
        sources = source_contents(repo, task)
        packet = {"task": task, "policy_hash": digest(policy), "source_manifest": sources}
        return {"mode": "shadow", "packet_hash": digest(packet), "packet": packet,
                "authority": "Reference context only. No launch, acceptance or authority amendment."}
    if args.command == "verify":
        if args.result:
            if not args.task_id:
                raise ContractError("--result requires --task-id")
            return inspect_proposal(repo, task_by_id(state, args.task_id), policy, load_json(args.result))
        for task in state["tasks"]:
            source_contents(repo, task)
        return {"mode": "shadow", "configuration_valid": True, "journal_valid": True,
                "sources_valid": True, "accepted": False,
                "note": "Structural and source checks only; run the test suite separately."}
    raise ContractError("unsupported command")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, default=ROOT)
    parser.add_argument("--state-dir", type=Path)
    commands = parser.add_subparsers(dest="command", required=True)
    for name in ("doctor", "init", "reconcile", "tick", "status", "pause", "resume", "stop"):
        commands.add_parser(name)
    verify = commands.add_parser("verify")
    verify.add_argument("--result", type=Path)
    verify.add_argument("--task-id")
    context = commands.add_parser("export-context")
    context.add_argument("--task-id", required=True)
    args = parser.parse_args(argv)
    try:
        result = run(args)
    except (ContractError, JournalError, OSError, ValueError) as exc:
        print(json.dumps({"ok": False, "error": str(exc)}, sort_keys=True))
        return 2
    print(json.dumps({"ok": True, **result}, ensure_ascii=True, indent=2, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
