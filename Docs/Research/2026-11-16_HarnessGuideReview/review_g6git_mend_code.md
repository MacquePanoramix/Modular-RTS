# Review of the G6-git mend (reader)

Shown/read: profile_check.py, commit-msg, pre-push, pre-commit diff, install.sh, uninstall.sh, the guard.sh and guard_profile.py diffs, the report's mend section. Not read: test_git_hooks.sh and guard_test.sh beyond the header; the full guard_profile.py. The suites were not run (usage; context). One probe run (probe.sh in the reader folder).

## Findings, most serious first

1. profile_check.py:374-408 (kept_message), CONFIRMED by a run. commit-msg decides from the message text alone that git will drop comment lines, but with -m or -F the cleanup is whitespace and git keeps them. A -F file holding git's two template lines followed by a comment line carrying the profile path: commit exits 0, `git log` shows the line stored. Same class for a fake scissors triple (git cuts only with -v or cleanup=scissors; it does not cut with whitespace). The report's doubt 2 says "caught only at the push"; it is, but the commit already holds it. Fix: drop comment lines only if cleanup is known strip, or read everything and accept over-refusal. Sure.
2. profile_check.py:156, low. needles() runs at import, outside the fail-open try. An exception there (lone surrogate in a name under a codec encode) gives exit 1, which the hooks treat as a refusal, with no "skipped" line. Fail-closed, contrary to the stated rule. Fairly sure; not run.
3. profile_check.py:131-135, low. When SHGetKnownFolderPath fails, the note is shown only under WG_PROFILE_TRACE on success; the user sees nothing and the changed USERPROFILE gap is silently open.
4. Short 8.3 names (name~1) match only when the user name is at most 8 characters, because the pattern needs the full name; longer names would be missed. Not run.
5. next/guard_profile.py:442-443, low. The working-tree read relies on `git diff --name-only`, which is blind to assume-unchanged and skip-worktree paths. The git hook version (profile_check.py:292-330) is sound: it reads the working copy only if its git hash equals the staged oid, so the bytes are the blob's.
6. Possible false refusals: github.com/users/<name> URLs match (Users + separator + name).

## Answers
- Right boundary: (?!letter-or-digit) in each codec; a name followed by a separator, quote, line end or end of data is caught; chunk-edge cases only over-detect. OK.
- System-profile code: GUID and ctypes use look correct; exceptions are caught inside; the fallback is the environment only.
- Pass cases for the wrong reason, guard changes: the words()/noverify, config_writes, lfs and cp changes are limited to the commands they list by reading; none refused an ordinary command that I could see. Rule c refuses any commit/push/merge in a command that mentions HOME= or USERPROFILE= anywhere (the report's doubt 7). Not run.
- install.sh/uninstall.sh: no defect found.

Name check: the user name, built at run time, was searched for in this file and in probe.sh before returning: count 0 in this file (the probe builds it at run time from the environment).
