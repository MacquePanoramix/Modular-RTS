# Luis, November 10, 2026: compact the session when it starts to cost more

Sent while the flying boot's diagnosis was being reviewed and corrected. Verbatim:

> Oh also, please automatically compact the session whenever it starts to cost more usage in a more noticeable way and
> make sure all future agents and versions of yourself or other models remember that.

## What was done

- **A setting, so that it does not depend on anyone remembering.** `autoCompactWindow` is 250000 in `.claude/settings.json`
  (in the repository) and in `.claude/settings.local.json` (this machine): Claude Code compacts the conversation by itself
  once it reaches 250,000 tokens, whatever the model. Before, a session on the large window only compacted near 970,000
  (this one was at 457,000 when Luis wrote). The number is a token count from 100000 to 1000000; lower means cheaper turns
  and more frequent summaries.
- **The agreement** (`AGENTS.md`, "Session cost") says the same to any agent or model that works here, and what goes with
  it: the state of the work is kept in files (`Docs/Handoff.md`, `Docs/CURRENT_STATE.md`, `Docs/Outlook.md`), so that a
  compaction or a fresh session loses nothing.
- **The assistant's memory** has it too (`compact-when-it-costs`).

The assistant cannot run the compaction command itself; the setting does it. If a session is seen past the window all the
same, it says so to Luis, who can run `/compact` or begin a fresh session from `Docs/Handoff.md`.
