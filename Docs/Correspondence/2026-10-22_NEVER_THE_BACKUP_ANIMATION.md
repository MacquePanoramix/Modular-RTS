# Never the backup animation — October 22, 2026

**From:** Luis (Game Director), answering the report of October 21-22: the
getting up by its own body in the game behind a switch (off), and Small,
pushed over in the game, landing on its back or side, where it has no way yet,
so every push ended with the old getting up.

## The message (verbatim)

> Okay thanks! Now I would like for the Hi-Fi prototype for us to try make it so we never have to use the backup animation. If that is really really not possible then we should improve the backup animation so it feels natural and more like a mix of the fully procedual one with a backup ensurance that looks super natural also. Anyways what do you suggest now as the next step?

## What it says

| | Luis | What follows |
|---|---|---|
| The aim | "make it so we never have to use the backup animation" | **Direction from Luis:** in the high-fidelity prototype a miner always gets up by its own body; the old posed getting up is not used |
| If not possible | "improve the backup animation so it feels natural and more like a mix of the fully procedual one with a backup ensurance that looks super natural" | A fallback, if one must exist, is mostly the procedural getting up, with an assurance that it succeeds which itself looks natural |
| Next | "what do you suggest now as the next step?" | Answered in the session (the suggestion is in [the body's own](../Design/TheBodysOwn.md)) |

## Luis's answer to the suggestion (verbatim)

The suggestion: a way up from the back and the sides (rolling onto its front first);
then graduated assurance (its own; trying again; from the third try, more effort
through its own joints); and one question, the very last resort: a gentle hidden
push at the hips (natural to look at, but against the rule that nothing pushes the
body from nowhere), or extra tries instead.

> Yeah I prefer the to allow the extra tries. It should be absolute last resort the unnatural asistance.

| | Luis | What follows |
|---|---|---|
| The last resort | "I prefer the to allow the extra tries" | **Decided by Luis:** a miner that fails tries again, as often as it needs, before anything helps it from outside its body |
| Unnatural assistance | "It should be absolute last resort" | A push from nowhere only if nothing else will do, and never before the extra tries |

## Luis, after the report of October 22 (evening) (verbatim)

The report: the roll from the back works on the bench and the judges read it as
real with a flaw, but the getting up still fails rolling up from hands and feet
(in the game, 11 tries in 300 s, none up); the suggestion was to end it in the
crouch the standing body already rises from. Luis's answer, with a screenshot of
the three side sessions in the sidebar:

> Can you do before we decide on a approach do a deeper research of all possible options? Really consider all but still with the previous rules of the body being responsible fully to the act of getting up. Also did these agents finish their task they were doing on the side?

| | Luis | What follows |
|---|---|---|
| The approach | "before we decide on a approach do a deeper research of all possible options" | **Not decided:** no approach is chosen until the options are researched and shown to him |
| The rule | "with the previous rules of the body being responsible fully to the act of getting up" | **Kept:** every option is weighed by whether the body's own joints, against the ground, do all of it |
| The side sessions | "did these agents finish their task" | Answered in the session: both finished, committed on their own branches, not merged |

## Luis, after the research (October 22, night) (verbatim)

The research report ([Getting up by its own joints](../Research/reports/Getting%20up%20by%20its%20own%20joints.md))
ended with three questions only Luis could answer: help that exists only while
training; changes to the body; a one-hand-first release and a hand on the knee.
His answer, with a screenshot of the side sessions:

> 1. Yes, what we care about is the game results to not receive help. We can arrive there however necessary.
> 2. Yes they are, although they should all be checked by a team of aesthetic agents following the aesthetic guide of the game that is established.
> 3. I guess so? I don't see why not to be honest.
>
> Also, do you think, if you think it's reasonable, can you merge the work that these other agents had done here? Let me know if you have any more questions.

| | Luis | What follows |
|---|---|---|
| Help while training | "what we care about is the game results to not receive help. We can arrive there however necessary" | **Decided by Luis:** help that exists only while searching or training (extra strength later taken away, a lift reduced to nothing) is allowed; in the game the body receives none |
| Changes to the body | "Yes they are, although they should all be checked by a team of aesthetic agents following the aesthetic guide of the game" | **Decided by Luis:** a smaller head share, more ankle range, a longer heel or broader sole are open; each is checked by a team of aesthetic judges against the game's established aesthetic guide before it stays |
| One hand first, a hand on the knee | "I guess so? I don't see why not" | Allowed, as a way to try |
| The side sessions' work | "if you think it's reasonable, can you merge the work" | Merged by hand where the two branches touch the same code (below, and in the validation history) |

## Luis, after the merge and the first test (October 22, night) (verbatim)

The report: the side work merged and pushed (the whole suite passes); the first
test of the research's first path (every try that let its hands go with its
weight outside its boots failed; all that stood let go with it over them); next,
the hands kept down until the weight is over the boots, helped by the toddler's
ways (hips back, a hand walked nearer, one hand first, the other on the knee),
then what tips over a third of the hand-overs.

> Okay sounds perfect to me. You can proceed please.

## Luis, on the third path (October 22, late night) (verbatim)

The report: hands kept down until the weight is over the boots, and raised legs
first, three quarters of those handed over stand; but only a third come to the
hand-over, and searched poses have reached their ceiling there. The question: a
learned controller for that one moment (the third path of the research; Unity's
ML-Agents and a Python trainer to be downloaded, a major new tool), or a
hand-built one.

> I approve we try the third path and you do the downloads then.

| | Luis | What follows |
|---|---|---|
| The approach | "we try the third path" | **Decided by Luis:** a learned controller for the moment from hands and feet to the squat, trained offline; in the game it only sets the body's joint targets |
| The downloads | "you do the downloads" | **Approved by Luis:** Unity's ML-Agents package (work branch only) and its Python trainer with what it needs |

## Luis, the next morning (October 23) (verbatim)

After the night's save ("save your progress for now I need to turn off laptop to sleep", October 22, late):

> You can proceed now please again.

## Luis, October 26 (verbatim)

After the session of October 23 ended (the laptop off) with the fourth teaching run (from falls as in the game) at
650,000 decisions:

> Please proceed.

## Luis, on the learned controller's next step (October 26) (verbatim)

The report: the learned nudges more than doubled standing from neat front starts but not from falls as in the game
(7 of 72 with or without them); three options: (1) a learned controller with full control of the joints at the hard
moment (the pike and the roll-up), taught with the help allowed only while learning (extra strength and a lift, both
taken away), on falls as in the game; (2) the last stage redesigned by hand, the toddler's way; (3) the getting up
put aside for now.

> option 1

| | Luis | What follows |
|---|---|---|
| The approach | "option 1" | **Decided by Luis:** a learned controller sets every joint's target itself through the pike and the roll-up, taught on falls as in the game, helped while it learns (more strength, a lift) and that help taken away to none; in the game, its own joints only |
