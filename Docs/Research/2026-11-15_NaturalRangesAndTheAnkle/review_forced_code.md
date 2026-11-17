# Review of forced checks code (range e8bac96~1..HEAD)
Shown: diff of 6b84d78 and e8bac96, forced_checks.md, live_ex_*.log by grep, AnkleForced.cs partly. Not read: whole AnkleForced.cs line by line, research.md 4.2.
1. Off path: no line under Assets/_WonderGather/Scripts changed in the range (empty diff). Game with switches off is unchanged. Sure.
2. UnityEditor in Scripts: grep finds none. Sure.
3. .meta: only AnkleForced.cs.meta added; none deleted or recreated. Sure.
4. BAR_A/B/C lines are log text; the tests assert only "miners ran / 0 lost / 0 makes missed". A run where a bar FAILs still ends 1/1 passed (forced_down_c0b did). So the checks decide by a human reading the log, not by the test result; a lazy rig could pass with no event except via the read-back lines. Medium sure.
5. Static state: AnkleForced.TheBootForced sets OwnRiser.AnkleLimit/AnkleSpeed directly (lines 620-627), restored at 643 only on the normal path, not in a finally; AnkleMendArguments.Restore does not cover them. An assertion failure mid-loop leaks the switches to later tests in the run. Explicit tests, low risk. Once-said flags: forgotten at Save and Restore (OK). With -onceSaidKeep 1 the flag leaks by design.
6. Exercised() logic: sound; Long/Round honestly NOT_EXERCISED. Small only live exercised (stated).
No correctness bug found in FindRing's -liveRing 0 path (empty ring lists; downstream use of empty Legs not read, unsure).
