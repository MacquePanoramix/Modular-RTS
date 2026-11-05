# Layer 1 in the game, first step: the stance atlas in pictures

Status: a design, November 4, 2026. Nothing here is built, nothing under `Assets` or `ProjectSettings` was changed, and
Unity was not run. Work branch `claude/wip-round` at `f303d3f`. The port's independent review (`port/REVIEW.md`) was
still running while this was written, and its fixes were uncommitted in `port/src` (`AD.cs`, `Effort.cs`,
`Kinematics.cs`, `Margin.cs`, `ReserveSolver.cs`). Game files are cited by line at `f303d3f`. Port files the review is
editing are cited by member name instead.

## 0. What this step is for

**Pictures for the Director.** Every stance of the port's library (`StanceLibrary.PrototypeBatch`, 45 stances) is fitted
to Small, Long and Round and to ten bodies made between and beyond them (§2.6). Each is seen from four sides:

- as the body's own solids, for every body;
- as the drawn miner, for every body whose lengths are a miner's;
- with r, the worst joint and the margins printed.

The Director judges whether the stances look natural, or robotic and repetitive. The aesthetic judges see them first.
This is evidence step 2 of `Docs/Design/AnyBody/stances_strategy.md` §5, and layer 3 starts only after it.

**What changes in the game: nothing.** This step adds:

- one engine-free assembly that nothing in play calls;
- a read-only bridge;
- test and bench code;
- a Python sheet script.

It also makes eight private constants of `OwnBody` and `PhysicalBody` internal (§2.1), which changes no behaviour.

There is no new switch, because nothing new runs in play.

## 1. One source

### 1.1 Where

```
Assets/_WonderGather/Scripts/Units/Rising/
  AnyBody/WonderGather.AnyBody.asmdef     the port, engine-free (noEngineReferences)
  AnyBody/Core/*.cs  AnyBody/Body/*.cs  AnyBody/Reserve/*.cs     (21 files, moved from port/src)
  BodyFrame.cs                            the bridge (§2): WonderGather.Runtime, namespace WonderGather
```

**Why this place.**

- Strategy §5 names `Scripts/Units/Rising/` for the runtime modules, and gameplay belongs under `Assets/_WonderGather`.
- A nested asmdef takes the folder out of `WonderGather.Runtime`, which otherwise claims everything under `Scripts/`
  (`Scripts/WonderGather.Runtime.asmdef`).
- With `noEngineReferences` set, Unity itself enforces "no UnityEngine".

**One asmdef, not three.** Unity then compiles one assembly. The console keeps three projects, which is stricter:
Reserve cannot see Body there. No type name is used twice across Core, Body and Reserve (checked), so the merge
compiles.

**Rejected:**

- a DLL built by `dotnet` and dropped into Assets: it goes stale, and the source would not be in the game;
- a local package under `Packages/`: gameplay belongs under `Assets/_WonderGather`.

```json
{ "name": "WonderGather.AnyBody", "rootNamespace": "WonderGather.AnyBody", "references": [],
  "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false,
  "precompiledReferences": [], "autoReferenced": false, "defineConstraints": [], "versionDefines": [],
  "noEngineReferences": true }
```

Two asmdefs gain the reference `"WonderGather.AnyBody"`:

- `Tests/PlayMode/WonderGather.PlayModeTests.asmdef`;
- `Scripts/WonderGather.Runtime.asmdef`, for `BodyFrame` only.

The game build then gains one assembly that nothing in play calls.

**Zero-diff alternative.** `BodyFrame.cs` can sit in `Tests/PlayMode` for this step, and the Runtime reference can
wait until layer 3. The constants of §2.1 would then be copied rather than made internal, and tests B3 and B5 would
guard the copies.

### 1.2 The console runners build the same files

- The three `src/*/*.csproj` stay where they are. `bin/` and `obj/` therefore stay outside Assets, and the tests'
  `ProjectReference` paths do not change.
- A new `port/Directory.Build.props` names the folder:

```xml
<Project>
  <PropertyGroup>
    <!-- One source: Unity compiles these files as WonderGather.AnyBody; the console projects compile the same. -->
    <AnyBodySrc>$(MSBuildThisFileDirectory)..\..\..\..\Assets\_WonderGather\Scripts\Units\Rising\AnyBody\</AnyBodySrc>
  </PropertyGroup>
</Project>
```

Each src project then compiles only its folder (Core shown; Body and Reserve alike):

```xml
<PropertyGroup><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
<ItemGroup><Compile Include="$(AnyBodySrc)Core\**\*.cs" /></ItemGroup>
```

- Unity's `.meta` files are not `.cs`, so the console never sees them.
- Unity also generates a `WonderGather.AnyBody.csproj` at the repository root for the IDE. The root `.gitignore`
  ignores it, and it is not the console's project.

### 1.3 The move

1. Wait until `REVIEW.md` is finished and its fixes are committed. Rerun both runners and note their counts. Before the
   review they were Body.Tests 21541 checks, 0 failed, and Reserve.Tests 5072, 0 failed.
2. **Delete `port/src/*/bin` and `obj` first.** A folder move would drag them into Assets:
   - `obj/**/*.AssemblyInfo.cs` and `*.AssemblyAttributes.cs` would compile into the one assembly three times
     (CS0579, duplicate assembly attributes);
   - `bin/**/*.dll` would import as plugins that define every type a second time (CS0433).
3. Move each `.cs` with `git mv`, which keeps their history. Unity makes the `.meta` files for them and for the new
   folders. These carry new GUIDs only, and no existing asset's GUID changes. Commit the `.meta` files with the move.
4. `.gitattributes` has `*.cs text eol=lf`, so the CRLF files are normalised on commit. Unity's "inconsistent line
   endings" warning is per file, and no file mixes the two.
5. Update the layout tables in `README.md` and `INTERFACE.md`.

### 1.4 Names

**Keep the namespace `WonderGather.AnyBody`**, not `WonderGather` as strategy §5 wrote. The port has short type names
(`Pose`, `Stance`, `Target`, `Margin`, `Statics`, `Supports`, `Effort`, `Kin`, `D`, `Tape`, `Sym`). Kept apart:

- they cannot shadow game types;
- they cannot be shadowed by game types;
- the console tests are unchanged.

What was checked at `f303d3f`:

- **The game's types.** None of the port's 59 type names (nested ones included) is among the 236 type names under
  `Assets/_WonderGather`. No game identifier is called `AnyBody`.
- **UnityEngine's types.** Two clash:
  - `Pose` (the port's `static class Pose`, `Body/Pose.cs`) with `UnityEngine.Pose`;
  - `ContactPoint` (`Core/StaticsProblem.cs`) with `UnityEngine.ContactPoint`.

  Any file with both `using UnityEngine;` and `using WonderGather.AnyBody;` that names either type fails with CS0104.

**The rule.** Game-side files (namespace `WonderGather` or `WonderGather.Tests`) never write
`using WonderGather.AnyBody;`. They write `AnyBody.Pose`, `AnyBody.BodyModel`: inside `namespace WonderGather`, the
child namespace resolves by its short name. No alias and no rename are needed. Never name a game type or member
`AnyBody`, because that would break the lookup.

Inside the AnyBody assembly no clash is possible, because it references nothing.

### 1.5 What Unity's compiler or IL2CPP could reject

**Language and library.** Nothing found.

- The projects already build with `LangVersion 9.0` against `netstandard2.1`, which is Unity's level. C# 9 is the
  language, and `ProjectSettings.asset` sets `apiCompatibilityLevel: 6`, which is .NET Standard 2.1.
- The C# 8 and 9 features that need runtime support Unity lacks are not used: init setters and records (they need
  `IsExternalInit`), covariant returns, default interface members, module initializers and function pointers. `src`
  declares no interface at all.
- `src` uses no `unsafe`, `stackalloc`, reflection, `Emit`, `dynamic`, `System.Numerics`, `System.Text.Json`,
  `Console`, environment reads or threads. The tests use JSON and `Console`, and they stay outside Unity.

**IL2CPP.** Standalone builds use Mono today: `ProjectSettings.asset` sets a scripting backend only for Android.
IL2CPP would still have nothing to object to:

- no generic methods at all, and the only virtual calls are `Target.Eval`'s overrides;
- no `MakeGenericType`;
- the boxed `Stance.Args` (`object[]`) and the dictionaries of named tuples compile as usual;
- managed stripping keeps everything, because nothing is reached by reflection.

**What to watch.**

- **Floating point.** Runs on different runtimes are not promised to agree bit for bit (`Math.Sin`, `Cos` and `Log`;
  README). A fit can end in another basin on another runtime. Tests compare with tolerances and count such fits
  rather than assert them away.
- **Mutable statics.** `BodyFactory.Small`, `Long` and `Round` are `static readonly` objects with public fields.
  Never mutate them; `BodyFrame` builds its own.
- **Allocations.** The fitter allocates per fit: a `Tape` of 2048 rows after the review. That is fine for a capture.
  Layer 3 takes over the review's findings on allocations and threads.

## 2. BodyFrame: the game's body as a `BodyModel`

### 2.1 What and where

`Scripts/Units/Rising/BodyFrame.cs` is a static class in namespace `WonderGather` (Runtime). It only reads, and
nothing in play calls it in this step. It has two entry points.

**`FromUnit(GameObject unit)`** takes a miner before it is made: a prefab asset, a scene instance that has never woken,
or a live unit. `MinerChoice` keeps the miners it does not show inactive (MinerChoice.cs:44), so a test reaches a
never-woken miner without `AssetDatabase`.

- It reads `ProceduralBiped.SavedProportions`, never `BodyProportions`. Until a body wakes, the active proportions are
  the 2.2 m default body's (`ProceduralBiped.cs:95-97`).
- It reads `PhysicalBody`, the Body solution's foot and toe boxes, and the height of the unit's capsule.
- It returns `BodyFactory.Make(source, BodyVariant identity)` with the capacities now written into the per-side arrays.

**`FromMade(OwnBody own)`** takes a body standing by its own joints and reads its 13 links (`OwnBody.Part(i)`,
`OwnBody.cs:347`):

- masses;
- anchors, which give the lengths;
- colliders, which give the radii and boxes;
- `forceLimit`, which is the capacity as made (`OwnKeeper.Soften` keeps it, `OwnKeeper.cs:220-229`; the riser would
  scale it by tone, `OwnRiser.cs:353-376`, so only a standing body is read);
- PhysX's centres and inertias, set with `SetLinkMassProperties`.

This is strategy §5's "masses and centres as PhysX has them". A standing body has no toes, and its boot carries
Foot + Toe (`OwnBody.cs:719`), so the toes come from `FromUnit`.

**One port change:** a new overload `BodyFactory.Make(MinerSource s0, BodyVariant v)`. Today's `Make(v)` becomes
`Make(Miner(v.Source), v)`, and the console's `bodies` check must still give 4e-16.

**One game change, with no effect in play.** `BodyFrame` should use the game's own numbers, so these go from `private`
to `internal`:

- `AnkleGives`, `HipGives` and `NeckHolds` (`OwnBody.cs:51`);
- `PhysicalBody`'s `ArmReference`, `ShoulderMuscle`, `ElbowMuscle`, `KneeMuscle` and `LegReference`
  (`PhysicalBody.cs:29, 221`), which the made bodies' capacities need (§2.6).

### 2.2 `MinerSource`, field by field

The "Small" column gives the prototype's value / the game's value. S is `Strength`. P is the unit's
`ProceduralBiped.Proportions` as saved (`SavedProportions`). This is the game's proportions type: no type called
`StanceProportions` exists at `f303d3f`.

| MinerSource (b3) | from the game | file:line | Small |
|---|---|---|---|
| `H` (H) | `P.hipHeight`, which MinerSetup wrote as ankle + 0.956·2·leg | ProceduralBiped.cs:43; MinerSetup.cs:377 | 0.605714 / 0.6057141 |
| `WH` (wH) | `P.hipWidth` | ProceduralBiped.cs:43, 1288 | 0.068 |
| `Leg` (lT = lS) | `P.legSegment`, both thigh and shin | :43, 1301-1303 | 0.27234 |
| `A` (a) | `P.ankleHeight` | :45, 978 | 0.085 |
| `Hl, Bl, Tl` | `P.heelLength, ballLength, toeLength` | :45, 1292-1297 | .07242/.08449/.05552 |
| `HW` (hW) | `P.waistRise` | :47, 1238 | 0.1904 |
| `TR` (b3 uses hS/2) | `P.torsoRise` (= shoulder.y/2, MinerSetup.cs:380) | OwnBody.cs:682-683 | 0.10295 |
| `HeadRise` (→ lN) | `P.headRise` | OwnBody.cs:686 | 0.3805 |
| `Sx, Sy, Sz` (wS, hS, dS) | `P.shoulder.x, .y, .z` | ProceduralBiped.cs:49, 1264 | .145/.2059/−.010 |
| `Lu, Lf` | `P.upperArm, P.forearm` | :50, 1413 | .2343/.1988 |
| `BodyFront` (→ gH) | `P.bodyFront` | :65; OwnBody.cs:675 | 0.1452 |
| `HeadHalf` (→ rH) | `P.headHalf` | :65; OwnBody.cs:692 | 0.163 |
| `Ra, Rl` | `PhysicalBody.ArmRadius, LegRadius` | PhysicalBody.cs:48-49; OwnBody.cs:656 | .033/.041 |
| `Height` (report only) | the unit's `CapsuleCollider.height` (= the agent's height) | MinerSetup.cs:324, 336 | 1.42 |
| `MHips` | Σ parts on bones "Pelvis…", at least 0.1 kg (OwnBody.Weighs) | OwnBody.cs:470-480, 674 | 7.88 / 7.8801 |
| `MTrunk` | Spine + Chest + Neck | :680 | 8.96 / 8.9600 |
| `MHead` | Head × `HeadTimes` (1) | :685 | 9.96 / 9.9619 |
| `MUa[L,R]` | UpperArm.L, UpperArm.R | :703 | 1.12, 1.11 / 1.1183, 1.1108 |
| `MFa` | Forearm + Hand | :706 | 0.80 / 0.8044 |
| `MTh`, `MSh` | Thigh; Shin | :714, 719 | 2.38, 1.35 / 2.383, 1.3501 |
| `MFt` | Foot (the getting-up boot) | :718-719 | 1.07 / 1.0749 |
| `MTo` | Toe | :820 | 0.21 / 0.2119 |
| `CBack` | `BackNow` = 2.4·upperAsks·g·S·share(Back)·fresh(Back) | PhysicalBody.cs:191-192; OwnBody.cs:680 | 257 |
| `CNeck` | 3·MHead·g·max(0.03, headRise − 2 torsoRise)·S·share(Neck) | OwnBody.cs:686-690 | 51 / 51.2 |
| `CShoulder` | `ShoulderOf(side)`·ArmsTimes; muscle rule: share(Arms)·121.3·(ra/0.047)³·S·fresh(arm) | PhysicalBody.cs:243, 260; OwnBody.cs:703 | 42 / 41.5 |
| `CElbow` | `ElbowOf(side)`·ArmsTimes; the same with 104.5 | PhysicalBody.cs:244, 261; OwnBody.cs:706 | 36 / 35.8 |
| `CKnee` | `KneeNow`; muscle rule: share(Legs)·137.5·(rl/0.046)³·S·fresh(Legs) | PhysicalBody.cs:248-258; OwnBody.cs:657 | 87 / 86.9 |
| `CHip`, `CAnkle` | knee × `HipGives` 1.5; knee × `AnkleGives` 0.7 | OwnBody.cs:51, 714, 723 | 130, 61 / 130.4, 60.8 |
| `CToe` | knee × 0.7 × `ToeGives` 0.3 | OwnBody.cs:180, 829 | 18 / 18.2 |

**The need rule.** Under `-strengthRule need`, `PhysicalBody.KneeCapacity` reads the active proportions
(`PhysicalBody.cs:254`), which belong to the default body until the unit wakes. `BodyFrame` therefore computes that
knee from the saved proportions itself.

**Numbers b3 assumes that the game holds as data.** `BodyFrame` reads them and sets them on the model, then reruns
`ShapeMassProperties`:

- foot box: half-width `fw` = box x/2, height `fh` = box y;
- toe box: half-width `tw` and height `th`, read the same way.

They come from `MinerSetup.cs:365-366` (0.10 × 0.06 and 0.10 × 0.05) through `OwnBody.cs:725, 816`.

### 2.3 b3.make_body's derived values against `OwnBody.Make`

The two agree for all three miners, except where the last column says otherwise.

| b3 | make_body | the made link (OwnBody.Make) | file:line |
|---|---|---|---|
| lT, lS | leg | thigh and shin rods = 2 × segment `localScale.y` = legSegment | OwnBody.cs:715, 721 |
| rT | 1.25 rl | thigh radius `leg * 1.25f` | :715 |
| hS, tR | sy, hS/2 | trunk anchor (0, −torsoRise, 0); rod half-length torsoRise + breadth/2 | :682-683 |
| lN | max(0.03, headRise − hS) | neckLever = max(.03, headRise − 2 torsoRise), the head's anchor | :686, 694 |
| rH | max(0.06, headHalf) | the skull's radius | :692 |
| gH | clamp(bodyFront, 1.5 rl, 0.3 H) | girth; capsule across, height 2(hipWidth + girth) | :675-677 |
| gT | max(sx − ra, 0.5 sx) | breadth | :681 |
| capsule ends | upper arm and legs inset by r; forearm elbow to wrist plus caps | rods: upper arm = segment; forearm = segment + 2 arm; legs = segment | :704, 707, 715, 721 |
| foot box | x −hl..bl, z −a..−a+fh about the ankle | box at the foot segment: sole under the ankle, heel to ball | ProceduralBiped.cs:973-978, 1292-1293 |
| toe box | from the ball, 0..tl × 0..th | toe segment | ProceduralBiped.cs:1297 |
| centres | the shapes' centroids (`ShapeMassProperties`) | PhysX's own from the colliders; nothing sets them unless `Weighed` | OwnBody.cs:803 |
| masses | per link | `Weighs` | b3 rounded each to 0.01 kg |
| capacities | the need-rule tables, whole N m | the muscle rule with the miners' shares (`SharesAsNeeded`, PhysicalBody.cs:137-151) | within 0.3-0.8% |
| friction | 0.6 (`Supports.Mu`, a const) | 0.7 dynamic, 0.8 static | OwnBody.cs:655 — **kept 0.6** |
| ranges | b3.RANGES | none on the made body (`HingesStop` off); layer 0's `JointRange.Of` behind `Anatomical` | OwnBody.cs:195; JointFrames.cs:27-41 — **b3's kept** |

### 2.4 The frame map

**The two frames.**

- **AnyBody:** x forward, y left, z up (right-handed); the ground is z = 0.
- **Unity:** x right, y up, z forward (left-handed).

**Points:** u(p) = O + Yaw(ψ)·(−p.y, p.z, p.x), where O is on the stage top and ψ is the turntable's turn (§3.4).

**Link axes.** OwnBody's frame for each link (y along the part away from its joint; z the side its elbow or knee sticks
out; JointFrames.cs:11-13), in the AnyBody link's own axes (the columns of `Kin.R`):

| links | OwnBody x | y | z | at zero, in Unity |
|---|---|---|---|---|
| hips, trunk, head, boots, toes | −y | z | x | right, up, forward |
| upper arms, forearms | −y | −z | −x | right, down, back (180° about x) |
| thighs, shins | y | −z | x | left, down, forward (180° about z) |

- The Unity rotation is `LookRotation(M·R·ẑ_O, M·R·ŷ_O)`, where M·p = (−p.y, p.z, p.x), then the yaw.
- These are `JointFrame.Of`'s zeros (JointFrames.cs:56-85): the shoulder 180° about x, the hip 180° about z, the boot
  180° about z from the shin.

**Segment positions.** These are where ProceduralBiped puts the segments (ProceduralBiped.cs:1070-1075, 1238-1255,
1292-1297) and where OwnBody makes its links (OwnBody.cs:674-723):

| segment | position |
|---|---|
| pelvis | P |
| torso | trunkC |
| head | headC |
| upper arm | (sh + el)/2 |
| forearm | (el + wr)/2 |
| thigh | (hj + kn)/2 |
| shin | (kn + an)/2 |
| foot | an + R_ft(½(bl − hl), 0, −a + ½fh) |
| toe | ball + R_to(½tl, 0, ½th) |

**`ReadPose`, the inverse** (used by the B4 test now, by layer 3 later).

1. Each link's rotation gives R = Mᵀ·Q·C⁻¹. Q is the link's Unity rotation with the turntable's yaw taken off, and C is
   the link-axes table above, written as a matrix.
2. The parent-to-child turns are taken apart in b3.fk's order (Kinematics.Forward). The sign s is +1 left, −1 right
   (Kinematics' `Sg`).

| joint | turn |
|---|---|
| hips | Rz·Ry·Rx |
| back, neck | Ry·Rx·Rz |
| shoulder | Ry(−flex)·Rx(s·abd)·Rz(s·rot) |
| elbow | Ry(−elb) |
| hip | Ry(−flex)·Rx(s·abd)·Rz(s·rot) |
| knee | Ry(knee) |
| ankle | Ry(−dorsi)·Rx(s·inv); the engine's ankle twist, which b3 lacks, is returned as a residual |
| toe | Ry(−toe) |

### 2.5 The bridge tests (PlayMode, `BodyFrameTests.cs`)

**B1. The miners as the prototype had them.** `FromUnit` on each miner prefab against `BodyFactory.Make(name)`.

| quantity | tolerance | why they can differ |
|---|---|---|
| lengths, radii, boxes | ≤ 2e-6 m | b3 copied the prefabs' float32 numbers to 6-7 digits (miners.json: Small leg 0.27234000884) |
| each link's mass | ≤ 0.006 kg | b3 rounded each link to 0.01 kg (Foot.L 1.0749 → 1.07) |
| total mass | ≤ 0.05 kg | the same rounding (Small 40.68 against 40.65) |
| capacities | ≤ max(0.6 N m, 1.2%) | b3 has the need rule's whole N m. The default now is the muscle rule with the miners' shares: the knee keeps its value, each arm joint is within 0.3-0.8% (Small shoulder 41.5 vs 42; Long elbow 65.7 vs 65; body_map §1.5) |
| ranges | equal | both b3's defaults |
| `LinkCentre` | ≤ 2e-6 m | the same formulas on the same lengths |

Then the consequences, at BodyFactory's fits of the 45 stances:

- r: |ΔS| ≤ max(0.005, 1.5% of S);
- base margins: within 0.5 mm.

**B2. Wake order.** `FromUnit` gives the same result, bit for bit, on a miner that has never woken and on the same miner
awake. It does so under both strength rules.

**B3. The made body agrees.** On the standing own body (`MinerWorkPreview.SetOwn(true)`, as in
`OwnRiseBench.cs:240-247`), for all three miners, `FromMade` against `FromUnit`:

- link masses within 1e-4 kg (the boot = Foot + Toe);
- lengths from the anchors within 1e-5 m;
- collider radii and boxes within 1e-6 m;
- `forceLimit` equal to the capacities within 1e-4 (relative);
- PhysX's centres, carried into the AnyBody link frame, equal to `ShapeMassProperties` within 0.5 mm.

**B4. Read back.** `ReadPose` on the standing own body, then `Kinematics.Forward`, against the made links' anchors in
the world:

- waist, shoulders, elbows, wrists, hips, knees, ankles and balls within 1 mm;
- the boots' corners within 5 mm, because the engine's ankle twists and b3's does not;
- the neck within 5 mm, because the head was turned at make (`matchAnchors`, OwnBody.cs:766).

This proves the frame map and every sign at once.

**B5. The muscle rule, mirrored.** `BodyFrame`'s muscle capacities equal `PhysicalBody`'s own, to float precision, for
the three miners at their shares and at shares of 1.

### 2.6 Made bodies for the sheets

These are data bodies, not bodies the game has made: there is no `BodySpec` or `BodyMaker` yet (body_map §3.4).

**How each is built.**

1. The source is `FromUnit(miner)`, or a mix of two miners.
2. `b = BodyFactory.Make(source, variant)` applies b3's verified rules for lengths, masses and shapes: scale, length
   multipliers with mass in proportion, and mass multipliers.
3. `BodyFrame.GameCapacities(b, shares, S)` then gives each capacity by the game's rule now, from b's own numbers:
   - **arms and legs** by muscle, from b's radii. A body scaled by k gets k³, so a giant is weaker for its weight
     (Luis, November 3).
   - **the back** by its need as the game reckons it: the source's measured back × b3's asks ratio
     (`BodyFactory.PresentCaps`).
   - **the neck** by OwnBody's formula.

b3's "present" rule, under which capacity follows need, is not used for these bodies.

| # | body | how |
|---|---|---|
| 1-3 | Small, Long, Round | `FromUnit` |
| 4-6 | halfway Small–Long, Long–Round, Round–Small | every `MinerSource` number, the region shares, and the back per unit of b3's asks, each mixed at t = 0.5 |
| 7 | Small ×0.7 | Scale 0.7 |
| 8 | toddler-like | Small, head mass ×1.3, legs ×0.85 |
| 9 | Long ×1.25 | Scale 1.25 |
| 10 | Long, legs ×1.2 | LegMul 1.2 |
| 11 | Round, trunk mass ×1.5 | MassMul trunk 1.5 |
| 12 | Small at strength 0.5 | Strength 0.5 |
| 13 | Round at strength 0.3 | Strength 0.3 |

- Bodies 11-13 keep a miner's lengths, so they are drawn as that miner too.
- A uniform scale also scales the soles and the ankle, as b3 does. The game's own makers do not: the Blender pipeline
  keeps the ankle at 0.085 m, and MinerSetup keeps the soles at 0.10 × 0.06 m. The sheet says so.

## 3. The atlas sheets

### 3.1 Fit and read

For each body × stance:

1. **Build and fit.** `StanceLibrary.Make(b, family, args)`, then `StanceFitter.Fit(b, st, opt)` with the default
   options.
   - A made body adds the nearest miner's fit of the same stance as `opt.ExtraStarts`. A halfway body adds both
     parents' fits.
   - This is the README's `--atlas-extra`. On the 450 test fits it brought the fits worse than the prototype's from 21
     to 17, and it lowered every variant body's mean cost.
   - It is also the strategy's defence against fits that jump between neighbouring bodies (§6 item 4). As an extra
     start, though, it acts only when the body's own start ends at a cost of 0.05 or more.
2. **r.** `Statics.Build`, then `ReserveSolver.Solve`.
   - **r** is printed as S: b3's s, the LP optimum, which is unique.
   - The vertex-dependent R and hands' share go to the CSV only (INTERFACE.md, "Read this before using R").
   - **The worst joint** is every joint whose `PerJointLp` is within 0.005 of S.
3. **Report.** `Statics.Report` gives cost, contact error, penetration, base margin and `ReleaseMargin`.
4. **Let go.** The let-go r is `Solve(p.Subset(c => kind not in Statics.ReleaseKinds(family)))`.
5. **Wander.** For held stances, `FeasibleRegion.Compute` gives how far the weight may wander.

There is one `ReserveSolver` per thread. Fits may run in parallel (`-atlasThreads`).

**Output.**

- `figures.csv`: one row per body × stance.
- `atlas.json`: each body's numbers and each fit's q.
- `-atlasFits atlas.json` re-renders without fitting again.
- The console can refit the same bodies, which compares the two runtimes.

### 3.2 The solids (every body)

They come from `Kinematics.Forward` at the fitted q (values only, `D` from `double`):

- **10 capsules** (`Kin.CapA/CapC/CapR`), each built as a cylinder plus two spheres. A scaled Unity capsule stretches
  its caps: `BodysOwnBench.Shape`'s scaling (`BodysOwnBench.cs:238-241`) is exact only when the capsule is four radii
  long.
- **the head's sphere** at headC.
- **boxes** for the boots and toes, placed as in §2.4.

Materials are matte clay. Links that bear a support are tinted.

**Marks on the picture.**

- Each contact point (`Supports.Contacts`) is a 12 mm dot coloured by kind: hands and forearms; knees and shins; soles,
  balls and toes; seat and calves; a hand on the thigh. A short bar shows its normal load in one optimal force split.
- The base is the hull of the ground contacts, drawn as a line on the stage.
- The feasible region (`FeasibleRegion` points) is a second line.
- The centre of mass is a dot with its plumb line.
- For a ready stance, the base left after the let-go is drawn dashed.

### 3.3 The drawn miner (bodies 1-3, 11-13)

**How MinerBody draws.** It runs in `LateUpdate` at execution order 50 (MinerBody.cs:11, 510-581):

- It places the pelvis bone from the Solution's pelvis segment.
- It turns the chest and head from the torso and head segments. The spine and neck each take half the turn.
- It aims each limb bone along its segment's ends. `Ends` is position ± rotation·up·`localScale.y` (:456-462).
- It rolls each limb by a pole. When `TurnsAsSolved()` was called that frame (:195), the pole is the segment's own
  forward: the side its knee or elbow sticks out (:477-490).
- Feet and toes take their segments' rotations.
- It only turns bones, and every bone keeps its modelled length.

**What feeds it.** `OwnBody.LateUpdate`, at order 41, writes the segments from its links and calls `TurnsAsSolved`
(OwnBody.cs:1308-1331).

**The atlas does the same.** An `AtlasPoser` (tests, `[DefaultExecutionOrder(45)]`) does two things each frame:

- writes the 15 segments of `MinerBody.Solved` (pelvis, torso, head, upper arms, forearms, thighs, shins, feet, toes)
  from the `Kin`, by §2.4;
- calls `TurnsAsSolved`.

**Set beforehand.**

- `ProceduralBiped.LetGo = true`, so the biped leaves the segments alone (ProceduralBiped.cs:570-574).
- `PhysicalBalance.Acts = false`, as OwnBody does when it takes the body (OwnBody.cs:856-858).
- `MinerBreath` off, with `Swell = Shrug = 0`: no breath in a still.
- `Motor.Stop()`, and the `NavMeshAgent` off.
- `OwnBody` not standing.
- The pickaxe hidden.
- Each segment's `localScale.y` set to the model's half-lengths. The test asserts these are within 1 mm of what the
  biped left.

**Hung things.** The lamp, mug and satchel (`MinerBody.Hang`, :676) settle under gravity: the pose is held 30 frames
before a stance's first shot, and 15 after each turn. MinerBody knows no ground, so in a lying stance a hung thing may
pass into it. That is noted, not fixed here.

**Where the two looks differ.** The solids are what the fit used, so contact is read on them.

- The drawn head stands about 8 cm above the solids' sphere: the physics head is centred at the Head bone's base
  (body_map §1.7).
- Where the mesh's shape differs from a solid (seat, hands, knees), the drawn miner may sink into the stage or float
  above it by a few centimetres.
- When one look is shot, the other is hidden.

### 3.4 Place, light, views, sizes

- **The scene:** TheOrdinaryPlace, set up as `OwnRiseBench.cs:200-213`:
  - `CaptureTools.AsItWas`, `MinerChoice`, and `Camera.main` with `RtsCamera` off;
  - `GrassField` off, `TimeOfDay.Hour = 13` (the benches' hour), `Time.captureFramerate = 30`.
- **A level stage.** It stands on the path spot `MinerCloseCapture` uses (`ground.Path[4]`,
  `MinerCloseCapture.cs:91-93`):
  - a disc 3.2 m across, its top 2 mm above the highest ground under it;
  - the path's earth colour, with a faint 10 cm grid.

  The fits are for level ground (strategy §6 item 6), and the grid gives the size.
- **A turntable.** The body turns so that its front, its right side, its back and its left side face the camera in turn
  (ψ = 0, 90, 180, 270). The camera and the sun stay put, so every side is lit alike and the background holds still.
  The body turns about the centre of its stance's bounds on the stage, so the camera's target stays put too.
- **The camera.**
  - The sun is 40° to the camera's left. The camera is 15° above the stage, with a field of view of 20° (little
    perspective).
  - It aims at that centre.
  - Its distance is set once per body, so that the standing height × 1.25 fills the frame. All of a body's stances share
    one scale.
- **Tiles** are 512 × 512, named `<body>_<stance>_<look>_<view>.ppm`. `CaptureTools.Render` writes PPM, because the
  project has no image-encoding module (CaptureTools.cs:8-9, 35-65).
- **Counts:**
  - solids: 13 bodies × 45 stances × 4 views = 2340 tiles;
  - drawn: 6 × 45 × 4 = 1080 tiles;
  - about 2.7 GB of PPM until the script converts them.
- **Run time, estimated (not measured):** about 3 min of fitting under Mono, and about 10 min of settling and
  rendering.

### 3.5 Files and commands

- **Captures** go to `Captures/Atlas/<date>/`: `tiles/` (PNG, once the script has converted the PPM), `figures.csv`,
  `atlas.json`, `log.txt` and `sheets/` (PNG). Git ignores `/[Cc]aptures/`.
- **What is kept.**
  - The sheets sent to the judges and the Director go to `Docs/Images/StanceAtlas/<date>/` as JPG, like the other
    committed pictures, and are listed in `Docs/Validation.md`.
  - `figures.csv` is copied to `Docs/Technical/AnyBody/results/atlas_<date>.csv`.
  - Everything else is made again by the two commands.

```
Unity -projectPath D:/Dev/WG -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.StanceAtlasCapture
      -atlasOut <folder> [-atlasBodies small,long,...] [-atlasStances squat,...] [-atlasLooks solids,miner]
      [-atlasFits <atlas.json>] [-atlasThreads n]      ([Explicit] and run on its own, like MinerCloseCapture.cs:22-25)
python Docs/Technical/AnyBody/sheets/atlas_sheets.py <folder> <folder>/sheets     (Pillow; PPM -> PNG, PPM removed)
```

### 3.6 Composing the sheets (Python, afterwards)

The figures are printed by Python, not Unity, as `Art/Review/sheets.py` and `Docs/Technical/RiseWays/stagesheet.py`
already do. Three reasons:

- Unity has no image encoder.
- The tiles stay clean for close reading.
- The layout can change without rendering again.

| sheet | count | contents |
|---|---|---|
| one stance, the miners | 45 | rows Small, Long, Round, each drawn and then as solids; columns front, right, back, left |
| one stance, the made bodies | 45 | rows 4-13 as solids; 11-13 also drawn |
| one body, the whole library | 13 | the left side of every stance, 9 × 5: repetition and stiffness at a glance |
| the line-up | 1 | all 13 bodies standing, at one scale |

The sheets are PNG. Tiles appear at 384 px on the stance sheets, and at 256 px on the body pages and the line-up. Before
the judges get them, check by eye that what they are asked about shows at that size.

**Headers and labels.**

- Each stance sheet's header says what the stance asks: its supports and bands, from the library.
- Every row has a label in plain words, for example:
  `Long · fit 0.08 · r 0.21 (left hip) · base +63 mm (asks 28) · let go of the hands: r 0.19, +63 mm · wanders 41 mm`
- When a stance is not held, the label says "cannot hold" and gives the margin.
- Other marks:
  - "poor fit" at a cost of 0.5 or more;
  - "failed fit" at 2 or more (strategy §2.2 item 5);
  - a contact error or penetration over 2 mm.
- A legend strip is on every sheet.

## 4. Order of work

Each step has its own test and is reported from the run that was read. Unity runs in the worktree, with nothing heavy
beside it.

| # | step | test | what it shows |
|---|---|---|---|
| 0 | the review finishes | `REVIEW.md` closed and its fixes committed; both runners rerun and their counts written in Validation | nothing |
| 1 | one source (§1) | both runners give step 0's counts; Unity compiles with no error and no new warning (Editor.log); a fresh clone builds the runners | nothing; the game unchanged |
| 2 | the port inside Unity: `AnyBodyPortTests`, PlayMode `[Test]`s, no scene | see below | numbers only |
| 3 | `BodyFrame` (§2) and the `Make(MinerSource, …)` overload | B1-B5; the console's `bodies` still at 4e-16 | a table, game against prototype, field by field, for the three miners; read-back errors per joint |
| 4 | the solids: `AtlasStudio` (stage, light, turntable, camera) and `AtlasSolids` | capsule ends within 1 mm of the `Kin`'s; contact dots within 1 mm of the stage; every solid inside its tile with 5% to spare | Small, stand and hands and knees, four sides, read by eye first |
| 5 | the drawn miner: `AtlasPoser` | see below | those 12 stances × 4 sides, drawn beside the solids |
| 6 | the atlas run (§3.1-3.5) | 3420 tiles and 585 figure rows, all present; the miners' costs as in step 2; per body, the median cost and the fits off or through the ground, beside the console's refit of the same `atlas.json` bodies | all tiles |
| 7 | the sheets (§3.6) | 104 sheets; the script checks a sample of labels against `figures.csv` | the sheets |
| 8 | read, judges, Director | see below | the Director's sheets |
| 9 | free limbs (if needed) | see below | a second set of sheets beside the first |

**Step 2's test.** It reads one golden CSV, written by a new console command `golden`. Unity's profile has no general
JSON reader, and `JsonUtility` cannot read the reference files. The CSV holds:

- the 17 bodies of `body_ref_bodies.json`;
- the 126 saved miner poses and costs of `res_miners2.json` (`check_saved.py` reproduces them exactly);
- the console's S, base margin, let-go margin and wander at those poses.

Unity must match them:

- the bodies within 1e-12;
- the costs at the saved poses within 1e-9 (relative);
- S and the margins within 1e-9.

Also checked:

- The three miners' 45 fits from x0 against `results/fit_all.csv`: the number of equal costs (within 1e-6) and the
  number in other basins, counted.
- The time per fit under Mono.

**Step 5's test.** For Small, Long and Round × stand, hands and knees, squat at h 0.35, and half-kneel with a hand on
the thigh:

- the model's joint bones within 15 mm of the fit's joints (MinerBody keeps the modelled lengths);
- each knee and elbow on its fit's side (dot product above 0.5);
- standing boots within 10 mm of the stage;
- hung things moving under 5 mm/s at every shot.

**Step 8.**

1. I read every sheet myself, at its own size, by the model quality method.
2. Steps 1-3 get a technical review: three reviewers (the physicist, the sceptic, the builder) on a copied folder.
3. Three aesthetic judges (`Art/Review/judges`) get the sheets. They are told what each stance asks and its figures,
   nothing about the build.
4. Each finding is checked against the figures. Only faults of the bridge or the pictures are fixed here. A stance that
   looks robotic is a finding: it is written down for the Director, in the judges' words.
5. The Director gets the miners' stance sheets, the body pages and the line-up. Nothing must be switched on to see
   them.

**Step 9, if free limbs read as robotic.** This is expected: strategy §2.2 item 6 reports Long's free elbows at
107-119° and free toes curled 40-70°.

- **The switch.** `FitOptions.RelaxFreeLimbs`, off by default, so every console result stays as it is.
- **What it adds.** The strategy's "free limbs" (§1.1): a weak pull of the free angles toward the relaxed pose:
  - neck rotation 0;
  - shoulder rotation 0;
  - free elbows about 15°;
  - free toes 0.
- **Test.** With the switch off, the 450-fit table is unchanged. With it on, the median cost without the pull rises by
  under 0.01, and r by under 0.01.
- **The sheets** are made again beside the first ones and labelled. The Director chooses (README "next", item 3).

## 5. Not in this step

**Not done.**

- **No controller and no game change.**
  - Layer 3 is not started.
  - Nothing played changes: not `OwnBody`, `OwnRiser`, `OwnKeeper` or `PhysicalFall`. There is no new switch.
  - Small's present way and the old way stay (Luis, November 3).
- **Nothing of layer 2.** No `WayChooser`, `RisePlan` or `SeededDraw`. No route strips or sweep sheets. No `StanceAtlas`
  cache: the fits are made on each run. No warning in the creator.
- **No new stances.** Side-lying, side-sit, the tuck sit, kneeling on the heels, the plank, the roll, and the squat on
  hands and feet at h ≈ 0.25 are not added (strategy §1.2). "Hands and knees, ready" is not rewritten.
- **No `BodySpec` or `BodyMaker`.** The made bodies are data bodies.
- **Kept as they are:**
  - no measured centres (`OwnBody.Weighed` stays off);
  - b3's ranges, not layer 0's;
  - friction 0.6;
  - level ground only.
- **Not run:** no IL2CPP build, and no timing on an idle machine.

**Not shown by these pictures.**

- **Repetition across falls.** Stills cannot show it; that is layer 2's seeded variation and layer 3's motion.
- **Motion** of any kind.
- **The game's uneven ground.**
- **Holdability.** Whether the engine's body could hold a fitted stance: r is quasi-static.

**Open, for the lead or the Director.**

- **The reserve's measure:** S, or the full-norm reserve (README, "Reserve side, next").
- **Free limbs:** ~~pulled toward the relaxed pose (step 9), or left as fitted~~ **decided by Luis, November 5: they rest**
  (`FitOptions.RelaxFreeLimbs`, now the default; built after the first look and the judges' round 1, `77cc4ec`).
- **Which ranges the fits use:** b3's knee stops at 150°, layer 0's at 155°.
- **The miners' muscle shares:** at 1, their build would show in their strength.
- **Friction:** the port's 0.6 against the game's 0.7/0.8.
- **Hung things** in lying stances (§3.3).

## Check (adversarial)

Checker: an independent agent that wrote none of this design, November 4, 2026. Each claim about existing code was
opened at its file and line. The designer's text above is left as it was; corrections are here. Nothing under `Assets`
or `ProjectSettings` was changed, Unity was not run, nothing was committed. Grades: **sure** (read in the code, or
computed from it), **likely** (read, but one step not run), **question** (for the designer or the lead).

Status: complete for this pass (November 4, evening). Findings are ranked, most consequential first.

### C0. State of the tree (read before acting on anything above)

- **sure.** The review is committed: `claude/wip-round` is at `00991e5`. `git diff f303d3f 00991e5` touches only
  `Docs/Technical/AnyBody/port`, so the game lines cited at `f303d3f` hold at `00991e5`. Step 0 of §4 is half done:
  REVIEW.md gives both counts after its fixes (Reserve 5072 / 0, Body 21541 / 0).
- **sure.** **Another session is changing the game under this design right now.** Uncommitted in `D:/Dev/WG`
  (modified 20:36, after the review's commit): `Scripts/Units/PhysicalBody.cs`, `Editor/MinerSetup.cs`, the three
  `Prefabs/Miners/*.prefab` and `Tests/PlayMode/PhysicalBodyTests.cs`. Unity holds the same worktree
  (`Temp/UnityLockfile` and `Assets/InitTestScene…unity`, both 20:43: a test run). `ProjectSettings/*.asset` are
  modified (a new define, `APP_UI_EDITOR_ONLY`), and `Assets/ML-Agents/` and four `RiseModels/*.onnx` are untracked.
  Consequences:
  - The move (§1.3) and any edit of `PhysicalBody.cs` must wait until that work is committed and no Unity process holds
    the worktree. Never `git add -A`: stage the moved files by path.
  - What changes is C1 below.

### C1. The miners' strength calibration is being rewritten (sure; affects §2.1, §2.2, §2.3, B1, B5, §2.6)

At `f303d3f`, `PhysicalBody.SharesAsNeeded` (`:137-151`) gives each miner one share for the arms and one for the legs
(Small `armsMuscle` 0.989, `legsMuscle` 0.893). The uncommitted change replaces it with `KeepsWhatItHad`: the region
shares go back to 1, and three new serialized factors carry the calibration, one per joint (`shoulderKept`,
`elbowKept`, `kneeKept`; Small 0.9967 / 0.9817 / 0.8930, Long 1.4018 / 1.3947 / 1.4672, Round 0.7990 / 0.8115 /
0.7995). Under the muscle rule the capacities become `shoulderKept · armsMuscle · 121.3 · (ra/0.047)³ · S`, and the
same for the elbow and the knee. The wrist and the hand's hold now take the arms' share too.

- The formulas in §2.2's `CShoulder`, `CElbow` and `CKnee` rows, and B5 ("at their shares and at shares of 1"), then
  miss a factor. `BodyFrame`'s mirror of the muscle rule must read `ShoulderKept`, `ElbowKept` and `KneeKept` (public
  getters in the new code).
- The miners then keep the need rule's numbers exactly (Small 41.8 / 35.5 / 86.9 N m), not "within 0.3-0.8%".
  B1's tolerance, max(0.6 N m, 1.2%), still passes. The worst cases are the elbows, Small 35.51 and Long 65.49 against
  b3's whole 36 and 65. But the stated reason (§2.3's last row, B1's "why") is then stale.
- The cited lines in `PhysicalBody.cs` move: `BackNow` 205, `ShoulderCapacity` 257, `KneeCapacity` 263-272,
  `ShoulderOf` 275, constants 29 and 235. They are right at `f303d3f` and wrong in the working copy.
- **question (lead / Director).** For the made bodies of §2.6, does a body made from a miner inherit that miner's kept
  factors (Long ×1.25 at Long's 1.40), or is it "a body made since", with factors of 1 (`PhysicalBody.cs` comment;
  body_map §1.5: "a body made from now on shows its build")? It changes the made bodies' arms and legs by up to 40%,
  so their r. At `f303d3f` the design mixes the region shares; after the change, that would mean the kept factors.
- **Correction.** Take the zero-diff alternative of §1.1 for this step. `BodyFrame` stays in `Tests/PlayMode`. The
  constants are copied, and B5 guards the copies against `PhysicalBody`'s public `ShoulderCapacity`,
  `ElbowCapacity`, `KneeCapacity` and `BackCapacity` on the three miners under both rules. Then `PhysicalBody.cs`,
  which the other session is editing, is not touched. Re-read §2.2's capacity rows after that work lands.

### C2. The drawn miner's hands will go through the stage, and Long's arms are 9 cm off (likely; §3.3, step 5)

- **Straight wrists.** `MinerBody.LateUpdate` turns each hand as its forearm (`MinerBody.cs:572, 576`:
  `follows = fore * root * handRest`). The port's "hand" is the forearm capsule's round end, with its contact at
  `wr − ra·z` (`Supports.cs`, b3 `contact_points`). So in every stance with a hand on the ground (quad, tripod, bear,
  the squats with hands down, the half-kneel with hands on the ground, the situp propped on its hands, press), the
  drawn hand carries straight on from a near-vertical forearm. Its length less `ra` goes into the stage, a few
  centimetres or more. §3.3 calls this "a few centimetres"; drawn, it looks broken, and it is exactly what the
  Director is to judge.
- **Long's shoulders.** The model's shoulders are not where the body solves them: 9 cm behind on Long, in the chest's
  frame (`MinerBody.cs:134-137, 573`; `Docs/ArtDirection/TheMiners.md` W5, P1). On hands and knees the chest's back
  faces up, so Long's drawn arms start about 9 cm above the solids' and its hands float. Step 5's bullet "the model's
  joint bones within 15 mm of the fit's joints" cannot pass for Long's shoulders, elbows and wrists.
- **Correction** (a look choice, so show it, do not decide it).
  - Measure first: in step 5, report each drawn hand's lowest point against the stage, and each joint's offset from
    the fit, per miner. Test the arm joints relative to the model's own shoulder (vectors elbow − shoulder,
    wrist − elbow), not as absolute positions.
  - Then offer a drawing-only wrist. After `MinerBody` (order > 50), turn a supporting hand's bone to lie on the stage
    (palm down, fingers on the line of the forearm's horizontal projection).
  - For Long, ask the drawn arm for the wrist less the shoulder's offset, as `MinerBody.WristFor` does for tools
    (`:305-315`).
  - Label tiles that use either.

### C3. Six of the 45 stances lean a forearm on a thigh that the game's body passes through (sure; §3, §5)

The hand-on-thigh supports (`halfkneel` "thigh" and "both", `halfkneel_up` 0.55 / 0.7 "thigh", `stoopknees` 30 / 60;
`Stances.cs:262-271, 316, 341`) push a forearm against a thigh. In the game's body that pair never collides:

- getting up, `OwnBody.Strike` lets forearms strike only the trunk and the head (`OwnBody.cs:585-595`), and
  `Physics.IgnoreCollision` is set for every other pair (`:839-841`);
- standing, every self-contact is ignored.

So these fitted poses are poses the engine's body cannot hold as fitted. Holdability is listed as "not shown" (§5),
but these six are known not to hold, not merely unknown. **Correction.** Mark these tiles "needs a forearm–thigh
contact the game's body does not have". Add it to §5's Open list: a Strike pair, or a different support for layer 3.

### C4. The order of work gives the Director nothing to see until step 8, and the first set has a known flaw (sure)

- The first picture (step 4) waits on the move into Unity (1), the port's tests there (2) and the whole bridge (3).
  But the three miners' solids need only `BodyFactory.Make("Small" | "Long" | "Round")`, the port's own miners.
  Step 3's B1 is the proof that these equal `FromUnit` to 2e-6 m and about 1%, so pictures made before the bridge stay
  valid for the miners. The drawn miner needs the frame map (B4) and `AtlasPoser`, not B1-B3 or B5.
- Strategy §2.2 item 6 already says the stills *will* show free limbs "no living body holds": Long's free elbows at
  107-119°, toes curled 40-70°. Step 9 makes the relaxed-pose pull only after the Director's sheets. That shows Luis a
  flaw the team knew of. It goes against the working agreement's proactive polish, and against showing looks
  additively, early and side by side.
- **Correction, a proposed order.**
  1. After step 1, a **first look**: the miners' solids from `BodyFactory`, for 6-8 key stances (stand, quad, squat
     0.35, half-kneel, bear, situp 25 with hands), four sides. Make it both without and with `RelaxFreeLimbs` (step 9's
     switch), side by side and labelled. The judges see it first, then Luis.
  2. Then steps 2-3 (bridge).
  3. Then 5 (the drawn miner, with C2 measured), 6 and 7.
  4. In step 8, both free-limb sets go out together.
  If even step 1 is too slow, the console runner can dump each fit's capsules and boxes to JSON, and
  `atlas_sheets.py` (needed anyway) can draw orthographic views with Pillow, with no Unity in the loop.

### C5. The turntable's sign and the view labels (likely; §2.4, §3.4)

`u(p) = O + Yaw(ψ)·M·p`, with ψ = 90 called "its right side". In Unity, `Quaternion.AngleAxis(90, Vector3.up)` turns
forward to right, clockwise seen from above. So with the camera in front, ψ = +90 shows the body's **left** side.
Only an AnyBody-sense yaw (counter-clockwise about z, before M) shows the right. A tile labelled "right" that shows the
left would put "(left hip)" beside the wrong hip. **Correction.** Define each view by the side that must face the
camera, not by ψ. Add a test per view: in a "right" tile the right shoulder (and hip) is nearer the camera than the
left; in "front" the chest faces it.

### C6. `TimeOfDay` re-aims the sun every frame (likely; §3.4)

`TimeOfDay.Update` calls `Apply()` each frame (`TimeOfDay.cs:136-139`). `Apply()` sets
`sunAndMoon.transform.rotation = LookRotation(−lightDirection)` from the hour (`:233-238`). So "the sun 40° to the
camera's left" cannot be set on the light while `TimeOfDay` runs. (`minutesPerSecond` is 0 in TheOrdinaryPlace, so
the hour does not drift.) **Correction.** Put the camera's azimuth where the 13:00 sun falls 40° to its left; the
stage spot is fixed, the camera's direction is free. Or set the hour, then disable `TimeOfDay` and turn the light.

### C7. Which LOD is drawn depends on where the unit's root is (likely; §3.3)

The miners carry an `LODGroup` with LOD0 above 18% screen height (`MinerSetup.cs:411-419`). `MinerBody` writes world
transforms and the root's rotation cancels out of its turns, so the bones can be drawn far from the unit's root.
But the LODGroup measures from its own reference point under the root. A unit left where it stood while its body is
drawn on the stage can come out in LOD1 or LOD2. **Correction.** Warp the unit's root to O (agent off) for every drawn
shot, or `ForceLOD(0)` while capturing. Assert LOD0 in step 5.

### C8. Only the limb segments' `localScale.y` may be written (sure; §3.3)

The feet and toes' `localScale` is their box: 0.10 × 0.06 × (heel + ball), and 0.10 × 0.05 × toe
(`MinerSetup.cs:365-366`). `ProceduralBiped` reads it (`:1292-1297`), and `OwnBody.Make` takes it as the sole and toe
boxes (`OwnBody.cs:725, 816`). Pelvis, torso and head are at scale 1. "Each segment's `localScale.y` set to the model's
half-lengths" must read "each upper arm, forearm, thigh and shin segment's". Written on a foot, it would change the
boot that the next `OwnBody` makes.

### C9. The hung things are kicked by every turn (likely; §3.3)

A 90° turn of the whole body about the stance's centre moves each hung thing's pivot up to about 1 m in one frame.
`MinerBody.Hang` reads that as a carried speed (`(pivot − hangFrom)/dt`, `MinerBody.cs:729-736`) and swings. It resets
only for a jump over 1 m (`:711`). With the miners' damping of 0.8-0.9 per sixtieth (`miners.json`), 15 frames at 30
fps leaves several per cent of the kick. Step 5's 5 mm/s test would fail or flicker. **Correction.** Turn over about
a second, then hold until every hung thing is below 5 mm/s (with a cap, and a note when the cap is hit), not a fixed 15
frames.

### C10. Extra starts take the place of the noisy restarts (sure; §3.1 step 1)

`StanceFitter.Fit` uses `ExtraStarts[extra]` instead of the noise for the first `ExtraStarts.Length` restarts
(`StanceFitter.cs:75-86`), with `Restarts` 2 by default.

- A made body with one neighbour loses run3's `Noise1` start.
- A halfway body with two parents loses **both** noisy restarts.

README's `--atlas-extra` result (21 → 17 worse) was measured with one extra start only. **Correction.** Set
`Restarts = 2 + ExtraStarts.Length` for made bodies, so they keep the tested noisy starts. Or state that the halfway
bodies run an untested setting.

### C11. Ranges: the Open item is wider than the knee (sure; §2.3 last row, §5)

b3 allows more than layer 0 in places (`BodyModel.cs:276-281` against `JointFrames.cs:33-39`):

| joint | b3 | layer 0 |
|---|---|---|
| elbow | 145° | 140° |
| shoulder flexion | 170° | 150° |
| neck rotation | ±50° | ±60° |
| ankle inversion | ±20° | −15..25° |
| knee | 150° | 155° |

In the 765 saved prototype fits with a cost under 0.5, layer 0's ranges are exceeded only by:

- five shoulder flexions of 151-155° (Long variants, bears);
- one hip extension of −15.6°;
- one inversion of −15.5°.

So it is rare, but such a pose is one the anatomical body cannot take. **Correction.** Print a "beyond layer 0's
range" mark on those tiles. Word §5's item as the whole set, not the knee alone.

### C12. Smaller corrections

- **sure.** "Unity's profile has no general JSON reader" (§4, step 2) is not true here. `com.unity.nuget.newtonsoft-json`
  3.2.1 comes in through `ml-agents` → `ai.inference`, and its DLL is auto-referenced (`isExplicitlyReferenced: 0`).
  The CSV is still the better choice, because it does not tie the test to ML-Agents staying. But **commit it**:
  `port/.gitignore` drops every `reference/*.json` but `body_ref_bodies.json`, so on a fresh clone step 2 has nothing
  to read unless the golden CSV is in git.
- **likely (hygiene).** With `"overrideReferences": false`, the AnyBody assembly auto-references every auto-referenced
  plugin in the project: Newtonsoft.Json, Google.Protobuf, Grpc.Core, System.IO.Abstractions, Mono.Cecil and others.
  "It references nothing" (§1.4) holds only with `"overrideReferences": true` and `"precompiledReferences": []`. No
  clash found either way.
- **sure (wording).** "No generic methods at all" (§1.5): none is declared, but generic methods are called
  (`Array.Empty<T>`, `HashCode.Combine`, `List<T>.ToArray`), and `ToString`, `Equals` and `GetHashCode` are
  overridden. IL2CPP's conclusion stands: every instantiation is closed and visible. The rest of §1.5 checked out: no
  `#if`, `#nullable`, unsafe code, Span, `IsFinite`, LINQ, records, `init` or interfaces in `src`.
- **sure (minor).** The head segment: `ProceduralBiped` puts it on the chest's axis (`waist + chest·(0, headRise, 0)`,
  `:1242`), not at headC. The two agree only with the head unturned. `MinerBody` reads only the head segment's
  rotation, so drawing is unaffected. But §2.4's "where ProceduralBiped puts the segments" is not exact for the head.
- **likely (B4).**
  - At a bow of up to 3°, which `CanStand` allows (`OwnBody.cs:55, 371`), the head differs from the chest by up to 60%
    of it (`Slerp(facing, posture, .4)`, `ProceduralBiped.cs:1243`). That moves the neck up to about 5.5 mm on Small
    and 6.3 mm on Long, against B4's 5 mm. Take the tolerance from the head's actual turn.
  - Take hS from the shoulders' anchors in the trunk frame, not from the neck's.
  - B4 meets the signs only near standing: abduction, rotation and inversion are a few degrees. Add a round trip on
    100 random poses: `AtlasPoser` writes, `ReadPose` reads back, q within 1e-6.
  - Note for layer 3: the root's Rz·Ry·Rx loses yaw against roll at pitch ±90°, which is lying flat, and the
    shoulder's and hip's Ry·Rx·Rz do the same at abduction ±90°.
  - `JointFrame.Side` is +1 right (`JointFrames.cs:47`), the port's s is +1 left: never pass one for the other.
- **likely (B2, B3).**
  - `MinerChoice.Awake` wakes the miner remembered in PlayerPrefs (`MinerChoice.cs:47-53`), so which miners are
    "never woken" at load depends on the last choice made on this machine. Check `activeSelf == false` before
    reading, rather than assuming Small, Long or Round.
  - Call `PhysicalBody.Refresh()` before `SetOwn(true)` in B3, because `forceLimit` is made with `Fresh()`.
- **sure (labels).** The sheets print S under the name "r" (§3.1, §3.6). The strategy's tables and `res_*.json` call
  the vertex-dependent share r. REVIEW F2 shows they differ, by up to 0.04. Print "S", or "reserve S", and define it
  in the legend. Gating on S follows F2's recommendation, but it is still the lead's or the Director's call: list it as
  decided or Open, do not leave it implied.
- **likely.** A test coroutine resumes before `LateUpdate`, so a render right after changing the pose shows the last
  frame's bones. Render only after at least one `LateUpdate` since the last change (the hold frames do this for the
  drawn look; the solids need it too, if they are placed through `AtlasPoser`).
  `GameObject.CreatePrimitive` gives every cylinder, sphere and cube a collider: destroy them on the solids and the
  stage.
- **sure (minor).** The selection ring shows whenever the unit is selected (`SelectableUnit.cs:11-12`), and
  `MinerChoice.Choose` keeps the selection. Call `SetSelected(false)` before the drawn shots.
- **sure (from REVIEW).**
  - F8: `Margin.RoundHull` is a public static that the console tests flip. Nothing may touch it while `-atlasThreads`
    fits run.
  - F5(b): the port has one length per limb for both sides. `FromMade` should assert that left and right agree, which
    they do for the miners, rather than silently take one.
- **trivial.** `Time.captureFramerate = 30` is not in `OwnRiseBench.cs:200-213` (no bench sets 30; `BodysOwnBench`
  uses 50). The Small column has two truncations: CAnkle 60.86 (written 60.8), CToe 18.26 (18.2).

### C13. Checked and confirmed (no correction needed)

- **§1.** The asmdef layout: `Scripts/WonderGather.Runtime.asmdef` claims `Scripts/` (`rootNamespace` WonderGather);
  `PlayModeTests.asmdef` takes names.
  - `Directory.Build.props`: four levels up from `port/` is the repository root. No `Directory.Build.*` lies above it.
    The root `.gitignore` ignores `*.csproj`, `*.sln` and `/[Cc]aptures/`, not `*.props`.
  - The 21 files are CRLF throughout, none mixed, and `.gitattributes` has `*.cs text eol=lf`.
  - There are 59 port type names, none duplicated and no partial classes, against 236 game type names, with none in
    common. "AnyBody" appears in the game only in comments. Of UnityEngine's types, `Pose` and `ContactPoint` are the
    clashes. Inside `namespace WonderGather` and `WonderGather.Tests`, `AnyBody.X` resolves through the enclosing
    `WonderGather`.
  - `apiCompatibilityLevel: 6`, a scripting backend only for Android (Mono, `ProjectSettings.asset:841-842, 934`).
- **§2.1-2.3**, every row at its line:
  - `OwnBody.Part` `:347`; the constants `:51`, `ToeGives :180`, `HingesStop :195`, `Weighs :470-480`;
  - `Make` `:647-830`: the girth, breadth, rods, anchors, sole and toe boxes, friction `:655`, matchAnchors `:766`,
    `WeighCentres` only when `Weighed` `:803`;
  - `OwnKeeper.Soften` `:220-229`, `OwnRiser` tone `:353-376`;
  - `MinerSetup.cs:324, 336, 365-366, 377, 380`;
  - `ProceduralBiped.cs:43-65, 95-97, 978, 1070-1075, 1238-1303, 1413`;
  - the Small prefab: `hipHeight` 0.605714, masses 7.8801 / 8.9600 / 9.9619 / 1.1183 / 1.1108 / 0.8044 / 2.383 /
    1.3501 / 1.0749 / 0.2119, 40.679 kg, `upperAsks` 10.9223 (→ 257.2 N m), capsule 1.42 m;
  - the PhysicalBody lines at `f303d3f` (see C1);
  - `MinerSource`, `BodyVariant` and `Make(v)`'s use of `Miner(v.Source)` (`BodyModel.cs:241-453`). `H`, `TR` and
    `Height` are not used by the fit: `HipHeight` is recomputed and `TrunkCentre` is hS/2.
- **§2.4, the frame map.** `M` and the link-axes matrices are each improper (det −1), so `M·R·C` is a proper Unity
  rotation. The axes agree with `JointFrame.Of`'s zeros (`JointFrames.cs:56-85`) and with how `OwnBody.Make` builds
  limb frames (`Along`/`Bends`, `OwnBody.cs:449-468, 700-723`: y towards the far joint, z the side the joint sticks out).
  - A worked case, the left shoulder abducted 90°: the frame is proper, the arm points left and the elbow back.
  - The left side stays left: index 0 is left in both the port and `MinerBody`.
  - The Euler orders and signs in the joint table are `Kinematics.Forward`'s (`Kinematics.cs:100-150`) and b3.fk's.
  - `MinerBody`'s ends and poles read these frames as `OwnBody.LateUpdate` writes them (`MinerBody.cs:456-500`,
    `OwnBody.cs:1308-1331`).
  - The foot box centre `an + R(½(bl−hl), 0, −a+½fh)` and the toe box `ball + R(½tl, 0, ½th)` match
    `ProceduralBiped.cs:1293, 1297`.
- **§3.**
  - `MinerBody` order 50, `TurnsAsSolved :195`, `Ends :456-462`, `Poles :477-500`, `Hang :676`; `LetGo`
    `ProceduralBiped.cs:570-574`; `PhysicalBalance.Acts` `OwnBody.cs:856-858`. Nothing else writes the segments in
    `LateUpdate`: only `OwnBody` (41) and `PhysicalFall` (40) do, both idle here.
  - The capsule scaling of `BodysOwnBench.Shape` (`:240`) is exact only at four radii, as said.
  - `CaptureTools.cs:8-9, 35-65`, `MinerCloseCapture.cs:22-25, 91-93`, `OwnRiseBench.cs:199-210, 238-245`.
  - `Art/Review/sheets.py`, `Docs/Technical/RiseWays/stagesheet.py`, `Art/Review/judges/` and Pillow 12.1.1 exist.
  - The counts: 45 stances in `PrototypeBatch`, 2340 + 1080 = 3420 tiles, 585 rows, 104 sheets, 2.7 GB of PPM.
- **Not checked here:** nothing was compiled in Unity or run. The Mono timings and the 3 + 10 minute estimate stay
  estimates.
