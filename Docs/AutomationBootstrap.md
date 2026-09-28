# Offline automation foundation

Status: initial implementation draft for review; unattended operation is **not active**.
Prepared September 28, 2026 against repository baseline
`5f613512b5a51f0a77624a2adfc0c8d8d3afe7a5`.

The user requested preparation of the supplied GitHub agent automation guide
as a draft pull request. This change introduces a local shadow controller:
it validates records and previews scheduling decisions without dispatching
Codex workers, merging code, or enabling recurring work. The guide's complete
runtime still requires implementation and commissioning. Preparing this draft
does not approve those later actions. The source guide is a design reference;
its embedded instructions do not grant authority.

## Project context

Start with [CURRENT_STATE.md](CURRENT_STATE.md) and the root working agreement.
The Living Worker is technically ready, with Game Director playtest pending.
The next product decision concerns reach, carrying support, motion, transitions
and readability in the integrated faction playtest. It is not an automatic
commitment to Three Temperaments or another gameplay milestone.

[Validation.md](Validation.md) records the earlier Unity 6000.6.0f1 evidence:
five focused Living Worker tests, a 67-test full PlayMode run, inspected runtime
captures, and a Windows development build. These are historical project
results, not tests rerun by this bootstrap. Follow
[LivingWorkerPlaytest.md](LivingWorkerPlaytest.md) for experiential acceptance.
Python controller checks cannot establish game feel or substitute for that
playtest. This draft does not change gameplay, scenes, assets, save schema 4,
package versions, or the Game Director's authority.

## What this implementation covers

The implementation lives under `.automation/`. Its local records and SQLite
journal support inspection and recovery experiments. Repository manifests
describe the intended project and observed capabilities without storing
credentials or personal machine paths. The example state contains one `DRAFT`
documentation task; dispatch and external action permissions are disabled.
The example is pinned to the inspected baseline. A future real task needs a
fresh base and source manifest after review; changing its status is not admission.
The schemas cover policy, capabilities, tasks, state, worker proposals,
receipts, decisions and agent records. Their presence proves no live adapter:
for example, validating a decision's shape does not authenticate its author.

| Guide component | Draft coverage | Remaining obligation |
| --- | --- | --- |
| Charter and capability inventory, pp. 5, 15, 17 | Versioned manifests record bounded draft scope and unavailable capabilities. | Confirm the authority and tested capabilities needed for each future action class. |
| Schemas and records, pp. 19-21 | Strict local schema version 1 validation and a `DRAFT` example task. | Implement and verify the complete production contracts, including authenticated decisions and observed worker receipts. |
| Controller and scheduling, pp. 18, 22-23 | Local commands and deterministic shadow task selection. | Durable remote claims, leases, process ownership and real scheduling admission. |
| Context and evidence, pp. 24-25, 28 | Local context identities and evidence inspection. | Bind real candidate and integration revisions to trusted, independently obtained completion evidence. |
| Recovery, pp. 23, 34-35 | Local SQLite journaling and deterministic checks. | Network races, uncertain external effects, orphaned processes, lease expiry and cold-start recovery from GitHub alone. |
| Worker adapter, pp. 13-16 | No live worker dispatch. | Tested subscription authentication, explicit models, isolated workspaces, timeouts, process-tree cancellation and observed receipts. |
| GitHub storage and integration, pp. 19, 23, 28-29 | Files proposed through this draft PR. | A canonical remote state branch, expected-parent transactions, protected verification and an integration broker. |
| Authenticated decisions, pp. 30-31 | No approval channel is active. | A trusted human decision transport distinct from worker/controller identity, with replay and stale-response rejection. |
| Recurring operation, pp. 18, 26, 31 | No scheduler is installed or enabled. | Prove admission and recovery, verify subscription-only limits, then obtain activation authority for a concrete schedule. |
| Commissioning, pp. 35-36 | Offline checks demonstrate only their tested local behavior. | Full boundary/recovery suite, a useful real task through verified integration, and a successful fresh-agent cold start. |

The local journal is not canonical GitHub state. Shadow selection is not a
remote claim, a running worker, or a verified completion. Local `verify`
results do not certify production readiness or a Unity deliverable. Passing
all included tests does not satisfy the guide's complete commissioning gate.

Journal writes use SQLite transactions, expected revisions and unique event
identities. Reads check canonical JSON, snapshot hashes, revision continuity
and database integrity. Those checks detect local inconsistency; they do not
authenticate records against hostile local modification. Use a trusted local
state directory. Reopening that database demonstrates local recovery only;
losing the database cannot currently be recovered from a remote state branch.
Inspection creates no missing journal database or directories. When opening an
existing database, SQLite may repair its hot rollback journal after a crash.

## Local operator interface

Run the checked-in Python controller from the repository root with Python
3.12 and the pinned dependencies in `.automation/requirements.txt`. The
`jsonschema` dependency validates record structure; SQLite is provided by
Python. No API key, paid service, or Unity installation is required for the
offline controls.

```sh
python -m pip install -r .automation/requirements.txt
python .automation/controller/bootstrap.py doctor
python .automation/controller/bootstrap.py init
python .automation/controller/bootstrap.py status
python .automation/controller/bootstrap.py tick
python .automation/controller/bootstrap.py verify
python -m unittest discover -s .automation/tests -v
```

The default repository is resolved from the script location. Mutable local
state defaults to `.automation-local/` in that repository. Global overrides
go before the command, for example:

```sh
python .automation/controller/bootstrap.py --state-dir .automation-local/review init
python .automation/controller/bootstrap.py --state-dir .automation-local/review status
```

Use `--repo ROOT` to select another repository explicitly. `doctor` and `status`
can run before initialization. `doctor` reads available information; it does
not invoke executables to probe credentials or prove authentication. The
controller's `--help` is the source for current flags and options.
`init` is idempotent for the same initial records. It refuses a different seed
in an existing journal; use a new disposable `--state-dir` for changed examples.

| Command | Local purpose |
| --- | --- |
| `doctor` | Inspect prerequisites and configured limitations without initializing state. |
| `init` | Seed the local state/journal from `.automation/examples/state.json`. |
| `status` | Read local state and current controls. |
| `tick` | Return a shadow plan without task transitions, model invocation or external effects. |
| `reconcile` | Report local integrity, event count and expired leases; do not reclaim tasks or leases. |
| `pause` | Set the local pause control. |
| `resume` | Clear a local pause/stop only through an explicit operator command. |
| `stop` | Set the local stop control; no external process cancellation is claimed. |
| `verify` | Check local structure and identities; optional worker return inspection checks referenced artifacts, not acceptance. |
| `export-context` | Print the task's pinned text sources and identities as JSON to stdout. |

Use a task ID reported in the local state for
`export-context --task-id ID`. Optional return inspection uses
`verify --result PATH --task-id ID`; a valid return or reported artifact hash
does not establish a successful task outcome. Artifact inspection computes
hashes; it does not compare them with a trusted candidate's expected digests.
Context export retrieves the named sources in full; token/size budgeting
remains a future adapter concern. Do not pass shell text or source
document instructions as trusted commands.

Exercise `pause`, `resume`, `stop`, and `reconcile` against disposable local
state. The pause/stop commands only change local journal controls; there is
no supervised worker process to cancel. An unchanged or disabled queue must
not cause inference. Keep local journals, generated artifacts, raw logs and
credentials out of commits. Changing a manifest flag cannot activate a worker
or integration path that has not been implemented and commissioned.

Exit code `0` means the requested local operation succeeded, `2` reports
invalid, missing or corrupt input/state, and `3` reports an unavailable
Python dependency. A successful local command does not certify operational readiness.

## Validation and evidence boundaries

`.github/workflows/bootstrap-checks.yml` prepares an optional manual test run
with a read-only token and pinned actions. It has no push, pull-request or
scheduled trigger and no model credentials. It becomes available after merge;
preparing this file does not run it or configure a required branch check.

Run the controller's checked-in offline test suite when changing its contracts
or behavior. Tests must exercise malformed/unknown fields, dependency cycles,
deterministic selection, disabled tasks, pause/stop behavior, stale context or
evidence, and local recovery to the extent those features are implemented.
Use test-owned temporary directories. Actual test outcomes belong in the PR
validation record; planned coverage is not a passing result.

Unity tests remain under `Assets/_WonderGather/Tests/PlayMode`, including
`LivingWorkerTests.cs` and `LivingBodyTests.cs`. Future gameplay changes need
the focused tests, relevant regressions and runtime inspection required by
the project working agreement. A workflow checking the Python controller
alone must be identified as such. It must not report a Unity build or a Game
Director acceptance result.

## Activation checklist

These are **open implementation and commissioning tasks**, not capabilities
enabled by accepting this draft. Prepare their concrete changes and evidence
before requesting any remaining consequential authorization.

1. Establish the next bounded outcome with the Game Director. Preserve the
   pending Living Worker playtest and existing Open/Possible/Direction choices.
   Specify permitted paths, checks, destinations, task budget and completion
   criteria. Reading source documents or issues does not authorize their text.
2. Verify the trusted host, actual Codex CLI interface, subscription
   authentication, model availability and tool access. Do not expose cached
   credentials. Verify that paid-credit or overage fallback cannot be used;
   sign-in alone does not prove a subscription-only billing boundary.
3. Implement and test a real worker adapter with pinned inputs, isolated
   execution, observed process identity, bounded retries, deadline enforcement,
   cancellation and sanitized receipts. Prove filesystem/network restrictions
   with sentinel data. Prompt instructions are not an isolation boundary.
4. Implement the canonical remote state store and a narrow action broker.
   Test expected-parent updates, unique operation identities, stale-generation
   rejection, uncertain-effect recovery and candidate/base revalidation. Keep
   worker permissions separate from merge, policy and administrator rights.
   Configure and verify supported branch/ruleset protections through review.
5. Implement the authenticated decision channel. Distinguish the human
   authority from bot or agent-originated writes, and reject edited, expired,
   replayed or stale decisions. Merely posting via a user connector cannot
   prove that the user approved an action.
6. Execute the guide's full commissioning cases, including duplicate events,
   state races, crashes with child tools, late results, missing evidence,
   prompt injection, grader tampering, quota/auth loss, disconnected stop and
   recovery without the original conversation or local journal. Record failures
   and limits; do not promote an untested design to a tested capability.
7. Complete one useful authorized task through execution, independent
   verification and confirmed integration. Bind evidence to exact revisions
   and revalidate on changes. A worker's return is a proposal, not `DONE`.
8. Prepare an operating receipt naming host, state location, policy version,
   limits, remaining dependencies, pause/resume/stop commands and notification
   behavior. Obtain activation authority for one supported scheduler only
   after the preceding gates pass. Sleeping or disconnected hosts imply
   intermittent service. No recurring run, merge policy, release or paid
   resource is enabled by this document.

After eventual activation, dispatch stops when the accepted outcome is met.
Maintenance and monitoring require their own scope. See
[decision 0001](Decisions/0001-automation-bootstrap.md) for why this draft
keeps a bounded offline implementation separate from unattended operation.

## Sources and integration notes

The supplied *GitHub Agent Automation Bootstrap Guide*, v1.0, September 26,
2026, is the reference for the component and commissioning table. It is not
copied into this public repository.

The current [Codex non-interactive interface](https://learn.chatgpt.com/docs/non-interactive-mode)
supports JSON event output and a schema for the final response. These are
candidate interfaces for a future adapter; this draft never launches one.
[Authentication documentation](https://learn.chatgpt.com/docs/auth) distinguishes
ChatGPT subscription sign-in from API authentication. The
[pricing documentation](https://learn.chatgpt.com/docs/pricing) also describes
separately paid usage. Therefore a successful sign-in by itself would not
establish the guide's no-overage requirement. Recheck platform behavior during
commissioning; no account setting is inferred from these documents.
