# Physical characters in shipped games and engine tools: how they get up, and whether any do it on their own joints alone

Scope: shipped games, game-engine tools/middleware and studio research (October 2026). Each claim is tagged by where it comes from:
- **[DEV]**: the developer said it or published it (talk, documentation, released source code, developer forum post).
- **[PRESS]**: a journalist's description, sometimes based on a developer interview.
- **[PLAYER]**: players or the modding community, i.e. inference from outside.
- **[SNIPPET]**: seen only in a search-result excerpt, because the page itself could not be fetched (403/402). Weaker evidence.

For each game or tool the notes give a verdict: whether the get-up is driven only by joint torques, what help from outside the body is used, how natural it looks, and how robust it is.

Correction to the brief: **DReCon (2019) is Ubisoft La Forge + McGill University, not EA SEED** (see question 6).

---

## 1. Euphoria / NaturalMotion titles (GTA IV, GTA V, Red Dead Redemption 1 and 2, Backbreaker, Star Wars: The Force Unleashed, Max Payne 3): do characters get up physically or through an animation blend?

### Takeaway
No developer source says that Euphoria characters get up physically. Rockstar itself describes Euphoria as a layer for *physics-based reactions* inside a larger animation system. Modding evidence (a "blend from NaturalMotion" task, and peds that automatically stand up when a ragdoll timer runs out) fits get-ups being animation clips blended out of the Euphoria pose. **Verdict: not a joint-torque get-up. This is an inference with fairly high confidence, not something the developers confirmed.** Euphoria's fame comes from balance, stumble and fall reactions, not from getting up.

### Cited Findings
- **[DEV/PRESS]** Euphoria is "Dynamic Motion Synthesis", animating characters on the fly "based on a full simulation of the 3D character, including body, muscles and motor nervous system". Titles: GTA IV (the first), Red Dead Redemption, The Force Unleashed I and II, GTA V and RDR2 (with Euphoria integrated into RAGE), Backbreaker ("generates tackles in real time"), Max Payne 3, and Clumsy Ninja (the first mobile title). NaturalMotion stopped licensing Euphoria in 2017. — [Wikipedia: Euphoria (software)](https://en.wikipedia.org/wiki/Euphoria_(software))
- **[DEV/PRESS]** At launch NaturalMotion described Euphoria as synthesizing animation at run time "using fast simulations of the game character's actual motor control, muscles and biomechanics". Endorphin was the offline predecessor. — [Game Developer: NaturalMotion to launch Euphoria](https://www.gamedeveloper.com/game-platforms/naturalmotion-to-launch-euphoria-real-time-anim-technology)
- **[DEV]** Phil Hooker, director of technology at Rockstar North, October 2018: "We use Euphoria specifically to enhance the physics-based reactions of both humans and animals", and "Euphoria is just a part of how we simulate and animate characters". He also said they had "radically overhauled our entire animation system" for RDR2. — [Wccftech, reporting a VG247 interview](https://wccftech.com/rockstar-euphoria-evolved-rdr2/amp/)
- **[PLAYER]** A GTA V mod that ports Max Payne 3 Euphoria tasks lists a `BlendFromNaturalMotion` task, described as helping Euphoria blend more naturally when a ped dies. This points to an explicit NM→animation blend stage in the RAGE pipeline. — [gta5-mods: NaturalMotion Euphoria Mod](https://www.gta5-mods.com/tr/misc/naturalmotion-euphoria-oiv-kaai210-bravercoolio-jedijosh920)
- **[PLAYER]** Euphoria-overhaul mods for GTA V work by sending more "stayUpright" messages, adding "bodyrollup" tasks and varying "InjuredOnGround" (for example rolling sideways). The behaviours being tuned are all for staying up, falling and lying down; none for getting up was mentioned. — [gta5-mods: Euphoria Ragdoll Overhaul (ERO)](https://www.gta5-mods.com/cs/misc/euphoria-ragdoll-overhaul-ero)
- **[PLAYER]** In GTA V multiplayer (FiveM), a ragdolled ped automatically "gets back up" once the ragdoll time set through `SET_PED_TO_RAGDOLL(ped, time1, time2, ragdollType, ...)` expires. Server developers ask for natives to block this, and one workaround "seems to remove the 'getting up' effect". — [cfx.re forum thread](https://forum.cfx.re/t/add-native-to-hinder-players-from-getting-back-up-again-automatically-when-ragdolled/5215234); [cfx.re: ragdoll script](https://forum.fivem.net/t/release-simple-ragdoll-script/4793)
- **[SNIPPET/PLAYER]** A GTA fan wiki says ragdoll physics first appeared in GTA IV and that characters "still conform to some preset animations". — [GTA Wiki: Ragdoll Physics](https://gta.fandom.com/wiki/Ragdoll_Physics) (page returned 402, so only the excerpt was seen)
- **[PRESS]** In 2007, GTA IV's use of Euphoria was assumed by fans rather than confirmed in the Rockstar–NaturalMotion partnership announcement. — [gta4.net, 2007](https://www.gta4.net/news/3828/euphoria-engines-possibilities-for-gta-iv/)

### Inferences
- "Ragdoll timer expires → ped gets up", together with an explicit blend-from-NM task, is the classic architecture: a simulated reaction, then a blend into a get-up clip chosen by whether the body lies face up or face down. Nothing found suggests Euphoria behaviours drive the rise itself.
- Euphoria's PD-driven "motor control" only ever had to keep a body up or shape a fall over roughly 1–3 s. It was never asked to lift a body off the floor against gravity, which is the hard case for the miners.

### Gaps
- No NaturalMotion behaviour list or GDC talk was found that confirms or rules out a "get up" behaviour in Euphoria. The 2009 Euphoria developer interview video ([YouTube](https://www.youtube.com/watch?v=BtUwD_yGoSI)) was not transcribed.
- No source found for how get-ups work in Backbreaker, The Force Unleashed or RDR2 specifically. NaturalMotion's own documentation is not public.
- Whether the root or hips get external "balance help" during Euphoria balancing is undocumented.

---

## 2. Exanima (Bare Mettle): fully physics-driven characters. How do they get up, and do the developers say anything about balance or support forces?

### Takeaway
Exanima is the shipped commercial game whose developers most clearly say that human motion is physics-driven, with an "AI" that controls how tense each virtual muscle is. It is the nearest commercial relative of the miners' setup. **However, no developer statement was found on how getting up works or on whether any balance, root or support forces are used.** The developers also concede that the motion looks "a bit silly" for new players, and players complain about frequent tripping.

### Cited Findings
- **[DEV]** Developer Kieran, Exanima 0.8 post (1 April 2020): "Our physics driven character motion has come a long way, it works well and in the hands of a very experienced player it can look very good"; "It will realistically always feel tricky and look a bit silly for a new player"; the update introduces "a sort of better "AI" control over how tense each virtual muscle should be at any time", which "creates generally smoother motion, removes a lot of jittery moments and improves physics interactions"; "As we've tightened up the animations over time, they lost a bit of their physics feel, and this brings some of that back"; "Characters are better at remaining balanced under harsh conditions in general", "resulting in less tripping". — [Bare Mettle forums: Exanima 0.8](https://www.baremettle.com/forums/index.php?threads/exanima-0-8.12202/)
- **[DEV]** Exanima 0.7.3 changelog (July 2019): "Improved character balance on uneven ground"; "Removed a cause of frequent tripping during locomotion"; "Improved dynamic and rapid recovery from tripping during locomotion"; "Smoother recovery from staggers". Nothing about getting up from the ground. — [GOG forum: Exanima changelog](https://www.gog.com/forum/exanima/changelog)
- **[SNIPPET/DEV]** A developer reply on GOG says everything that moves is subject to physics and accurate collisions, and calls animation-driven interaction "just not realistically possible". — [GOG forum: In development Exanima, page 5](https://gog.com/forum/general/in_development_exanima_1841d/page5)
- **[DEV]** Lead developer Madoc Evans notes that because the physics is accurate, a weapon's shape can matter more than its stats. — [myArmoury forum](http://myarmoury.com/talk/viewtopic.32808.html)
- **[PLAYER]** A 2023 Steam thread complains that characters trip and fall too easily and suggests the developers learn from Half Sword. — [Steam discussion](https://steamcommunity.com/app/362490/discussions/0/3196989600451168411/?ctp=4). Another player reports characters that "faceplant instantly". — [Steam discussion](https://steamcommunity.com/app/362490/discussions/0/3884977551668880820)
- **[PLAYER]** A 2016 thread about "clumsy animations" was answered by a player, not a developer, saying the motion is real-time simulation and motion synthesis rather than canned animation. — [Steam discussion](https://steamcommunity.com/app/362490/discussions/0/496881136903255964)

### Inferences
- "We've tightened up the animations … they lost a bit of their physics feel" suggests an architecture in which a physical body tracks authored target motions through muscle tension. That is the same family as tracking controllers, not free-form motor control. Whether a getting-up target motion is tracked under joint torques alone, or with help, is unknown.
- The developers' own wording ("tricky", "a bit silly") and players' tripping complaints are the price of keeping the body physical in a serious (non-comedy) game.

### Gaps
- No developer statement on getting up, on root or upright support forces, or on strength limits. The Sui Generis Kickstarter page, the likely place for a technical pitch, returned 403.
- A player review's claim of "over 200 simulated muscles" was seen only in search excerpts and is unverified.

---

## 3. Overgrowth (Wolfire): the active-ragdoll get-up

### Takeaway
**Confirmed from Wolfire's released source code: the get-up is keyframed animation.** While falling the character is an active or limp ragdoll. Once a recovery timer has run out and the ragdoll's average velocity is below a threshold, `WakeUp(_wake_stand)` stores the ragdoll pose and plays an authored get-up clip, "stand from front" or "stand from back". Nothing about the rise is physical.

### Cited Findings
- **[DEV, source code]** `Data/Scripts/aschar.as` (Overgrowth, open-sourced under Apache 2.0):
  - State comment: `const int _ground_state = 1;  // character has fallen down or is raising up, ATM ragdolls handle most of this`
  - The wake types are `_wake_stand`, `_wake_flip`, `_wake_roll`, `_wake_fall`, `_wake_block_stand`.
  - In `HandleRagdollRecovery`, the wake is triggered when `recovery_time <= 0.0f && length_squared(this_mo.rigged_object().GetAvgVelocity()) < _auto_wake_vel_threshold`, with `_ragdoll_recovery_time = 1.0f`.
  - `// WakeUp is called when a character gets out of the ragdoll mode.` On `_wake_stand` it calls `PlayStandAnimation(...)`, which calls `SetAnimation("Data/Animations/r_standfromfront.anm", 20.0f, flags)` or `"Data/Animations/r_standfromback2.anm"`, plus `r_blockfrontfromground.anm` for blocking. When the ragdoll is left, the bone transforms are stored (`ragdoll_pose[i] = skeleton.GetBoneTransform(i)`) so the clip can blend from them.
  — [GitHub: WolfireGames/overgrowth aschar.as](https://raw.githubusercontent.com/WolfireGames/overgrowth/main/Data/Scripts/aschar.as)
- **[DEV]** The codebase is public on GitHub under Apache 2.0. — [gittrend: WolfireGames/overgrowth](https://gittrend.io/repo/WolfireGames/overgrowth)
- **[DEV]** David Rosen's GDC 2014 talk "An Indie Approach to Procedural Animation" covers interactive animation built from very few keyframes (Overgrowth, Receiver, Black Shades). — [GameAnim summary](https://www.gameanim.com/?p=15568); [Game Developer](https://gamedeveloper.com/design/video-an-indie-approach-to-procedural-animation). A forum summary says roughly 13 keyframes in total. — [Unity forum](https://discussions.unity.com/t/an-indie-approach-to-procedural-animation-gdc-video-talk/538228)
- **[DEV]** Alpha changelogs mention "Faster active ragdoll animation" and "Fixed slowdown when transitioning from ragdoll to animation". — [DSOGaming, Alpha 151](https://www.dsogaming.com/?p=8505); [DSOGaming, Alpha 209](https://www.dsogaming.com/videotrailer-news/overgrowth-new-video-highlights-changes-made-to-alpha-209-version/)

### Inferences
- Overgrowth looks fluid because the rise is a spring-interpolated keyframe pose, not a struggle against gravity. It is robust because the rise never fails. The comment "ATM ragdolls handle most of this" shows that even Wolfire left the ground phase to a ragdoll (physics) up to the wake moment, then handed over to animation.

### Gaps
- The blend weight and duration from `ragdoll_pose` into the clip were not traced in full. The roughly 180 KB of the file after offset 400k was not read.

---

## 4. Physics-comedy and indie games: Toribash, Gang Beasts, Human: Fall Flat, Totally Accurate Battle Simulator, Stick Fight, Rain World, Spore, Ragdoll Runners, QWOP, Getting Over It (plus Sumotori Dreams, Half Sword, Wobbledogs)

### Takeaway
The best-documented case, **TABS (Landfall), uses an explicit hidden upward force on the torso and head**, scaled by the head's height above the floor, plus balance forces on the legs and feet. "Getting up" is that force ramping on when the unit is "able to stand". Gang Beasts and Human: Fall Flat are widely believed by developers and players to use similar upper-body holding forces, but **no developer confirmation was found for either**. Fully joint-driven bodies exist in **Toribash** (the player sets each joint's state, turn by turn) and **QWOP** (the player drives thighs and calves), but there the *player* is the controller, and getting up is a skill most players fail. **Sumotori Dreams (2007)** is the historic shipped example of physics bodies that try to right themselves on their own. Its developer's follow-up page claims balance "with no cheating", but no technical detail is available.

### Cited Findings
**Totally Accurate Battle Simulator / Landfall**
- **[PRESS, from a developer interview]** Landfall's standing handler checks "the distance between the character's head and the floor", then "applies an upward force to the torso and head if the character is able to stand". The force follows an animation curve "where X is the distance to the ground Y is the output force". Walk cycles apply torque to the ragdoll's legs, also guided by animation curves. — [Game Developer: How Landfall finds the fun in physics engines (Oct 15, 2018)](https://gamedeveloper.com/design/how-landfall-games-finds-the-fun-in-physics-engines)
- **[DEV]** Wilhelm Nylund, Landfall CEO, in the same article: "The balance script adds force to the legs and feet to try to keep them in position to help support the body"; "if both feet are in front of the bodies center of mass one of them needs to move backwards"; "In the old system there was a script called PhysicsAnimation.cs which handled keeping the character upright". — [Game Developer](https://gamedeveloper.com/design/how-landfall-games-finds-the-fun-in-physics-engines)
- **[PRESS]** A guide describes every wobbler as "a physics driven ragdoll that balances, swings and falls over on its own", and a reviewer has units "flailing around like drunk toddlers". — [TABS wiki](https://tabs.wiki.gg/wiki/TABS); [bit-tech review](https://bit-tech.net/reviews/totally-accurate-battle-simulator-review/1)

**Gang Beasts (Boneloaf)**
- **[DEV]** Developer James Brown (2014) says the team "evolved this physics system" over game jams and that it's "probably too bold to call it a system". He mentions a coming climbing mechanic to "recover from somebody trying to kill you". Nothing about balance forces. — [bit-tech interview](https://bit-tech.net/reviews/gaming/pc/interview-gang-beasts/1/)
- **[SNIPPET][PLAYER/other developers]** A Unity forum guess at Gang Beasts' method: the head is held up just below standing height with a soft constraint so the foot animation can lift it. Another reply describes "constantly applying some force to an upper-body-part to make it stand upright". — [Unity forum: How to create a physics-based ragdoll controller like Gang Beasts](https://discussions.unity.com/t/how-to-create-a-physic-based-ragdoll-character-controller-like-gangbeasts/595806); [Unity forum: Physics-based animation system](https://discussions.unity.com/t/physics-based-animation-system-for-characters/652307)

**Human: Fall Flat (No Brakes Games, Tomas Sakalauskas)**
- **[SNIPPET][DEV]** The game grew out of "experimenting with the Intel RealSense camera and a desire to implement procedural character animation". It runs on Unity's PhysX ("I had to stay within the limits of existing engine in order to keep the scope of the project manageable by solo dev"). — [TheGamer interview](https://www.thegamer.com/human-fall-flat-interview-tomas-sakalauskas/) (seen through search excerpts of TheGamer interview pages)
- **[SNIPPET][DEV]** He used procedural animation because no one would motion-capture the character for him. — [bit-tech preview](https://www.bit-tech.net/previews/gaming/human-fall-flat-preview/1)
- **[SNIPPET][PRESS]** Each trigger reaches out one arm, and the hand sticks to whatever it touches, which is how Bob hauls himself up ledges. — seen in a search summary covering [Xbox Wire, GDC 2016](https://news.xbox.com/en-us/2016/03/17/gdc-2016-human-fall-flat-xbox-one/) and store pages; the exact source page was not confirmed

**Toribash (Nabi Studios)**
- **[DEV/community wiki]** Every joint has four modes: relax ("will move freely, with the body"), hold ("will try not to move unless forced"), extend and contract. Players set them while time is paused, then about ten frames are simulated. — [Toribash wiki: joints](https://toribash.fandom.com/wiki/Shoulder); [Fun-Motion](https://www.fun-motion.com/?p=88)
- **[PRESS]** It is a rigid-body simulation with dismemberment that "does away with keyframes entirely". — [Fun-Motion](https://www.fun-motion.com/2006/08/)

**QWOP (Bennett Foddy)**
- **[PRESS]** A "ragdoll-based video game" in which the player can "command movement of the runner's thighs and calves" with Q, W, O and P, "while trying to avoid falling over". Pressing R restarts the attempt. — [Wikipedia: QWOP](https://en.wikipedia.org/wiki/QWOP)

**Rain World (Videocult, Joar Jakobsson)**
- **[DEV]** "I have a bunch of points in space and I connect them at certain distances"; "All of the creatures in Rain World are pieced together in this way", making them "soft and bendable". — [Game Developer: Crafting the chaotic ecosystem of Rain World](https://gamedeveloper.com/design/crafting-the-complex-chaotic-ecosystem-of-i-rain-world-i-)

**Sumotori Dreams (Peter Soltesz, 2007)**
- **[PRESS]** A 96 KB "tech demo for procedural animation". Steve Theodore (Game Developer) called it "the future of animation" for "entirely physics-based movements that did not rely on animators or motion capture". Kieron Gillen found "the attempts to stand up and bow" funny. — [Wikipedia: Sumotori Dreams](https://en.wikipedia.org/wiki/Sumotori_Dreams)
- **[PRESS]** A Destructoid review says that when the bots are knocked off balance, "their internal physics AI will attempt to right itself", and that most of the character's motion comes from that AI rather than button presses. Its comparison to "a variation of Euphoria" is the reviewer's, not the developer's. — [Destructoid](https://vip-develop.destructoid.com/?p=14016)
- **[SNIPPET/DEV]** The developer's follow-up project page (Goofball Goals) says the AI balances the characters as much as it can, "with no cheating". — [ModDB: Goofball Goals](https://www.moddb.com/games/goofball-goals) (403 on fetch; seen only as an excerpt)

**Half Sword (Half Sword Games, early access 30 January 2026)**
- **[PRESS]** A physics-based medieval combat game with freeform "ragdoll" controls driven mainly by mouse movement. — [Wikipedia: Half Sword](https://en.wikipedia.org/wiki/Half_Sword)
- **[PLAYER]** Steam threads report the character "having trouble simply standing up" and balance getting worse after an update, compared with an earlier playtest. — [Steam discussion](https://steamcommunity.com/app/2397300/discussions/0/3949154954318457031); [Steam discussion](https://steamcommunity.com/app/2397300/discussions/0/597397269593988553)

**Wobbledogs (Animal Uprising)**
- **[DEV/publisher]** The dogs are "physically simulated all the way down to their guts", and each body moves differently according to its structure. No balance method is described. — [Gematsu](https://www.gematsu.com/2022/11/pet-simulation-game-wobbledogs-coming-to-switch-on-november-17)

### Inferences
- Comedy physics games accept hidden upright forces because the wobble *is* the joke, and an upward pull on the head reads as "drunk" rather than "fake". TABS shows the typical recipe: a force tied to head height, through a hand-tuned curve, that switches on when standing is allowed.
- The only shipped titles where the body is driven by joint actions alone (Toribash, QWOP) hand the controller problem to the player, and neither treats getting up as an automatic behaviour.
- Sumotori Dreams is the closest shipped precedent for *automatic* self-righting with no claimed cheating, but it is only a claim, the bodies are stylised dummies with simplified proportions, and the result reads as comedy.

### Gaps
- **Human: Fall Flat:** no developer statement found on how Bob stays upright or gets up (upper-body force, a hip "ball", or joints only). Community decompilation identifiers could not be confirmed.
- **Gang Beasts:** no developer confirmation of the head-holding force; only community guesses.
- **Stick Fight (Landfall), Spore, Ragdoll Runners, Getting Over It:** no sources found or fetched in this pass. From prior knowledge, which is unsourced and should be verified: Spore's creatures were animated procedurally and kinematically, not by physics; Getting Over It's character sits fixed in a cauldron and has no get-up. Treat both as unverified.
- **Rain World:** no source found on whether creatures right themselves through forces applied directly to their points versus joint-like constraints. The two-chunk slugcat body was seen only in a search summary.
- **Exanima vs Half Sword:** no developer statements comparing balance assists.

---

## 5. Unity PuppetMaster (RootMotion): pin weights, muscle weights and the get-up. Which forces are non-physical?

### Takeaway
PuppetMaster's documentation spells out the split. **Muscles** are ConfigurableJoint Slerp drives, i.e. joint torques inside the body. **Pins** are plain `AddForce` calls pulling each ragdoll bone towards its animated target, i.e. external, non-physical forces. **Mapping** blends the *visible* skeleton between the ragdoll and the animation. The BehaviourPuppet **GetUp** state blends the animation target from the ragdoll pose into a prone or supine get-up clip and **regains pin weight** during the rise. So the stock get-up = animation target + external pin forces (+ optional visual mapping blend). **Verdict: not a joint-torque get-up**, although it can be configured as one (pin weight 0) at the cost of the rise failing.

### Cited Findings
- **[DEV]** "Pin weight": "the weight of pinning the muscles to the position of their animated targets using simple AddForce". "Muscle weight": "the normalized strength of the muscles". "Mapping weight": "the weight of mapping the animated character to the ragdoll pose". "Muscle spring": "the positionSpring of the ConfigurableJoints' Slerp Drive". "Pin distance falloff" "reduces pinning force the farther away the target is". "Pin pow" "adjusts the slope of the pinWeight curve". — [PuppetMaster docs: PuppetMaster Component](http://root-motion.com/puppetmasterdox/html/page5.html)
- **[DEV]** Overview: the Target is "the normal animated character", and the Puppet is a ragdoll duplicate; the goal is to "make the Puppet ragdoll physically follow the motion and animation of the animated Target character". Each muscle "automatically calculates Joint target rotations, pinning forces and other values". Pins are described as "spring joints pulling each ragdoll bone towards their animated target". After physics, the Target is mapped to the Puppet's pose, blendable per muscle. — [PuppetMaster docs: Overview](http://root-motion.com/puppetmasterdox/html/page3.html)
- **[DEV]** BehaviourPuppet states are Puppet, Unpinned (balance lost; muscles weakened by `unpinnedMuscleWeightMlp`) and GetUp, "triggerred automatically after 'Get Up Delay'". `maxGetUpVelocity`: "will not get up before the velocity of the hip muscle has come down to this value". `blendToAnimationTime`: "the duration of blending the animation target from the ragdoll pose to the getting up animation". The events `onGetUpProne` and `onGetUpSupine` fire for face-down and face-up starts. `getUpOffsetProne/Supine` offset the target from the hip bone to prevent sliding. `getUpRegainPinSpeedMlp`: "regain pin weight speed multiplier while in the GetUp state". Raising the GetUp multipliers helps keep the character from losing balance again right after switching to GetUp. — [PuppetMaster docs: BehaviourPuppet](http://root-motion.com/puppetmasterdox/html/page10.html)
- **[DEV]** `knockOutDistance`: "if the distance from the muscle to its target is larger than this value, the character will be knocked out". Collision unpinning is set by `collisionResistance`. — [PuppetMaster docs: BehaviourPuppet](http://root-motion.com/puppetmasterdox/html/page10.html)
- **[DEV]** The author advises switching behaviours with `behaviourPuppet.SetState(BehaviourPuppet.State.Unpinned)` rather than toggling components. — [Unity forum: PuppetMaster thread, p.110](https://discussions.unity.com/t/puppetmaster-advanced-character-physics-tool-released/599766?page=110)

### Inferences
- In PuppetMaster terms the miners' rule allows only "muscles" (muscle weight with pin weight 0 and mapping weight 1 to the physics pose). Most published PuppetMaster get-ups rely on pins coming back during GetUp; that is the "hidden hand".
- `getUpOffset*` exists to stop the character sliding at the start of the rise, a sign that the target and the body disagree at hand-over, which pins then hide.

### Gaps
- The documentation does not state numerically how fast muscle weight recovers during GetUp, nor any pin-free get-up setting that RootMotion recommends.

---

## 6. Engine and studio tech: Unreal Physical Animation and Physics Control, Chaos; Ubisoft La Forge (DReCon, motion matching, For Honor); EA SEED; Embark (ARC Raiders); NVIDIA; others

### Takeaway
Engine tools offer both internal drives (local or parent-space joint motors) and **external world-space drives** that push bodies towards animation targets. Their documented get-up path is **a pose snapshot blended into a get-up montage**, i.e. animation. Ubisoft La Forge's DReCon character "moves and balances entirely using their own strength" (PD motors capped at 200 Nm), **but the authors say plainly they have no natural get-up from a fall**. Embark's ARC Raiders robots are RL policies outputting PD targets. Embark admitted to a **"magic force" on the head on another project, then penalised its use** in the reward. NVIDIA's ASE code trains get-up from random fallen states with no external forces. Tencent's Wobbledoll balances with an RL policy, **but its get-up is a kinematic animation blend**.

### Cited Findings
**Unreal Engine**
- **[DEV]** `PhysicalAnimationData.is_local_simulation`: "Whether the drive targets are in world space or local". `position_strength` and `velocity_strength` are "Only used for non-local simulation", i.e. world-space linear correction, which acts as an external force on each body. There are also `max_linear_force` and `max_angular_force`. — [Epic docs: PhysicalAnimationData](https://dev.epicgames.com/documentation/en-us/unreal-engine/python-api/class/PhysicalAnimationData?application_version=5.3)
- **[DEV]** The Physics Control plugin has two control types: world space ("each object/part is driven independently") and parent space ("Control is done in the space of the parent of each object"). `CreateControlsAndBodyModifiersFromLimbBones` builds both for every limb. — [Epic docs: EPhysicsControlType](https://dev.epicgames.com/documentation/unreal-engine/API/Plugins/PhysicsControl/EPhysicsControlType); [Epic docs: CreateControlsAndBodyModifiersFromLimbBones](https://dev.epicgames.com/documentation/unreal-engine/API/Plugins/PhysicsControl/UPhysicsControlComponent/CreateControlsAn-)
- **[DEV]** Pose Snapshot captures the ragdoll pose after physics so the character can blend from it into a "get up" montage. — [Epic docs: Animation Pose Snapshot](https://dev.epicgames.com/documentation/unreal-engine/animation-pose-snapshot-in-unreal-engine)
- **[PLAYER]** Physical animation profile parameters have only tooltip documentation, so tuning is trial and error. — [Unreal forum](https://forums.unrealengine.com/t/anybody-using-physical-animation-profile/417815)

**Ubisoft La Forge: DReCon (Bergamin, Clavet, Holden, Forbes; SIGGRAPH Asia 2019)**
- **[DEV]** The characters "use their joint actuators (similar to muscles)" to follow a motion-matching character and "move and balance entirely using their own strength". The policy learns small corrections on top of open-loop PD targets taken from the animation. — [Ubisoft La Forge: DReCon](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters)
- **[DEV, paper text]** Bullet physics; "Velocity based constraints are used to simulate PD servo motors at the joints, with motor constraint torques clamped to 200 Nm"; 60 Hz, 32 solver iterations. A fall ends the training episode (head more than 1 m from the kinematic head). Limitations: "our character can sometimes fall when control requirements are too severe, and we have not yet developed good methods of making a character stand up from a fall in a natural looking way derived from a motion capture database". They suggest feeding the simulated state into motion matching to produce a get-up sequence. Related work notes that some methods "use external forces for added stability, allowing for more stylistic control at the cost of physical realism". — [DReCon paper (PDF)](https://static-wordpress.ubisoft.com/montreal.ubisoft.com/wp-content/uploads/2019/11/13214229/DReCon.pdf)

**Ubisoft: For Honor and motion matching**
- **[DEV/PRESS]** For Honor used **motion matching**, a *kinematic* method that picks animation segments by matching pose, future trajectory and events (Simon Clavet, "Motion Matching for Realistic Animation in For Honor", nucl.ai 2016). This is not physics-based balance. — [Game Developer: inspiring animation tech talks of 2016](https://gamedeveloper.com/programming/most-inspiring-game-animation-tech-talks-of-2016); [TrueAchievements: For Honor masterclass](https://www.trueachievements.com/n24310/for-honor-masterclass-video)

**Embark Studios: ARC Raiders (GDC 2026, Martin Singh-Blom, "Learning to Move: Physics-Based Enemy Locomotion in ARC Raiders")**
- **[DEV, talk as reported in Japanese and machine-translated]** The robots (for example 12 m, six-legged) are driven by physics simulation plus deep RL. The agent observes joint angles and target direction and outputs **target angles for PD controllers**. It was trained with PPO and SAC, first in MuJoCo and Isaac Gym. Early gaits did not fall but looked "clumsy". Style came from adversarial imitation of animator-authored curves. Separate policies for "move" and "stop" are switched by a behaviour tree. **On another project the team added a non-physical "magic force" to the head to keep a robot upright, then penalised its use in the reward to keep motion natural.** One robot's gait took years to get right. — [4Gamer GDC 2026 report](https://www.4gamer.net/games/609/G060942/20260312064/); session listing: [GDC schedule](https://schedule.gdconf.com/session/learning-to-move-physics-based-enemy-locomotion-in-arc-raiders/917319) (403 on fetch)

**NVIDIA: ASE (Peng, Guo, Halper, Levine, Fidler; SIGGRAPH 2022)**
- **[DEV, released code]** `humanoid_amp_getup.py` generates fallen states by giving the root a random orientation and applying random joint actions for 150 simulation steps until the body settles. Training mixes "recovery episodes" (termination suppressed for `recoverySteps`) with episodes that start fallen (`fallInitProb`). The file calls no force or torque APIs, so the get-up must come from the policy's PD targets. — [GitHub: nv-tlabs/ASE humanoid_amp_getup.py](https://raw.githubusercontent.com/nv-tlabs/ASE/main/ase/env/tasks/humanoid_amp_getup.py); a pretrained `HumanoidASEGetupSwordShield` task appears in a third-party port. — [Rofunc docs: ASE](https://rofunc.readthedocs.io/en/stable/lfd/RofuncRL/ASE.html)
- **[DEV]** The skill embeddings were trained on "over a decade of simulated experiences" in a massively parallel GPU simulator (Isaac Gym). — [arXiv 2205.01906](https://arxiv.org/abs/2205.01906)

**Tencent LIGHTSPEED: Wobbledoll (Nan Ma, 2023)**
- **[DEV]** The system "can perform given actions while balancing" through a neural-network policy (DeepMimic-style motor controller). After a knock-down it can "perform custom motion while falling" and then "blend to kinematic get-up animation when the character settles on the ground". — [80.lv: Wobbledoll](https://80.lv/articles/wobbledoll-developing-an-advanced-full-body-ragdoll-system/)

**Naughty Dog: Uncharted 4 (GDC 2017, Michal Mach)**
- **[DEV/PRESS]** Physics simulation was layered over gameplay animation for fluidity and momentum. Early results were "pretty terrible". No get-up detail. — [GDC news](https://www.gdconf.com/news/see-naughty-dog-deconstruct-uncharted-4s-physics-animation-gdc-2017); [DualShockers](https://www.dualshockers.com/uncharted-4-naughty-dog-technology-gdc-2017/)

### Inferences
- Every engine and middleware path that ships includes a **world-space (external) drive option** and a **snapshot-to-get-up-montage** recipe. The industry's default answer to "how do they get up" is therefore "they don't; animation does".
- The only studio statement found about a non-physical upright assist being *used and then trained away* is Embark's (another project). That is direct evidence that a production team saw the "magic head force" as a crutch to remove rather than keep.
- DReCon's 200 Nm motor clamp and its admitted lack of a get-up are useful calibration: a well-resourced lab got robust physical locomotion under realistic torque limits, but not a get-up.

### Gaps
- **EA SEED:** no EA SEED physics-based character-control paper was found in this pass. The brief's "DReCon" attribution to SEED is wrong (it is Ubisoft La Forge).
- **SuperTrack (Fussell, Bergamin, Holden 2021) and Learned Motion Matching (Holden et al. 2020):** not fetched here. Learned Motion Matching is, to my knowledge, kinematic. Verify before citing.
- **ARC Raiders:** the 4Gamer report does not say whether the shipped robots right themselves through learned policies or through scripted or animated recovery. The talk recording was not available.
- **Chaos-specific ragdoll get-up documentation, NVIDIA's own game demos beyond ASE, and Star Wars Jedi: Fallen Order's physical animation talk:** not covered.
- **Skyrim (Havok Behavior):** a mod article excerpt mentions `GetUpBegin` and `GetUpStart` behaviour-graph events during ragdoll recovery, which suggests an event-driven animated get-up, but the page returned 403. — [Nexus Mods: Integrating with More Ragdoll](https://www.nexusmods.com/skyrimspecialedition/articles/12372) [SNIPPET]

---

## 7. Indie or research demos showing a fully physical get-up by learned or hand-tuned controllers (for example ML-Agents), and what they report

### Takeaway
Fully joint-actuated get-ups that hold up in testing exist **only as learned RL policies in research or research-grade code**: NVIDIA ASE, Perpetual Humanoid Control and "Learning to Get Up". They were trained over enormous simulated experience, often with curricula (a strong body first, then weaker and slower). No Unity ML-Agents get-up demo with documented results was found, only walking and standing examples. The strongest hand-tuned shipped claim remains Sumotori Dreams' "no cheating" balance (see question 4).

### Cited Findings
- **[DEV]** "Learning to Get Up" (Tao, Wilson, Gou, van de Panne; SIGGRAPH 2022): RL without motion capture, in three stages. A *strong* character model first discovers strategies; the policy is then adapted to progressively *weaker* characters; finally slower motions are produced. The policies show diverse strategies, often sharing a final standing strategy, and can be paused at many points. The method is also tested with a cast on a leg and an arm. The abstract opens: "Getting up from an arbitrary fallen state is a basic human skill." — [arXiv 2205.00307](https://arxiv.org/abs/2205.00307)
- **[DEV]** Perpetual Humanoid Control (Luo, Cao, Winkler, Kitani, Xu; ICCV 2023): the controller "learns to naturally recover from fail-state" "without using any external stabilizing forces". — [arXiv 2305.06456](https://arxiv.org/abs/2305.06456)
- **[DEV]** ASE get-up training uses random fallen states and recovery episodes, with no external forces (see question 6). — [GitHub: ASE humanoid_amp_getup.py](https://raw.githubusercontent.com/nv-tlabs/ASE/main/ase/env/tasks/humanoid_amp_getup.py)
- **[DEV]** DReCon (shipped-game-budget RL tracking) explicitly lacks a natural get-up. — [DReCon PDF](https://static-wordpress.ubisoft.com/montreal.ubisoft.com/wp-content/uploads/2019/11/13214229/DReCon.pdf)
- **[PLAYER/hobbyist]** Unity ML-Agents ships Walker and Crawler ragdoll environments. A hobbyist's wiki logs training a ragdoll to stay upright (16 limbs observed, feet-contact reward, penalties for other contacts) but not to get up. — [GitHub wiki: ngmeijer Advanced-Tools](https://github.com/ngmeijer/Advanced-Tools/wiki/22%E2%80%9004%E2%80%902024)
- **[DEV]** Embark's ARC Raiders: an RL physics pipeline in a shipped game, with a "magic head force" used and then penalised on another project (see question 6). — [4Gamer](https://www.4gamer.net/games/609/G060942/20260312064/)

### Inferences
- The "strong → weak → slow" curriculum in Learning to Get Up maps directly onto the miners' situation (realistic strength caps): find a get-up *trajectory family* with relaxed caps, then tighten towards the real ones, then slow it down for naturalness. It is a principled alternative to hidden help that leaves the final controller external-force-free.
- ASE's way of making fallen states (random orientation + random joint actions + settle) is a cheap recipe for building a test set of realistic starting poses to measure get-up success, which the project's searched-pose approach currently lacks as a benchmark.

### Gaps
- No public Unity ML-Agents project with documented results for a *get-up* (not walking) was found. YouTube "AI learns to stand up" style videos were not reviewed.
- Success rates and torque limits for ASE and PHC get-ups were not extracted here; the PDFs were too large to fetch. These may be covered in the sibling notes `learned_and_tracking.md`.

---

## 8. Common compromises (hidden upright torque on hips or torso, pin forces, blending) and how players perceive them

### Takeaway
Three compromises dominate:
1. **Animation for the rise**: a snapshot or ragdoll pose blended into prone or supine get-up clips (Overgrowth, PuppetMaster, Unreal, Wobbledoll, Euphoria titles by inference, Skyrim by excerpt).
2. **External forces towards an animated target**: PuppetMaster pins, Unreal world-space drives.
3. **Hidden upright forces on head, torso or feet**: TABS explicitly; Gang Beasts and Human: Fall Flat suspected; Embark's "magic head force" on another project.

Studios that keep bodies more honestly physical (Exanima, Half Sword, Sumotori Dreams, Toribash, QWOP) accept clumsy, tripping, comedic motion, and developers and players say so. **No shipped game was found whose developer confirms a human getting up from the ground by joint torques alone, automatically and reliably.**

### Cited Findings
- **[DEV]** Pins are "simple AddForce" towards animated targets. — [PuppetMaster docs](http://root-motion.com/puppetmasterdox/html/page5.html)
- **[DEV]** Unreal's non-local physical animation corrects "linear position error", i.e. a world-space force. — [Epic docs](https://dev.epicgames.com/documentation/en-us/unreal-engine/python-api/class/PhysicalAnimationData?application_version=5.3)
- **[PRESS/DEV]** TABS applies upward force to the torso and head by head height, through an animation curve, and the balance script adds force to the legs and feet. — [Game Developer](https://gamedeveloper.com/design/how-landfall-games-finds-the-fun-in-physics-engines)
- **[DEV]** Embark: a "magic force" on the head, penalised in the reward to keep motion natural. — [4Gamer](https://www.4gamer.net/games/609/G060942/20260312064/)
- **[DEV]** Research using "external forces for added stability" gains stylistic control "at the cost of physical realism". — [DReCon PDF](https://static-wordpress.ubisoft.com/montreal.ubisoft.com/wp-content/uploads/2019/11/13214229/DReCon.pdf)
- **[DEV]** Overgrowth: velocity threshold + timer → keyframed `r_standfromfront` / `r_standfromback2`. — [aschar.as](https://raw.githubusercontent.com/WolfireGames/overgrowth/main/Data/Scripts/aschar.as)
- **[DEV]** Wobbledoll: an RL balance policy, but "blend to kinematic get-up animation" once settled. — [80.lv](https://80.lv/articles/wobbledoll-developing-an-advanced-full-body-ragdoll-system/)
- **[PLAYER/other developers]** Common indie advice: drive walk cycles by "jolting" feet with impulses, use a "custom gravity script" to move the centre of mass, or make "string puppet" characters (a physical torso and hands and feet, with visual-only IK limbs). — [Game Developer: 8 tips for animating active ragdolls (2021)](https://www.gamedeveloper.com/design/8-tips-for-animating-active-ragdolls). Another developer says "self preservation, up righting, and spatial awareness is missing" from their active ragdolls and is faked by tweaking blended-animation probabilities. — [Unity forum: How we made our active ragdolls somewhat convincing](https://discussions.unity.com/t/wip-how-we-made-our-active-ragdolls-somewhat-convincing/848337)
- **Perception: [DEV]** Exanima: "It will realistically always feel tricky and look a bit silly for a new player". — [Bare Mettle forums](https://www.baremettle.com/forums/index.php?threads/exanima-0-8.12202/)
- **Perception: [PLAYER]** Exanima players complain of excessive tripping. — [Steam](https://steamcommunity.com/app/362490/discussions/0/3196989600451168411/?ctp=4). Half Sword players report trouble standing up after an update. — [Steam](https://steamcommunity.com/app/2397300/discussions/0/3949154954318457031)
- **Perception: [PRESS]** TABS units flail "like drunk toddlers". — [bit-tech](https://bit-tech.net/reviews/totally-accurate-battle-simulator-review/1). Sumotori Dreams' "attempts to stand up and bow" are played for laughs. — [Wikipedia](https://en.wikipedia.org/wiki/Sumotori_Dreams). Gang Beasts is "a tool for daft, physics-led pratfalls". — [PC Gamer](https://pcgamer.com/gang-beasts-review)

### Summary table (verdict per title or tool)

| Title / tool | Get-up driven by joint torques only? | Help from outside the body | Naturalness | Robustness | Evidence |
|---|---|---|---|---|---|
| GTA IV/V, RDR1/2, Max Payne 3 (Euphoria) | No (inferred) | Get-up animation after an NM reaction; blend-from-NM task | Reactions acclaimed; get-up is canned | Always succeeds (animation) | Rockstar: Euphoria = "physics-based reactions"; mods; FiveM |
| Backbreaker, Force Unleashed | Unknown (likely no) | — | — | — | No source |
| Exanima | Unknown | None stated; muscle-tension "AI" over authored motions | "a bit silly" (developer) | Frequent tripping (players) | Developer posts; changelog |
| Overgrowth | **No** | Keyframed `r_standfromfront/back2` blended from ragdoll pose | Fluid | Always succeeds | Released source |
| TABS (Landfall) | **No** | Upward force on torso and head by head height; leg and foot forces | Deliberately wobbly | High | Game Developer + CEO quotes |
| Gang Beasts | Probably not | Suspected head-holding force | Comedic | — | Community only |
| Human: Fall Flat | Unknown | Unknown | Comedic sway | — | No developer statement found |
| Toribash | Yes (player-controlled joint modes) | None documented | Player skill | Low (turn-based, manual) | Wiki, press |
| QWOP | Yes (player drives thighs and calves) | None | — | Players mostly cannot recover | Wikipedia |
| Sumotori Dreams | Claimed yes ("no cheating", follow-up project) | None claimed | Comedic dummies | Self-rights automatically | Press; developer excerpt |
| Half Sword | Unknown | Unknown | — | Players report trouble standing | Players |
| PuppetMaster | **No** (by default) | Pins (AddForce) regained during GetUp; animation target; optional mapping blend | Good | High | Official docs |
| Unreal Physical Animation / Physics Control | **No** (get-up recipe) | World-space drives; pose snapshot → get-up montage | Good | High | Epic docs |
| Ubisoft DReCon | n/a: no get-up | None (200 Nm PD) | — | Falls end episodes | Paper |
| Embark ARC Raiders | Unknown for get-up; locomotion is RL PD | "Magic head force" used then penalised on another project | Years of style tuning | Shipped | GDC 2026 report |
| Tencent Wobbledoll | **No** | Kinematic get-up blend | — | — | 80.lv interview |
| NVIDIA ASE / PHC / Learning to Get Up (research) | **Yes** | None (stated or visible in code) | Varied; slowed for naturalness | Learned over very large simulated experience | Code, arXiv |

### Inferences
- Shipped games have converged on "physics for falling, animation (or pins) for rising" because a failed get-up is a gameplay bug, whereas a canned get-up is only a small aesthetic cost players rarely notice. When the body is kept physical, studios either lean into comedy (TABS, Gang Beasts, Sumotori) or accept a learning curve and clumsiness (Exanima, Half Sword).
- For the project's rule, the realistic precedents are research RL get-ups (no external forces, curricula over strength and speed) and Embark's practice of *training away* an assist. Nothing in shipped games offers a proven hand-tuned recipe for a torque-only human get-up.

### Gaps
- No player-perception study was found that compares canned and physical get-ups directly. The perception evidence is anecdotal (reviews, Steam threads, developer remarks).
- Whether any shipped Euphoria or Havok title used hidden root or hip balance forces during *balancing* (not get-up) remains undocumented.
