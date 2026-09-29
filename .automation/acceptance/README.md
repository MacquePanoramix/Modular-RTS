# Bootstrap acceptance

These checks cover the offline foundation. They are not a commissioned verifier
for game work and must not be used to authorize integration.

| Criterion | Executable evidence |
| --- | --- |
| BOOT-RECORDS | `test_contracts.py`: malformed/unknown records, invalid paths, graph and lease identities, disabled authority |
| BOOT-RECOVERY | `test_journal.py`: concurrent compare-and-swap writers, duplicate events, rollback, abrupt crash recovery and corruption |
| BOOT-CONTEXT | `test_bootstrap.py`: immutable commit sources, hashes, stale base and scope controls |
| BOOT-NO-EFFECTS | `test_bootstrap.py`: repeated shadow ticks do not change tasks, local controls cannot enable dispatch, worker claims cannot award acceptance |

Run from the repository root with Python 3.12:

```sh
python -m pip install -r .automation/requirements.txt
python -m unittest discover -s .automation/tests -v
```

The optional `Bootstrap checks (manual)` GitHub workflow runs only on explicit
workflow dispatch. It has a read-only token, no persisted checkout credentials,
no model credentials and no scheduler. State-only branch pushes trigger no CI.
The workflow has been prepared, not executed or made a required branch check.

The example task's `WG-DOCS-EVIDENCE-v1` rubric remains a proposed human review:
its links must resolve, historical tests must be distinguished from new runs,
and it must preserve pending Game Director playtest acceptance. Reported check
IDs do not prove these criteria passed. A future protected verifier must bind
actual outcomes to candidate, base, policy, acceptance and decision identities.

See `Docs/AutomationBootstrap.md` for the remaining guide commissioning gates:
remote races/cold start, live process cancellation, billing enforcement,
authenticated decisions, injection/effect-broker isolation, exact-head
integration, recovery after external effects, and a real accepted project task.
