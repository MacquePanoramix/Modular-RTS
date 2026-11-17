# The cold-entry brief

Given to a fresh Opus 5.5 agent (medium effort) that was told nothing else. For a new test, change only the name of the file it writes.

You are a fresh Claude Opus 5.5 session that has just been made the master agent of this Unity project (Wonder Gather). Nobody is available to answer questions: no author, no director. This is a cold-entry test of the project's own entry pages, so behave exactly as a real new session would. Enter by the project's ordinary route: AGENTS.md (already loaded for you), then whatever those pages tell a new session to read, in the order they say. The assistant's memory index, which a real session has loaded, is '(the memory directory)/MEMORY.md'; read it, and open a memory file only when you need it. Do NOT read anything under 'Docs/Research/2026-11-16_HarnessGuideReview/': it is about this test. One side task (another agent reviewing a friend's process guide) is in flight and is not yours; ignore it. Change nothing, commit nothing, start no Unity.

Keep a tally as you go: each file opened, roughly how many lines of it you read, and why you opened it.

Then write 'Docs/Research/2026-11-16_HarnessGuideReview/cold_entry_after.md' (start the file early, at most about 150 lines) with:
1. The goal of the current milestone, in two sentences. Evidence as file:line.
2. The exact next action you would take as master, as concrete steps or commands, and what would tell you it is finished. Evidence as file:line. Your confidence, and what makes you unsure.
3. What is waiting on Luis right now, and what he is waiting on from you.
4. What must not be touched, and the standing holds. For each: who set it, why, and what would lift it, if the pages say.
5. Luis's decisions of the last week that bind the next step.
6. What is unfinished or in flight, and anything left running or left behind. Check the actual state with read-only commands (git status and the last commits in this project and in the worktree D:/Dev/WG; whether a Unity process is running) and say where the pages and the actual state differ.
7. Known failing tests or known-red items, and where they are tracked.
8. The entry itself, which matters most: your reading tally; every place where a page gave an old instruction that reads as current, where two pages disagreed, where you had to hunt, where a page was too long to read as told, or where you were unsure which statement was the latest. Quote the line numbers. Then say what you could NOT determine, and what a short page would have needed to say to save you the hunt.

Do not fix anything. Your final message: the file's path and a summary of at most 8 lines.
