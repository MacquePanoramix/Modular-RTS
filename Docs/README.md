# Wonder Gather documentation

Start with [CURRENT_STATE.md](CURRENT_STATE.md). It says what is happening now
and routes to everything else.

## Core (top level)

| Document | Purpose |
|---|---|
| [CURRENT_STATE.md](CURRENT_STATE.md) | What is being built now, the latest direction, and the next action |
| [GAME_VISION.md](GAME_VISION.md) | The design source of truth (Locked, Direction, Possible and Open), with the decision log |
| [DESIGN_RATIONALE.md](DESIGN_RATIONALE.md) | The few "whys" that must not drift |
| [PROJECT_CULTURE.md](PROJECT_CULTURE.md) | Roles and how work is routed |
| [ShowcaseRoadmap.md](ShowcaseRoadmap.md) | The Worker Showcase stages (S0–S5) and their open decisions |
| [NextMilestonePlan.md](NextMilestonePlan.md) | The current stage's plan (S3: weight and strength at the rock) |
| [Validation.md](Validation.md) | Evidence for every milestone: tests, builds, captures, failures and fixes |

## Folders

| Folder | Contents |
|---|---|
| [ArtDirection/](ArtDirection/) | **The art direction.** [VisualSoul.md](ArtDirection/VisualSoul.md) (Luis's handoff, and the direction now) and its approved reference images. [TheEssence.md](ArtDirection/TheEssence.md) (the study of what lies under the soul, what Luis's favourite frame teaches, and a proposed language). [StyleStudies.md](ArtDirection/StyleStudies.md) (the earlier Blender studies, not chosen). [TheMiners.md](ArtDirection/TheMiners.md) (the worker's redesign: Small, Long and Round, built from modules, S1d). [CharacterPractices.md](ArtDirection/CharacterPractices.md) (how beings are built: practices, natural walks). [ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md) (how every model is checked and improved: the standard, seven passes, the capture matrix, the misses ledger). [MotionJudging.md](ArtDirection/MotionJudging.md) (how every movement is checked: the numbers, judges who did not make it, then Luis). [WorkerConcepts.md](ArtDirection/WorkerConcepts.md) (their first concepts, superseded) |
| [Correspondence/](Correspondence/) | Luis's messages, verbatim and dated, with how each was recorded |
| [Design/](Design/) | Design notes that feed GAME_VISION: the early design brief, the one-worker showcase vision, and [ThePhysicalBody.md](Design/ThePhysicalBody.md) (how a body is very stable and still able to fall; strength and stamina; real objects; every action physical; the interaction click) |
| [Plans/](Plans/) | Completed or superseded milestone plans, archived when replaced |
| [Playtests/](Playtests/) | One guide per playable milestone: how to run it, what to judge, and its evidence and limits. The latest: [MinersPlaytest.md](Playtests/MinersPlaytest.md) (choose a miner, walk the meadow) |
| [Research/](Research/) | What was looked up outside the project, with its sources. [What real bodies do, and how to judge ours](Research/2026-10-08_RealBodiesAndHowToJudgeThem.md) (for the playtest round of October 8) |
| [Reviews/](Reviews/) | Reviews of the whole project against Luis's direction, made when Luis asks; and rounds of fixes kept track of note by note. The latest: [2026-10-08_ThePlaytestRound.md](Reviews/2026-10-08_ThePlaytestRound.md) (Luis's ten notes on S3, and what was found and done for each). Before it: [2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md) (what Luis has asked for and where each thing stands; how a pickaxe is really swung; how a body can answer to real forces; a proposal) |
| [Technical/](Technical/) | Architecture and history for engineers and agents ([UnityProjectContext.md](Technical/UnityProjectContext.md)) and the equipment/mining contracts |
| [Images/](Images/) | Screenshots and renders, in one folder per milestone, named like its playtest guide |

## Where each kind of record goes

When something happens, it is written down in these places:

| What happened | Where it is recorded |
|---|---|
| **Luis asks whether the project is on course** | A review in `Reviews/<date>_<Topic>.md`, with a proposal; the plan it leads to goes in NextMilestonePlan |
| **Luis sends a message** | Verbatim in `Correspondence/<date>_<TOPIC>.md`, with how it was recorded. A summary goes in CURRENT_STATE ("Latest Game Director direction"), and a row per decision in GAME_VISION's decision log |
| **A look or a character changes** | Its art direction page (for example [TheMiners.md](ArtDirection/TheMiners.md)), with before-and-after images in `Images/<Milestone>/` |
| **Something becomes playable** | Its playtest guide (how to run it, what to judge, its limits) |
| **Code or assets change** | [UnityProjectContext.md](Technical/UnityProjectContext.md): how it works and why |
| **Tests, builds, benchmarks or captures run** | [Validation.md](Validation.md): results, what failed and how it was fixed, and what was not tested |
| **A practice is learned** | The practices page for its area (for example [CharacterPractices.md](ArtDirection/CharacterPractices.md)) |
| **Every step** | Committed and pushed to GitHub with a message saying what changed and why |

## Conventions

- **Dates and verbatim records.**
  - Every decision has a date and a status (Locked, Direction, Possible,
    Open or Implemented).
  - Luis's words are kept verbatim in Correspondence.
- **Milestone deliverables.** Each one has:
  - a plan (NextMilestonePlan, archived to Plans/ when superseded);
  - a playtest guide in Playtests/;
  - evidence in Validation.md;
  - images in Images/<Milestone>/.
- **Source art.** It lives outside Unity in `Art/` at the repository root
  (for example `Art/Blender/OrdinaryPlace/house.py`). Every model is
  reproducible from its script.
- **Branches.** Work happens on a branch and is merged into `main` per stage,
  after Luis's playtest. Each merge commit says which stages it contains.
