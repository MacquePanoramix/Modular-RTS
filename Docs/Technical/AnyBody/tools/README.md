# Tools for running and reading the work (kept in the repository so any session can resume)

| Tool | What it does |
|---|---|
| `unity_tests.sh <name> [filter] [extra args]` | Runs this worktree's PlayMode tests in batch mode; results and log in `TestRuns/` (ignored by git); prints each test's result and every failure's message |
| `testresults.py <results.xml>` | Prints a results file: totals, each test, each failure's message |
| `shoves.sh <name> [extra args]` | Runs only the shove test that is not yet met (`ShovedAgainAndAgainNothingOfItFlies`) and prints each course's line. With `-ownShoveTrace 1` it also writes `TestRuns/<name>.trace.txt`: the 16 steps before a part flies, one a line (each leg part's speeds and what its joint gave; the boots; and what the whole body holds against what its joints were counted to give, in joules). `-ownOnly Small` runs one miner; `-ownStepSet Name=value,...` sets the keeper's static settings for the run |
| `../sheets/atlas_sheets.py <capture folder> [--blind]` | Makes the stance sheets from a capture's tiles and `figures.csv` |

## The runs used most

```
# the whole suite (about 25 minutes; nothing heavy beside it)
bash Docs/Technical/AnyBody/tools/unity_tests.sh suite all

# one class, or several (';' between), with the tests' own arguments after
bash Docs/Technical/AnyBody/tools/unity_tests.sh facts "WonderGather.Tests.ArticulationFactsTests"
bash Docs/Technical/AnyBody/tools/unity_tests.sh own "WonderGather.Tests.OwnBodyTests" -ownAnatomical 1
bash Docs/Technical/AnyBody/tools/unity_tests.sh old_rule "WonderGather.Tests.PhysicalBodyTests" -strengthRule need

# the stance sheets: the fit's solids (any bodies, "made" = the ten made bodies), and the drawn miners
bash Docs/Technical/AnyBody/tools/unity_tests.sh atlas "WonderGather.Tests.StanceAtlasCapture" -atlasOut D:/Dev/WG/Captures/Atlas/solids -atlasBodies "Small,Long,Round,made" -atlasLooks relaxed -atlasLineup 1
bash Docs/Technical/AnyBody/tools/unity_tests.sh drawn "WonderGather.Tests.StanceAtlasDrawnCapture" -atlasOut D:/Dev/WG/Captures/Atlas/drawn
python -I Docs/Technical/AnyBody/sheets/atlas_sheets.py D:/Dev/WG/Captures/Atlas/drawn

# the solver's console checks (no Unity): from Docs/Technical/AnyBody/port/tests/Body.Tests and .../Reserve.Tests
dotnet build -c Release -m:1 && dotnet run -c Release --no-build -- all          # Body: 21541 checks, 0 failed
dotnet run -c Release --no-build -- relax --arms                                # the free limbs' rest on 450 fits
```

The golden data the console checks read (`port/reference/*.json`) is not in git: `python export_body.py`,
`export_reserve.py`, `export_lp_random.py` in `port/reference` make it again (Python 3.12, numpy, scipy).

## Rules that have cost a run before

- Nothing heavy beside the whole suite; no code or test file edited while an editor run is going.
- A patch script with any backslash goes through a file written by the editor tool, never a shell heredoc (the shell
  halves doubled backslashes).
- Luis's own uncommitted files in the main project are never committed, reset or discarded
  (`Assets/_WonderGather/Scenes/TheGroup.unity`, `ProjectSettings/PackageManagerSettings.asset`,
  `ProjectSettings/URPProjectSettings.asset`, `Assets/_Recovery*`); in this worktree `ProjectSettings/EditorBuildSettings.asset`
  and `ProjectSettings.asset` stay out of commits. Stage by path, never `git add -A`.
- A result is said only after its run was read. Runs repeat a handful of courses: count courses, not runs.
