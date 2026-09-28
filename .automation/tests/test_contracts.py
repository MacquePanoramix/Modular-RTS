"""Adversarial contract tests; fixtures are proposals, never completion evidence."""

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from controller.contracts import (  # noqa: E402
    ContractError,
    KINDS,
    SCHEMA_DIR,
    load_json,
    validate_record,
    validate_state,
)
from jsonschema import Draft202012Validator  # noqa: E402


NOW = "2026-09-28T10:00:00Z"


def common(record_id="T-001"):
    return {"schema_version": 1, "id": record_id, "project_id": "P-001", "created_at": NOW}


def task(record_id="T-001"):
    return {
        **common(record_id),
        "objective": "Inspect a bounded local fixture",
        "dependencies": [],
        "base_sha": "a" * 40,
        "plan_version": 1,
        "policy_version": 1,
        "acceptance_version": "AC-001-v1",
        "allowed_paths": ["Docs/AutomationBootstrap.md"],
        "source_manifest": [
            {"path": "Docs/CURRENT_STATE.md", "sha256": "b" * 64, "revision": "a" * 40}
        ],
        "expected_artifacts": ["out/report.json"],
        "checks": ["AC-001"],
        "tier": "worker",
        "max_attempts": 2,
        "timeout_minutes": 30,
        "escalate_on": ["design-change"],
        "status": "DRAFT",
        "priority": 0,
        "critical_path_rank": 0,
        "generation": 0,
        "attempts": 0,
        "lease": None,
    }


def lease(task_id="T-001", run_id="RUN-001"):
    return {
        "task_id": task_id,
        "run_id": run_id,
        "host_id": "HOST-001",
        "generation": 1,
        "claimed_at": NOW,
        "heartbeat_at": NOW,
        "expires_at": "2026-09-28T10:10:00Z",
    }


def active_task(record_id="T-001", run_id="RUN-001"):
    result = task(record_id)
    result.update(status="RUNNING", generation=1, attempts=1, lease=lease(record_id, run_id))
    return result


def state(tasks=None):
    return {
        **common("STATE-001"),
        "plan_version": 1,
        "policy_version": 1,
        "paused": True,
        "stopped": False,
        "tasks": [task()] if tasks is None else tasks,
    }


def worker_result():
    return {
        "task_id": "T-001",
        "run_id": "RUN-001",
        "outcome": "candidate",
        "summary": "Fixture prepared for verification",
        "artifact_refs": ["out/report.json"],
        "checks_reported": ["AC-001"],
        "unresolved": [],
        "failure_class": None,
    }


def policy():
    return {
        **common("POLICY-001"),
        "policy_version": 1,
        "mode": "shadow",
        "repository": "MacquePanoramix/Modular-RTS",
        "default_branch": "main",
        "state_branch": "automation-state",
        "goal": "Prepare an inactive controller for review",
        "audience": ["Game Director"],
        "deliverables": ["Draft pull request"],
        "criterion_ids": ["AC-001"],
        "exclusions": ["Activation"],
        "assumptions": [],
        "accepted_decisions": [],
        "authority": {"dispatch": False, "integrate": False, "publish": False},
        "resource_limits": {
            "max_concurrency": 1,
            "max_attempts": 2,
            "timeout_minutes": 30,
            "paid_usage_allowed": False,
        },
        "human_principals": ["MacquePanoramix"],
        "protected_paths": [".automation/**"],
    }


class ContractTests(unittest.TestCase):
    def test_embedded_task_schema_matches_standalone_contract(self):
        standalone = load_json(SCHEMA_DIR / "task.schema.json")
        standalone = {key: value for key, value in standalone.items()
                      if key not in {"$schema", "title"}}
        embedded = load_json(SCHEMA_DIR / "state.schema.json")["properties"]["tasks"]["items"]
        self.assertEqual(standalone, embedded)

    def test_task_requires_observable_artifacts(self):
        invalid = task()
        invalid["expected_artifacts"] = []
        with self.assertRaises(ContractError):
            validate_record(invalid, "task")
        with self.assertRaises(ContractError):
            validate_state(state([invalid]))

    def test_source_manifest_rejects_duplicate_paths_case_insensitively(self):
        for duplicate_path in ("Docs/CURRENT_STATE.md", "docs/current_state.MD"):
            invalid = task()
            invalid["source_manifest"].append({
                **invalid["source_manifest"][0], "path": duplicate_path,
            })
            with self.subTest(path=duplicate_path):
                with self.assertRaises(ContractError):
                    validate_record(invalid, "task")
                with self.assertRaises(ContractError):
                    validate_state(state([invalid]))

    def test_shadow_policy_cannot_expand_authority_or_resource_limits(self):
        validate_record(policy(), "policy")
        for section, key, value in (
            ("authority", "dispatch", True),
            ("authority", "integrate", True),
            ("authority", "publish", True),
            ("resource_limits", "paid_usage_allowed", True),
            ("resource_limits", "max_concurrency", 2),
            ("resource_limits", "max_attempts", 3),
            ("resource_limits", "timeout_minutes", 60),
        ):
            invalid = policy()
            invalid[section][key] = value
            with self.subTest(key=key), self.assertRaises(ContractError):
                validate_record(invalid, "policy")
        with self.assertRaises(ContractError):
            validate_record({**policy(), "mode": "active"}, "policy")

    def test_other_record_contracts_accept_explicit_unknowns(self):
        fixtures = {
            "capabilities": {
                **common("CAP-001"), "host_os": "Windows", "tools": [],
                "auth_mode": "unverified", "models": [], "integrations": [],
                "quota_observability": "unverified", "probe_time": NOW,
                "limitations": ["Dispatch is not commissioned"],
            },
            "receipt": {
                **common("RECEIPT-001"), "task_id": "T-001", "run_id": "RUN-001",
                "attempt": 1, "executor_id": "HOST-001", "model_id": None,
                "inputs": {key: task()[key] for key in (
                    "base_sha", "plan_version", "policy_version", "acceptance_version",
                    "generation", "source_manifest")},
                "started_at": NOW, "finished_at": NOW, "artifacts": [],
                "candidate_sha": None, "checks": [], "observed_usage": None,
                "outcome": "blocked", "failure_class": "INFRASTRUCTURE",
                "recovery_pointer": None,
            },
            "decision": {
                **common("DECISION-001"), "affected_tasks": ["T-001"],
                "question": "Which reviewed option should be selected?",
                "options": [{"id": "A", "label": "Review", "consequence": "Stay inactive"},
                            {"id": "B", "label": "Defer", "consequence": "No further work"}],
                "recommendation": "A", "delay_consequence": "Stay inactive",
                "authorized_respondents": ["MacquePanoramix"],
                "relevant_past_decisions": [], "resolution": None,
            },
            "agent": {
                **common("AGENT-001"), "role": "worker", "session_id": None,
                "run_id": None, "model_id": None, "surface": "unverified",
                "assigned_task": None, "lease": None, "capabilities": [],
                "heartbeat_at": None, "checkpoint": None, "completion_receipt": None,
            },
        }
        for kind, record in fixtures.items():
            with self.subTest(kind=kind):
                validate_record(record, kind)
                with self.assertRaises(ContractError):
                    validate_record({**record, "authorization_override": True}, kind)

    def test_schemas_are_valid_and_reject_unknown_root_fields(self):
        for kind in KINDS:
            with self.subTest(kind=kind):
                schema = load_json(SCHEMA_DIR / f"{kind}.schema.json")
                Draft202012Validator.check_schema(schema)
                self.assertFalse(schema["additionalProperties"])
                self.assertEqual(set(schema["properties"]), set(schema["required"]))

    def test_nested_objects_reject_unknown_fields(self):
        def visit(node):
            if isinstance(node, dict):
                if node.get("type") == "object":
                    self.assertIs(node.get("additionalProperties"), False)
                    self.assertEqual(set(node["required"]), set(node["properties"]))
                for value in node.values():
                    visit(value)
            elif isinstance(node, list):
                for value in node:
                    visit(value)

        for path in SCHEMA_DIR.glob("*.schema.json"):
            with self.subTest(path=path):
                visit(load_json(path))

    def test_duplicate_and_nonfinite_json_are_rejected(self):
        for text in ('{"id":1,"id":2}', '{"nested":{"x":1,"x":2}}',
                     '{"x":NaN}', '{"x":Infinity}', '{"x":-Infinity}', '{"x":1e999}'):
            with self.subTest(text=text), tempfile.TemporaryDirectory() as directory:
                path = Path(directory) / "record.json"
                path.write_text(text, encoding="utf-8")
                with self.assertRaises(ContractError):
                    load_json(path)

    def test_valid_task_state_and_json(self):
        self.assertEqual(validate_record(task(), "task"), task())
        self.assertEqual(validate_state(state()), state())
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "record.json"
            path.write_text(json.dumps(state()), encoding="utf-8")
            self.assertEqual(load_json(path), state())

    def test_worker_return_is_exactly_eight_fields(self):
        result = worker_result()
        self.assertEqual(len(result), 8)
        validate_record(result, "worker_result")
        for key in result:
            missing = dict(result)
            del missing[key]
            with self.subTest(missing=key), self.assertRaises(ContractError):
                validate_record(missing, "worker_result")
        for key, value in (("status", "DONE"), ("schema_version", 1), ("generation", 1)):
            invalid = {**result, key: value}
            with self.subTest(extra=key), self.assertRaises(ContractError):
                validate_record(invalid, "worker_result")
        with self.assertRaises(ContractError):
            validate_record({**result, "outcome": "DONE"}, "worker_result")
        with self.assertRaises(ContractError):
            validate_record({**result, "failure_class": "UNKNOWN"}, "worker_result")

    def test_versions_types_unknown_fields_and_nullability(self):
        for field, value in (("schema_version", 2), ("schema_version", True),
                             ("plan_version", True), ("attempts", False),
                             ("objective", None), ("allow_paid", True)):
            with self.subTest(field=field, value=value), self.assertRaises(ContractError):
                validate_record({**task(), field: value}, "task")
        invalid = task()
        invalid["source_manifest"][0]["trusted"] = True
        with self.assertRaises(ContractError):
            validate_record(invalid, "task")
        with self.assertRaises(ContractError):
            validate_record(task(), "unregistered")

    def test_utc_timestamp_must_be_real_and_explicit(self):
        for timestamp in ("2026-02-30T10:00:00Z", "2026-09-28T10:00:00",
                          "2026-09-28T10:00:00+00:00", "2026-09-28T25:00:00Z"):
            with self.subTest(timestamp=timestamp), self.assertRaises(ContractError):
                validate_record({**task(), "created_at": timestamp}, "task")
        validate_record({**task(), "created_at": "2026-09-28T10:00:00.125Z"}, "task")

    def test_paths_reject_traversal_absolute_ads_and_ambiguous_windows_names(self):
        bad = ("../secrets", "Docs/../secrets", "/tmp/a", "C:/a", "C:secret",
               "Docs/file:stream", "Docs\\file", "//host/share", "Docs//file",
               "Docs/./file", "Docs/file.", "Docs/file ", "Docs/CON.txt",
               "NUL", "Docs/LPT1.log", "Docs/a\nfile", "Docs/*.md", "**", "")
        for path in bad:
            with self.subTest(path=path), self.assertRaises(ContractError):
                validate_record({**task(), "allowed_paths": [path]}, "task")
        for path in (".automation/controller/**", "Docs/Report file.md", "Docs/a.md"):
            validate_record({**task(), "allowed_paths": [path]}, "task")
        invalid = task()
        invalid["source_manifest"][0]["path"] = "Docs/**"
        with self.assertRaises(ContractError):
            validate_record(invalid, "task")

    def test_graph_detects_duplicates_missing_dependencies_and_cycles(self):
        with self.assertRaises(ContractError):
            validate_state(state([task(), task()]))
        missing = task()
        missing["dependencies"] = ["T-MISSING"]
        with self.assertRaises(ContractError):
            validate_state(state([missing]))
        a, b = task("T-A"), task("T-B")
        a["dependencies"], b["dependencies"] = ["T-B"], ["T-A"]
        with self.assertRaises(ContractError):
            validate_state(state([a, b]))
        a["dependencies"] = ["T-A"]
        with self.assertRaises(ContractError):
            validate_state(state([a]))
        a["dependencies"] = []
        validate_state(state([b, a]))

    def test_state_requires_matching_project_and_versions(self):
        for field, value in (("project_id", "OTHER"), ("plan_version", 2), ("policy_version", 2)):
            invalid = task()
            invalid[field] = value
            with self.subTest(field=field), self.assertRaises(ContractError):
                validate_state(state([invalid]))

    def test_live_lease_identity_generation_and_attempt_are_bound(self):
        validate_state(state([active_task()]))
        with self.assertRaises(ContractError):
            validate_state(state([active_task(), active_task("T-002")]))
        for field, value in (("task_id", "T-OTHER"), ("generation", 2),
                             ("expires_at", NOW), ("heartbeat_at", "2026-09-28T09:59:00Z")):
            invalid = active_task()
            invalid["lease"][field] = value
            with self.subTest(lease_field=field), self.assertRaises(ContractError):
                validate_state(state([invalid]))
        for field, value in (("generation", 0), ("attempts", 0), ("attempts", 3),
                             ("lease", None), ("status", "DONE")):
            invalid = active_task()
            invalid[field] = value
            with self.subTest(task_field=field), self.assertRaises(ContractError):
                validate_state(state([invalid]))

    def test_expired_lease_is_not_implicitly_reclaimed(self):
        # Structural validation must preserve an old lease for supervisor recovery.
        old = active_task()
        old["lease"].update(claimed_at="2020-01-01T00:00:00Z",
                            heartbeat_at="2020-01-01T00:01:00Z",
                            expires_at="2020-01-01T00:10:00Z")
        unchanged = copy.deepcopy(old)
        validate_state(state([old]))
        self.assertEqual(old, unchanged)


if __name__ == "__main__":
    unittest.main()
