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
| [NextMilestonePlan.md](NextMilestonePlan.md) | The current stage's plan |
| [Validation.md](Validation.md) | Evidence for every milestone: tests, builds, captures, failures and fixes |

## Folders

| Folder | Contents |
|---|---|
| [ArtDirection/](ArtDirection/) | **The art direction.** [VisualSoul.md](ArtDirection/VisualSoul.md) (Luis's handoff, and the direction now) and its approved reference images. [TheEssence.md](ArtDirection/TheEssence.md) (the study of what lies under the soul, what Luis's favourite frame teaches, and a proposed language). [StyleStudies.md](ArtDirection/StyleStudies.md) (the earlier Blender studies, not chosen) |
| [Correspondence/](Correspondence/) | Luis's messages, verbatim and dated, with how each was recorded |
| [Design/](Design/) | Design notes that feed GAME_VISION: the early design brief and the one-worker showcase vision |
| [Plans/](Plans/) | Completed or superseded milestone plans, archived when replaced |
| [Playtests/](Playtests/) | One guide per playable milestone: how to run it, what to judge, and its evidence and limits |
| [Technical/](Technical/) | Architecture and history for engineers and agents ([UnityProjectContext.md](Technical/UnityProjectContext.md)) and the equipment/mining contracts |
| [Images/](Images/) | Screenshots and renders, in one folder per milestone, named like its playtest guide |

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
