# Luis, November 13, 2026: fix the solver's turns in the game, and the week's limit

## Afternoon, answering the report on the solver's turns

The report: the miners' own bodies run the physics solver at 6 and 1 turns (Unity's defaults), not the 14 and 4 their
code sets, because turns set on a body's root before its other parts are attached are lost (`OwnBody.cs:760`). Two
decisions were put to him. First, whether to fix the turns in the game; the recommendation was yes, behind a switch, so
he can play the step and the getting up both ways before choosing. Second, whether to spend what was left of the week's
usage (93%) on the independent review now, or wait for the reset on Monday, November 16.

> 1. Yes please.
> 2. Oh you don't need to worry too much about this weekly limit since we have a reset this week which I'm planning to
>    use after this limit is full. So we will have another full 100% to be used weekly limit after that which resets on
>    Monday.

What follows from it:

- **The turns are fixed in the game behind a switch**, so that both can be played and compared. The decision between
  them stays his.
- **The review is made now**, not after the reset. When the week's limit is full he will use a reset he has, which gives
  a new full week's limit until Monday. The five-hour window still applies.
