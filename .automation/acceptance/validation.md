# Offline validation record

Date: September 28, 2026. Product baseline: `5f613512b5a51f0a77624a2adfc0c8d8d3afe7a5`.
Scope: the offline automation foundation in this draft.

Environment: Windows, Python 3.12.14, jsonschema 4.25.1 and the exact dependency
versions in `.automation/requirements.txt`. Dependencies were installed into an
isolated preparation directory, not into the Unity project or global runtime.

```sh
python -m unittest discover -s .automation/tests -q
```

Final result: **52 tests run; 50 passed, 0 failures, 2 skipped**, 16.483 seconds.
The two skipped cases require symlink creation, which this Windows host denied.
Other path, scope and duplicate-source checks ran. Real symlink rejection still
needs a host that permits those test fixtures; a prepared Linux workflow is not
evidence that it has run.

The passing suite includes simultaneous expected-revision writers, conflicting
idempotency keys, forced abrupt-process termination with dirty SQLite pages,
rollback recovery, tampered state and incomplete events, malformed contracts,
cycles, lease generations, source hashes, tree-versus-commit identity, stale
base selection, pause/stop controls and worker claims that never award acceptance.

Separate documented command checks passed: `doctor`, `init`, `verify`, `tick`,
context export for `WG-DOCS-001`, and inspection of the blocked example worker
return. The example stayed `DRAFT`, paused, and unaccepted. Context export loaded
three pinned sources. No model, scheduler, merge or remote state write was run
by the controller. All operator commands also run in disposable test state.

Independent code review found and corrected acceptance of Git tree IDs as
source commits and unhandled Git read timeouts. Independent documentation review
corrected claims about SQLite recovery writes and context size bounds.

Not run: Unity Editor/tests/builds, Linux workflow execution, subscription-auth
or billing probes, live worker supervision, broker/decision-channel tests,
remote-state races or GitHub-only cold start. Those components are not established
by this record. No claim of full guide commissioning or product acceptance is made.
