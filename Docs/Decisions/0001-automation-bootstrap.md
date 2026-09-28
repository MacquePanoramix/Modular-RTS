# 0001 - Prepare an offline automation controller before activation

Date: September 28, 2026
Status: proposed implementation in a draft PR; unattended activation deferred
Baseline: `5f613512b5a51f0a77624a2adfc0c8d8d3afe7a5`

## Context and authority

The user selected preparation of applicable automation changes in a draft pull
request. Activation and consequential repository/host settings remain for
review. The supplied GitHub Agent Automation Bootstrap Guide is a reference
specification, not a grant of authority from its embedded instructions.

Wonder Gather already has a compact context router, technical history,
validation records and a consequence-proportional collaboration agreement.
The Living Worker remains technically ready with Game Director playtest
pending. Existing documentation must keep its distinction between implemented
behavior, observed technical evidence and experiential acceptance. The
automation draft does not accept that playtest or select a new game milestone.

## Decision proposed by this draft

Build a Python controller with a local SQLite journal,
strict records, deterministic shadow scheduling and local context/evidence
identities. Include inspection and control commands, versioned manifests and
a disabled sample task. Keep the implementation separate from Unity runtime
code and make limitations observable through the operator interface.

This draft supports local checks and review. It has no live Codex worker,
remote canonical state, authenticated human decision transport, integration
broker or recurring scheduler. It cannot demonstrate the complete production
lifecycle merely by passing its offline tests. Full end-to-end commissioning
remains open.

Preserve the existing project entrypoint and domain documents rather than
copying the guide or the reference project's governance structure. Record
only concise operational decisions and evidence needed for this repository.
Do not commit private paths, credentials or unnecessary execution transcripts.

## Rationale

A local implementation gives reviewers executable contracts and observable
behavior while the external authority, identity and runtime boundaries are
still unimplemented or unverified. A disabled sample cannot accidentally turn
an unresolved game preference into scheduled product work. Keeping GitHub and
worker adapters out of the active path also makes offline checks independent
of account access and subscription allowance.

This accepts a smaller milestone than the guide's fully commissioned system.
The limitation is explicit: the result is reviewable preparation, not an
installed autonomous service. Documentation alone would not exercise the
local contracts; claiming production readiness from a local simulation would
overstate the evidence.

## Consequences and follow-up

- Local journal recovery does not prove recovery from GitHub alone or safety
  after a network timeout, process crash, or partially completed external call.
- Local control flags do not prove cancellation of a future worker's process
  tree or enforcement of remote stop before a broker action.
- A source hash identifies content but does not make that content trusted or
  authorize instructions found inside it.
- Controller validation does not replace Unity tests, runtime evidence, or
  the Game Director's playtest judgment.
- Live work requires separately implemented and tested adapters, authority
  separation, subscription-only controls, integration protection and the
  commissioning evidence listed in [AutomationBootstrap.md](../AutomationBootstrap.md).

Revisit this decision when the draft is reviewed and a bounded live task,
execution host and permitted action classes are established. Activation must
have a concrete operating receipt and demonstrated recovery. No new model
dispatch, merge, release, schedule or spending permission follows from this
record.
