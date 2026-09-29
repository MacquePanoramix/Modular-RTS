"""Strict, versioned data contracts for the inactive shadow controller.

Validation establishes structural integrity, not authority or completion. No
content loaded through this module is interpreted as an instruction.
"""

from __future__ import annotations

import json
import math
import re
from datetime import datetime
from functools import lru_cache
from pathlib import Path

from jsonschema import Draft202012Validator, FormatChecker


SCHEMA_DIR = Path(__file__).resolve().parents[1] / "schemas"
KINDS = frozenset(
    {"policy", "capabilities", "task", "state", "worker_result", "receipt", "decision", "agent"}
)
ACTIVE_STATES = frozenset({"RUNNING", "VERIFYING", "INTEGRATING"})


class ContractError(ValueError):
    """A durable record violates its structural or relational contract."""


def _pairs(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ContractError(f"duplicate JSON field: {key}")
        result[key] = value
    return result


def _constant(value):
    raise ContractError(f"non-finite JSON number: {value}")


def load_json(path):
    """Load UTF-8 JSON, rejecting duplicate fields and non-finite numbers."""
    try:
        with Path(path).open(encoding="utf-8") as stream:
            result = json.load(stream, object_pairs_hook=_pairs, parse_constant=_constant)
        _finite(result)
        return result
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ContractError(f"cannot load JSON from {path}: {error}") from error


def _finite(value):
    if isinstance(value, float) and not math.isfinite(value):
        raise ContractError("non-finite JSON number")
    if isinstance(value, dict):
        for child in value.values():
            _finite(child)
    elif isinstance(value, list):
        for child in value:
            _finite(child)


FORMATS = FormatChecker()


@FORMATS.checks("utc-z")
def _utc_z(value):
    if not isinstance(value, str):
        return True  # The schema's type constraint handles non-strings.
    if not re.fullmatch(r"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,6})?Z", value):
        return False
    try:
        datetime.fromisoformat(value.replace("Z", "+00:00"))
        return True
    except ValueError:
        return False


def _safe_path(value, patterns=False):
    if not isinstance(value, str):
        return True
    if not value or value.startswith("/") or any(ord(c) < 32 for c in value):
        return False
    # Backslashes, drive names, ADS and URI forms are deliberately unsupported.
    if any(c in value for c in "\\:<>|\""):
        return False
    path_to_check = value[:-3] if patterns and value.endswith("/**") else value
    if any(c in path_to_check for c in "*?[]"):
        return False
    for segment in value.split("/"):
        if segment in {"", ".", ".."} or segment.endswith((".", " ")):
            return False
        if re.fullmatch(r"(?i:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..*)?", segment):
            return False
    return True


@FORMATS.checks("repo-path")
def _repo_path(value):
    return _safe_path(value)


@FORMATS.checks("repo-pattern")
def _repo_pattern(value):
    return _safe_path(value, patterns=True)


@lru_cache(maxsize=None)
def _validator(kind):
    if kind not in KINDS:
        raise ContractError(f"unknown record kind: {kind}")
    schema = load_json(SCHEMA_DIR / f"{kind}.schema.json")
    Draft202012Validator.check_schema(schema)
    return Draft202012Validator(schema, format_checker=FORMATS)


def validate_record(record, kind):
    """Validate one record; unknown control fields fail closed at every level."""
    _finite(record)
    error = next(_validator(kind).iter_errors(record), None)
    if error is not None:
        location = "/".join(str(part) for part in error.absolute_path) or "<root>"
        raise ContractError(f"{kind} {location}: {error.message}")
    if kind == "task":
        _task_semantics(record)
    return record


def _timestamp(value):
    return datetime.fromisoformat(value.replace("Z", "+00:00"))


def _task_semantics(task):
    source_paths = [source["path"].casefold() for source in task["source_manifest"]]
    if len(set(source_paths)) != len(source_paths):
        raise ContractError(f"task {task['id']} has duplicate source manifest paths")
    if task["attempts"] > task["max_attempts"]:
        raise ContractError(f"task {task['id']} exceeds its attempt limit")
    lease = task["lease"]
    if task["status"] == "RUNNING" and lease is None:
        raise ContractError(f"running task {task['id']} has no lease")
    if lease is None:
        return
    if task["status"] not in ACTIVE_STATES:
        raise ContractError(f"task {task['id']} has a lease outside an active state")
    if lease["task_id"] != task["id"]:
        raise ContractError(f"lease task identity differs for {task['id']}")
    if lease["generation"] != task["generation"] or task["generation"] < 1:
        raise ContractError(f"lease generation differs for {task['id']}")
    if task["attempts"] < 1:
        raise ContractError(f"leased task {task['id']} has no attempt")
    if not (
        _timestamp(lease["claimed_at"])
        <= _timestamp(lease["heartbeat_at"])
        < _timestamp(lease["expires_at"])
    ):
        raise ContractError(f"invalid lease timestamp ordering for {task['id']}")


def validate_state(state):
    """Validate graph and lease identities without treating expiry as reclamation."""
    validate_record(state, "state")
    tasks = {}
    runs = set()
    for task in state["tasks"]:
        _task_semantics(task)
        task_id = task["id"]
        if task_id in tasks:
            raise ContractError(f"duplicate task identity: {task_id}")
        tasks[task_id] = task
        for key in ("project_id", "plan_version", "policy_version"):
            if task[key] != state[key]:
                raise ContractError(f"task {task_id} has stale or mismatched {key}")
        if task["lease"] is not None:
            run_id = task["lease"]["run_id"]
            if run_id in runs:
                raise ContractError(f"duplicate live run: {run_id}")
            runs.add(run_id)
    indegree = {task_id: 0 for task_id in tasks}
    dependents = {task_id: [] for task_id in tasks}
    for task_id, task in tasks.items():
        for dependency in task["dependencies"]:
            if dependency not in tasks:
                raise ContractError(f"task {task_id} has missing dependency {dependency}")
            indegree[task_id] += 1
            dependents[dependency].append(task_id)
    ready = [task_id for task_id, count in indegree.items() if count == 0]
    visited = 0
    while ready:
        task_id = ready.pop()
        visited += 1
        for dependent in dependents[task_id]:
            indegree[dependent] -= 1
            if indegree[dependent] == 0:
                ready.append(dependent)
    if visited != len(tasks):
        raise ContractError("task dependencies contain a cycle")
    return state
