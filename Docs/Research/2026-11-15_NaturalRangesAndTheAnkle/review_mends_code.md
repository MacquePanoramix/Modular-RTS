# Review of the five mends (code), commit 262f4ca

Reviewer: did not write the code. Read only: `git -C D:/Dev/WG show 262f4ca` for OwnRiser.cs, OwnBody.cs, RiseAgent.cs, AnkleMend.cs, GettingUpByItsOwnTests.cs, CoverRingLiveTests.cs; the files at that commit; greps of the five short runs in D:/Dev/WG/TestRuns (ank_fix_on3, _bad, _Aalone, _limit2, _unheard2, and ank0_small_heard). Unity was not run and nothing was compiled by me.

## Verdict

The five mends do what they say, on a read of the code. With the switches at their defaults, none of the lines they changed alters what the game computes (traces below). No must-fix. Three should-fix or note items concern what the runs did and did not show (mend 4 never ran with anything to add; the once-flags are never reset; the run that proves mend 5 is only limit2).

## Mend by mend

1. AnkleSpeedHere (OwnRiser.cs ~549-566). Reads right.
   - `AnkleSpeed == 0` returns 0 first. A NaN speed fails `== 0`, gives a NaN product, fails `speed > 1e-3f`, so it says so once and returns 0. A negative value, a speed of 1e-6 or less, an infinite speed, and AnkleSpeedHeight 0 or negative (sqrt gives 0 or NaN) take the same route. `tall` is clamped to at least 0.1 in the constructor (line 324), so the division is finite.
   - Drive (line ~386), Aims (line ~412) and AnkleMended (`slows`) all ask this one function, so the three agree about "on". AnkleDamper gets a speed already above 1e-3, so the `has / speed` term cannot be endless.
   - Evidence: ank_fix_bad.log (speed 1E-06) and ank_fix_limit2.log (speed -3) each show the error line once. Both runs ended on their own (xml result Passed).
2. -ownUnheard accepts 0 or 1 only. In AnkleMendArguments.Apply (AnkleMend.cs:36-42) the Assert is the first statement, so a failure changes no static. Save() runs in UnitySetUp and Restore() in TearDown (CoverRingLiveTests.cs:56 and :79; AnkleMend.cs:417 and :432), so the failure is still restored. In GettingUpByItsOwnTests.cs (~55) the Assert comes after OwnBody.BootTurnsAtMost, ElbowFolds and KneeFolds are set, but TearDown puts those back to their constants (20, 145, 150) and RiseAgent.Unheard back to `unheardWas`. Nothing is left changed. Evidence: ank_fix_unheard2 with `-ownUnheard 2` failed with "takes 0 ... not 2." in the xml.
3. The AnkleTurnsAtMost warning, once, with AnkleLimit 1. It sits inside `if (limits)`, so it can never be reached on the default path. NaN fails `> 0` and stays silent, which is right. Evidence: ank_fix_bad.log shows the line once (value 5).
4. The learnt addition as `own * FromRotationVector(drivenNow[j])`. The units match: drivenNow is degrees per axis and FromRotationVector reads its magnitude as degrees (AngleAxis). It is reached only when AnkleLimit == 1. With drivenNow zero it is the identity quaternion, and `own * identity` returns own exactly. (Before, the round trip through a vector slightly changed own. This only matters on the AnkleLimit 1 path.) The old and new forms agree to first order for small turns and differ by a commutator. The note in the code says that which order is right is open; I agree.
5. Said(float) uses InvariantCulture. Correct. NaN prints as "NaN" and 1E-06 as "1E-06". Evidence: ank_fix_limit2.log (04:11) prints "0.605714" with a point; ank_fix_bad.log (04:08) prints "0,605714" with a comma, so it was built before this mend.

## Default path, bit for bit

(AnkleLimit 0, AnkleSpeed 0, AnkleTurnsAtMost 0, BootsFromOwnShin false.)
- Drive, line ~386: `if (j >= Foot) { speed = AnkleSpeedHere(); if (speed > 0) ... }`. AnkleSpeedHere returns 0 at once, so `damper` is untouched. This costs one call and no float result changes. (-0 also equals 0.)
- Aims, line ~412: the guard `(AnkleLimit != 0 || AnkleSpeed != 0)` is false, so `&&` short-circuits before AnkleMended and the old line `AnkleCan` runs unchanged. So mends 3, 4 and 5 (all inside AnkleMended, or in logging) cannot run.
- If a bad value is set (AnkleLimit 2, or AnkleSpeed NaN or negative), AnkleMended returns false whenever neither `limits` nor `slows` holds. Aims then goes on as before. I read this, and ank_fix_limit2 agrees: ANKLE_DONE shows the same size of target steps as the default form (most 11.5 against 11.4 for A alone), which shows only that nothing blew up, not equality.
- Not a mend, but a change on the default game: RiseAgent.Unheard now defaults to true (RiseAgent.cs:105-112). It is a stated, requested change (Luis, Nov 15), but it is not "every switch at default, the same as before": the game's get-up differs from the previous commit, for Small, whenever the model had something to add.

## Findings

1. note, should-fix before it is leaned on: mend 4 was never run with anything to add. Every short run has `RiseAgent.Unheard True`, so `drivenNow` is zero and `own * identity` is the only branch exercised. ank0_small_heard.log, despite its name, also shows `RiseAgent.Unheard True` and `AnkleLimit 1`. The nonzero branch (the order of the composition, and that nothing jumps at half a turn, which was the reason for the mend) is read, not run. Fix: one heard run, `-ownUnheard 0 -riseSet AnkleLimit=1`, with a jump count like ANKLE_JUMP.
2. should-fix (tests, small): the three once-flags (`ankleLimitSaid`, `ankleSpeedSaid`, `ankleTurnsSaid`, OwnRiser.cs:147) are static and never reset, and AnkleMendArguments.Restore does not touch them. In one editor session without a domain reload (or with domain reload off), the second test that sets a bad value prints nothing, and a check that looks for the line passes or fails wrongly. The runs above were one process each, so they are not hit; and the tests set `LogAssert.ignoreFailingMessages = true`, so nothing asserts on the message. In the game this is fine ("once" is the intent). Fix if wanted: an `internal static void ForgetSaid()` called from Restore, or reset in `[RuntimeInitializeOnLoadMethod]`.
3. note: each flag is set on the first message and a later different bad value is silent (the speed error says the speed, so a changed speed is not reported again). Fine for the stated "once".
4. note: nothing is logged every step: all three messages are behind a static flag, and `Said` is called only inside those branches. The counts of 1 in ank_fix_bad.log and ank_fix_limit2.log match.
5. note: no compile risk seen. `dofCount` and `jointPosition` were already used in earlier code and compiled in the runs; the runs of 04:07 to 04:11 post-date or equal the last edit of OwnRiser.cs (04:10:00) except on3 and bad, which predate it by up to three minutes. The last two runs (Aalone, limit2) are the ones that carry mends 3 to 5; on3 and bad show the older text (comma). No run ran the file exactly as committed at 04:12:32 unless nothing changed after 04:10:00 (the file's modification time is 04:10:00, so it probably is the same).
6. note: the NaN compare cases (AnkleLimit NaN: `!= 0` true, `== 1` false, so said once and acts as 0; AnkleSpeed NaN, handled by 1) are right. An Assert that leaves a static changed: none found (above).

## Not checked

- Nothing was run or compiled by me. The runs were read only through grep of lines starting ANKLE_ and the error lines.
- OwnBody.cs BootsFromOwnShin (switch C) was glanced at and is outside the question. Its default is false and every added line is under `refits` or an `if (BootsFromOwnShin ...)`.
- No courses, nothing as played (-ownAsPlayed 1), nothing on the meadow or at another strength. Bit-for-bit sameness is argued from the code, not shown by two runs compared tick by tick.
