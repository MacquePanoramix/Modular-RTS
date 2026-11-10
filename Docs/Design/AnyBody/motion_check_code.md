# Check of motion_design.md against the code

Checker: the code part (one of two; the control theory is the lead's). November 10, 2026. Read on the work branch
`D:/Dev/WG` (`claude/wip-round`) at HEAD `5e9a4a0`. The design cites `53f388d`. Between the two, only
`Body/StanceFitter.cs`, `Body/StanceProblem.cs` and `Tests/PlayMode/StanceAtlasDrawnCapture.cs` changed. Nothing was
run in Unity. Paths are under `Assets/_WonderGather/Scripts/Units/` unless named otherwise.

Grades: **sure** (read in the code, line quoted), **likely**, **question**.

## A. Claims refuted or shaky, worst consequence first

1. **sure: layer 0 has never been on in the game.** `OwnBody.cs:203` `public static bool Anatomical;` defaults to
   false. Outside OwnBody, the only place that sets it is `Tests/PlayMode/OwnBodyTests.cs:26` (`-ownAnatomical 1`).
   `Make` uses it at `:742` (`bool anatomical = Anatomical && !down;`). In the game as played, then, no body (standing,
   lying, or remade) has frames, `Stops`, `WriteMade` or an `Aim`. The design's "layer 0 ... used" (§0) and its
   "`Remake` makes it the standing body Luis knows" (§2.10) hold only because the switch is off. Making the lying body
   anatomical also hands the keeper an `Aim` (`OwnBody.cs:862`, `Aim = Aim`), and the keeper then sends its targets
   through `JointAim` (`OwnKeeper.cs:503`). That keeper, from the hand-over until `Remake`, is one Luis has never played
   (question: did any `-ownAnatomical 1` run of the keeper pass?).
2. **sure: the present riser's pieces only write targets for the made-pose body.** `OwnRiser.Aims` (:390-405) turns
   a turn "from how it was made" into `angle * axis` and writes it with `SetDriveTarget`. OwnRiser never uses
   `JointAim`. Every piece §2.2 reuses (`ArmFrom` :644, `LegFrom` :527, `AnkleFrom` :531, `KneeFrom`, `HoldPlanted`
   :1268) ends in `Aims`. On a body whose drive zero is `frames[i].Zero` (`OwnBody.cs:764`), they would ask for the
   wrong turns. Reusing them needs a new output stage. They are also position-target IK: `HoldPlanted` moves a
   virtual hip pose (`hipsTo`, :1308) and asks each planted limb's joints to reach from there (:1325-1331). It is not
   a ground push. Only `Balance` (:1379) computes pushes, and it gives them through `AddTorque` (:1490-1491).
3. **sure: the lift gate in `ShiftedOff` is not the design's.** Line :1216:
   `bool over = depth >= ... && (limb < BalanceLifts || shiftT >= BalanceUnloads) && shiftT >= .5f * BalanceUnloads;`.
   `limb` is a sum of `leanFelt`, which is a share of the **whole body's weight**, not of "what it bore" (§2.4).
   After 0.429 s (`BalanceUnloads`), the clock stands in for the unloading, and there is a 0.21 s minimum. §2.4's gate
   is new work, not "as `Balance` does".
4. **sure: `Statics.ReleaseKinds` (Statics.cs:81-100) has no per-support let-go.** It works per family and drops a
   whole kind at once (`Array.IndexOf(drop, ...)` in `Report`: both hands together). It returns nothing for `quad`,
   `tripod`, `stand` and `onekneel*`, and nothing ever drops a knee, shin, sole or toes. §1.2 says nodes carry "for
   each support it may let go (`ReleaseKinds`) S and the margin without it", but that data does not exist. It has to be
   generated with `Build(..., keep)` (:22-31), which does support it. Lifting one knee from hands and knees (the first
   edge of the route through one knee) has no release data today.
5. **sure: what bears a lying body is not sensed.** Touches are on the boots (`OwnBody.cs:804`), the forearms, thighs
   and shins (`:807`), and the toes (`:809-835`). The trunk, hips, head and upper arms have none. On its front the chest
   bears most of the weight, and on its back the trunk and hips do. "What bears it now" (§1.1 item 3) and the seat as a
   support (§2.4) need new touches. `GroundTouch` (:28-33) also keeps only `impulse.y`, so "within friction" (§2.2
   step 3) has no measured tangential force.
6. **sure: the legs are one muscle group, and there is no neck group.** `PhysicalBody.cs:56`:
   `enum Muscles { LeftArm, RightArm, Back, Legs }`. Tiredness cannot make one leg weaker, as §1.3's "a weaker leg"
   requires, and it cannot give per-joint fresh shares (§1.1 item 4, §2.3 τcap). The paces are small: `Tires .04`
   (:58) spends about 14% of a group in a 3.5 s all-out rise, so "a second try is weaker" (§1.6) will be slight (likely).
7. **sure: the effort precedent is cited wrongly.** §2.6 says S_j is "read from its drive's force (as the keeper's rise
   reads its legs': `OwnKeeper.cs:648-664`)". The keeper reads `asked[j]` (:649), its own `AddTorque` sum before the
   clamp (:1079), not the drive's force. The drive-force reading already exists elsewhere: `OwnBody.cs:1053-1064`
   (`parts[j].driveForce`, which gives `Exertion` and feeds `MinerBreath.cs:66`). That is the piece to reuse.
8. **sure: `GroundAt` (OwnRiser.cs:326) returns only `below.point.y`.** It drops the normal, and its ray starts 0.5 m
   above and runs 4 m (both in metres). "So the slope under it is known" (§1.1 item 3) is false today.
9. **sure: `Choose` (:447; the design's :455 is its `leads` line) uses a line, not a weight.** It takes the first way
   whose band holds the facing (`front >= one.frontLeast && front <= one.frontMost`). It measures how far onto a side
   only in the roll's `OtherWayUnder` 0.5 (:456).
10. **sure: Long and Round have no present way.** Only `Resources/RiseWays/Small.txt` exists (`OwnBody.WaysUp`
    :289-305). `PhysicalFall.ByItsOwn` returns false at `:801`
    (`if (!itsOwn.KnowsAWayUpFrom(front)) ... "no way up for lying so"`), and the old animation follows (`Rouse`, :980).
    §3.1's "onto its back or a side it gets up as before" means the backup animation for Long and Round on most falls
    (see C). The new switch must also get past this gate for Long and Round.
11. **sure: the fitter is newer than the design.** At HEAD, `FitOptions` already has `RestWeight` (a resting hand
    pulled per metre, :39) and `Seed` (each side varied, :43), from commit 13e4a53. §1.5's "at plan time each side is
    drawn on its own" partly exists. Its cite `StanceFitter.cs:9-44` reached `FitResult` at 53f388d. `RestWeight` is in
    metres.
12. **sure, minor cites.** `Measure` is at :328 (cited :334, its `Front` line). `StanceLibrary` starts at
    `Stances.cs:50`, and :528-579 is `Make` and `PrototypeBatch`. `AtlasHands` and `AtlasPoser` are now at
    `StanceAtlasDrawnCapture.cs:253` and `:299` (+12 lines). `StiffAnkleHolds` does not exist yet (§2.9 reads as if it
    does; §3.2 step 0 makes it). The `Strike` pairs (OwnBody.cs:587-595) let the forearm strike the trunk, the head
    **and the shins and boots** (`Shinbone(b) ... || Lower(a)`), not "only the trunk and the head" (§2.1). It never
    strikes a thigh, which confirms the needed change. Of the clocks §1.7 removes, two remain unnamed: `stage.within`
    (:831, `got > stage.within`) and `ReleaseWaitsAtMost` 4 s (:1190, :857).
13. **likely: `ArmFrom`'s elbow pole is set for prone.** It is `-Lies() + .5 * outward` (:652), "its elbow bends back,
    towards its feet". For a hand resting on a thigh while squatting or kneeling upright (§2.7), that pole points the
    elbow backwards and down, which may not be how such an arm hangs.

## B. Citations confirmed (line right, behaviour as said)

- PhysicalFall: ranges :40-42 (shoulder twist 70, aside 95); `AtRest` .05 :47; `LiesAfter` .5 :58 and :964;
  `LiesAtLeast` .8 :74; shaken, dazed and out of breath :95-97; `HowLongItLies` :99-104; `TriesByItsOwn` 12 :778;
  `RestsAfterATry` .7 :779 (capped at 5 tries, :964); `ByItsOwn` :788-814, called only after `lain >= liesFor` (:977-979).
- OwnBody: `Make` :637; `anatomical` :742; `maxAngularVelocity = 20` :751; floating root :753; the drive at :780 with
  `forceLimit = strength[i]`, no stops (`HingesStop` false :195); leaning touches :806-807; toes :809-835; seed from
  the name :850; `Stops` :414-425; `WriteMade` :431-447 (needs `Aim`); `SpeedOf` :215 (√(0.6/H)); `OnItsFeet` :603-622
  (`keeper.Soften()` :613); `Remake` :626-633; `GoDown` :1268-1303; breath to the riser :1071; hand capsule
  `Rod(Forearm, len + arm)` :706.
- OwnKeeper: `DownLeaning` 35 :26; the spring note :37-40; `RiseOnFeet` .85 and `RiseSettles` 0 :193; `Soften`
  :220-229 (`forceLimit = strength`); down test :607-608; legs' effort pace :645-664; `Outside < .01f` :653; leg
  torques by `AddTorque`, each `ClampMagnitude(torque, strength)` :1077-1083. Beside a drive with the same
  `forceLimit`, that allows up to 2x strength (confirmed).
- OwnRiser: `FullAt` 35 :115; `BalanceBorne` .02 :152; `BalanceLifts` .15 :156 (but see A3); `BalanceHoldLearns`
  :174; `StuckAfter` 3 s :752; pose-time gates :808-834; breath on the trunk :852; `Outline` (2-D hull) :970-990; the
  79% / 54% note :1180-1184; "all the same" after 2.5 s :1218; `Plant` :1226; `HoldPlanted` :1268-1332; `Shares`
  (3x3 Cramer, even split, cop constraint) :1336-1373; `Balance` :1379-1497 (torques from the ground's pushes only,
  no weight of the parts beyond), nearest deep-enough place :1399-1425; `AddTorque` within what the spring leaves
  :1490-1491. `HandEnd` :636.
- Others: `GroundTouch.cs:26` (`StillOnly && touch.body != null` ignores moving bodies); `JointFrames.cs` ranges
  :27-41, `Clamp` :111-126, `Box` :158-172 (±2 degrees about the made pose), `JointAim.Target` :193-198 (clamped, no
  slew); `OfOwnBody` has 13 frames and none for the toes (question for §2.1). `PhysicalBody.Fresh` :67 and `Worked`
  :71; the own body never calls `Worked` (the callers are MinerWorkPreview, PhysicalBack, PhysicalBalance and
  PhysicalHands). `Statics.Build` with `keep` :22-31. `ReleaseKinds` :81-100 and `Report` (to :150) behave as in A4.
  `Targets.HandOnThigh` :178-203 (defaults 0.6-0.95; the stances use 0.65-0.97 and 0.5-0.9). `Sym` at
  `Stances.cs:89, 110, 152, 178`. `ArticulationFactsTests` 9 of 9; the gravity forces are tested only on
  `root.immovable = true` (:35). `BodyFrame` :9 (test assembly; copies `HipGives`, `AnkleGives` and `NeckHolds` as
  constants, :11) and `MadeBodies` :24. `MinerWorkPreview.cs:640` (`stepsWanted = true`).

## C. Numbers in metres, or tuned on Small, that the design would reuse

- OwnRiser (searched on Small, October 14-29; the only ways are Small's): `BalanceMargin` .021 m, `BalanceShiftsAt`
  .122 m/s, `BalanceShiftsAtMost` .04 m, `BalanceShiftsDeep` .04 m, `BalanceLeansAtMost` .12 m, `BalanceLeans` .6,
  `BalanceQuick` 3.36 /s and `BalanceDamps` .19 s (seconds, not √(H/g)), `BalanceUnloads` .429 s, `BalanceFeels`
  .08 s, `WandersBy` .012 m (:117-174, :298). `StillUnder` .2 m/s (:137), `GoesOnBelow` .12 m/s (:745), `stepLifts`
  .06 m (:52), `handLift` .05 m (:71), the boot pressed .008/.03 m (:496), the kneel bump .02 m and fallback .08 m
  (:602, :612), the IK clamps .01 m (:538, :652), and the `GroundAt` ray (:326). Shares that are body units but were
  searched on Small: `KneeFaces` .759, `FoldedUnder` .163, `ArmStraight` .985, `RuleTimes` 10.
- OwnKeeper (searched on the three miners, on a level floor): `Comfort` .033 m, `KeepsBehind` .021 m, `RiseLeads`
  .03 m, the 1 cm `Outside` (:653). §2.10's gate uses these as they are, which is fine for the three miners and
  unknown for made bodies.
- PhysicalFall: the lying time depends on `BlowLight` 1 / `BlowHard` 6 m/s and `HeadLight` 2.5 / `HeadHard` 6 m/s, and
  stillness is `Still` .2 m/s (:58-75). A big body falls faster, so it reads as more shaken. Under §2.7 the "lying
  time" becomes tone recovery and inherits this.
- `JointRange.Speed` is set "for a body whose hips stand 0.6 m high (Small)" (JointFrames.cs:22-25); `SpeedOf` scales it.
- `FitOptions.RestWeight` is per metre (A11). `Statics.BaseMargin` is in metres; `MReq` is 0.03 H (body units).

## D. Measured from what exists (no new runs)

**(1) How a fall starts, and how bodies lie.** The panel's "Push it over" (`MinerPanel.cs:61`) calls
`MinerWorkPreview.PushOver()` (:678-696). That pushes the trunk's centre of mass along **the camera's horizontal
forward**, 6 x the mass (N) for 0.3 s (`ShoveEach`, `ShoveFor` :594; applied in `FixedUpdate` :702). So which way a body
falls relative to its own facing depends on where Luis's camera stands. The code cannot tell that.

The recorded facing (trunk front against down, at the start of each try):

- The game test as played (`GettingUpByItsOwnTests`, Small on the meadow, pushes at 0, 90, 180 and 270 degrees from
  its facing ±20). There are 620 `OWN_UP push` lines in the earlier session's logs, 615 with a try. At the first try:
  - straight ahead (155): front > 0.4 in 26 (17%), 0 to 0.4 in 96 (62%: crouched forward, chest not down), back in 22.
  - pushed from behind (155): 150 on the back.
  - sideways (305): mostly back (167), 0 to 0.4 in 85, front > 0.4 in 42.
  - all 615: **front > 0.4 in 11%**, -0.4 to 0.4 in 33%, back (≤ -0.4) in 56%. Over all 4,547 tries, retries
    included: front 35%, side 21%, back 44%.
- The training falls (RiseSchool, the same push of 6 x weight for 0.3 s, four ways ±40 degrees): 89,385 first tries
  (63,245 distinct), with 56% at front ≤ -0.8 and 22% at front > 0.4.
- Luis's own play: on October 21-22 Small "pushed over in the game, landing on its back or side ... every push ended
  with the old getting up" (`Docs/Correspondence/2026-10-22_NEVER_THE_BACKUP_ANIMATION.md:5-6`). Validation.md:4517
  gives "the game's tries on their backs 45%".

Share covered by a first build from the front only: with pushes from all directions equally often, **about 11% of
falls** (front > 0.4, the present riser's own line), or about 41% if "front" means > 0. With the camera behind the
miner, pushes straight ahead give 17%, or 79% if front > 0. Luis's camera habits are not recorded, so his share cannot
be told. Either way, the commonest result is lying on the back, and the second is a forward crouch at front 0.2-0.4,
which is not a prone stance. The design's "falls onto the front (the commonest)" (Q1) is **refuted** by these records
(sure for the tests, likely for Luis's play).

**(2) What the present riser does first, from each way of lying** (`Resources/RiseWays/Small.txt`, Small only):

- front ≥ 0.4: way `feet`: `hands` → `under` → `prop` → `stay` → `footA` → `footB` → `pike` → `rollup` (hands and knees,
  feet stepped under, a pike, a roll-up through a squat), then the hand-over.
- front < 0.4 (on its back, a side, or the forward crouch): way `roll` (front -1 to 0.4): `plant` → `push` → `front`
  (gate `front>0.6 still>0.25`). **It rolls onto its front first.** At the end `Choose` runs again (OwnRiser.cs:829),
  finds `feet`, and goes on (at most 4 choices, :451). The uppermost side leads (:455), or the other side after a failed
  roll if `RollsTheOtherWay` (off by default, OwnBody.cs:224).
- Long and Round: no way, so the old animation at every fall (A10).

So the present riser comes to the front from every way of lying. A first build "from the front" could take the body
from there, after the present roll. The design does not say whether layers 2 and 3 may take a body that the old riser
rolled onto its front (question).
