# House.fbx and Nature.fbx exported again without the profile path (November 17, 2026)

Status: **stopped after step 3's node-tree comparison**: the house's curtain mesh differs in the order of its faces (see "Stopped here"). Nothing committed.
The profile folder is shown as `<profile>`, the name as `<name>`.

## Same blobs on both branches

`git rev-parse <branch>:Assets/_WonderGather/Art/OrdinaryPlace/<file>`:

| File | claude/worker-showcase (main project) | claude/wip-round (D:/Dev/WG) |
|---|---|---|
| House.fbx | 3097f66fcd8d5d9a3c2dbe47e40f734038d2f233 | 3097f66fcd8d5d9a3c2dbe47e40f734038d2f233 |
| Nature.fbx | d70f18f5280da707deb641fc2998a7cec2f90592 | d70f18f5280da707deb641fc2998a7cec2f90592 |

Same blobs. Old copies: `D:/Dev/fbx_reexport/old/` (sha1 House fa7bfd96..., Nature 5b29e868...).

## 1. Where the old files hold the path

Scanner: `D:/Dev/fbx_reexport/scripts/fbxtree.py scan` (a small binary FBX reader; it builds the private forms at run time from USERPROFILE and prints them masked).

Both files are binary FBX 7400. The path is in every texture's three path fields, always the same form:
`<profile>\AppData\Local\Temp\claude\D--Programs-Unity-Projects-Wonder-Gather-The-Wanderer\<session-id>\scratchpad\paint5`
(back slashes, as written; a folder, not a file: the painted images were generated in Blender and never given a file path, so the exporter wrote the working folder).

| File | Node and property | Count |
|---|---|---|
| House.fbx | `Objects/Texture/FileName` | 10 |
| House.fbx | `Objects/Video/Filename` | 10 |
| House.fbx | `Objects/Video/Properties70/P "Path"` | 10 |
| Nature.fbx | the same three | 4 each (12) |

Raw byte counts of `<profile>` in the files: House 30, Nature 12 (UTF-8, back slashes); no other form (forward slashes, JSON-escaped, upper or lower case, UTF-16) and none inside the compressed arrays.
`RelativeFilename` is `.` in every Video node.
Other absolute paths: only Blender's placeholder `/foobar.fbx` in the header (`SceneInfo` `DocumentUrl`, `SrcDocumentUrl`, `Original|FileName`). Nothing else.

## Blender versions

- Written by: `Creator "Blender (stable FBX IO) - 4.4.0 - 5.12.7"`, header time stamp 2026-10-02 09:39:10.
- This machine: `D:/Programs/Blender/blender.exe` = Blender 4.4.0 (build 2025-03-18); also `D:/Programs/Blender 4.5.2`. The export again uses 4.4.0.

## Which script version made the committed files

- Both FBX last changed in `c330ec4` (2026-10-02 10:24).
- `house.py` and `nature.py` last changed in `d0a2637` (2026-10-01 22:32); unchanged since, so the c330ec4 and HEAD versions are the same.
- `painting.py` changed in `c330ec4` and again in `2bd6613` (2026-10-02 23:44, after the FBX). Its changes only alter the painted pixels; the images are not embedded in the FBX. The export again uses all three scripts as at `c330ec4` (`D:/Dev/fbx_reexport/src_c330ec4/`).
- The committed files were made with `--paint` (Validation.md, S1c second pass): without it the FBX has no Texture or Video nodes.

## 2. Exported again

Run (rung: run, read to its end; both `exit 0`, logs `D:/Dev/fbx_reexport/house_export.log`, `nature_export.log`):
working folder `D:/Dev/fbx_reexport/new`, TEMP and TMP set to `D:/Dev/fbx_reexport/tmp`,
`blender.exe -b --factory-startup --python src_c330ec4/house.py -- --paint . --fbx House.fbx`, the same for `nature.py`.
The working folder and the paint folder are the FBX's own folder, as in the old files (their `RelativeFilename` is `.`).

New files: `D:/Dev/fbx_reexport/new/House.fbx` (759 804 bytes), `Nature.fbx` (1 556 188 bytes).
Scan of the new files: **0 hits** of the profile path or the name, in every form (as is, forward slashes, JSON-escaped, upper case, lower case, UTF-8 and UTF-16, raw bytes and inside every decompressed array).
Absolute paths left: `D:\Dev\fbx_reexport\new` in the same three fields (House 10 x 3, Nature 4 x 3), and the header's placeholder `/foobar.fbx`. (Other raw regex matches are random bytes inside compressed arrays.)
**What replaced the path:** `D:\Dev\fbx_reexport\new`.

## 3. Comparison in Blender: node trees

Tool: `fbxtree.py tree` (every node, every property; arrays shown as length and SHA-1 of their decompressed bytes), then `normtree.py`, which replaces each object's FBX UID by its name, since Blender draws new UIDs at every export.

Can it fail? The FBX writer in `fbxtree.py` rewrites the old House byte for byte (`roundtrip`: identical by `cmp`). A known-bad copy, `bad/House_vertex.fbx`, with one vertex of the first Geometry moved by 0.1 (3.28093 -> 3.38093), shows in the tree diff:

```
265c265
<     Vertices d[](n=432,sha1=4eea847e32c4ca8c)
---
>     Vertices d[](n=432,sha1=3635319f1c46eb3e)
```

Diff old against new, UIDs named, the three path fields left out (they differ as described above):

- **Nature.fbx:** only the header's `CreationTimeStamp` (2026-10-02 09:39:40 against 2026-11-17 14:11:14). Nothing else.
- **House.fbx:** the time stamp, and **one mesh, `House_Curtain`**:

```
<     PolygonVertexIndex i[](n=1136,sha1=f971e4b381b3febf)
<     Edges i[](n=568,sha1=1f1efaf60252399f)
>     PolygonVertexIndex i[](n=1136,sha1=205be5057669c2f5)
>     Edges i[](n=568,sha1=b76a9fa92da07844)
<       UVIndex i[](n=1136,sha1=ead9e4c4db06f8f0)
>       UVIndex i[](n=1136,sha1=5af5f4a615217d49)
```

What the curtain difference is (`curtain.py`, decoding both meshes): Vertices, Normals, NormalsIndex, UV and the material layer are identical. The 284 polygons are the same polygons, with the same start corner and the same UV at each corner, but **44 of them stand at another place in the polygon list**; so the edge list and the UV index list follow. The shape and its texturing are the same; the order of its faces is not. In Unity this changes the order of the curtain's triangles in its index buffer, not the vertices.

Not yet known: why. Likely a step of `house.py` that builds the curtain in an order that is not fixed from run to run; not looked into.

## Stopped here

The task's rule: if the models differ in anything but the header and the paths, stop, commit nothing, and report. The curtain's face order is such a difference, so the work stopped after the node-tree comparison. The 5-hour usage window also stood at 79 % (the stop mark is 80 %).

- Nothing was copied into `D:/Dev/WG`; its two FBX files are the old ones. Nothing committed, nothing pushed. `D:/Dev/fbx_reexport/final` not made.
- The Unity dump test was written but never compiled or run; it was moved out of the worktree to `D:/Dev/fbx_reexport/scripts/FbxReexportCheck.cs` (no `.meta` was ever made), so `D:/Dev/WG` holds none of this work.
- The Blender import dump (`scripts/blender_dump.py`, with `scripts/run_dumps.sh` for old, new and the known-bad copy) was written and not run.

## What was not tested

- The Blender import dump (objects, transforms, mesh hashes, materials, images) was not run, on any file.
- Nothing in Unity: no import dump, no local-file-ID comparison, no reference scan of the scenes and prefabs, no `.meta` diff, no still. The project has a still tool (`CaptureTools.Render`, used by `DuskZoomCapture`); it was not used.
- Whether the curtain order changes from one export to the next (a second export of the house without `--paint` would show it in seconds), and whether an export can be made with the old order.

## For the Game Director's decision

Options, none chosen: (a) accept the curtain's new face order as harmless and go on with the Unity comparison, where its triangle order is the only expected change; (b) find the cause in `house.py` and export until the order matches; (c) leave the files as they are. The new files wait in `D:/Dev/fbx_reexport/new`; the old copies in `D:/Dev/fbx_reexport/old`.

## Patched instead (November 17, evening)

Status: **steps 1 to 4 done and read to their end; every check passed; reviewed by a sceptic (`review_fbx_patch.md`: no finding of substance, one miscount, mended); committed on both branches (the end of this section).**
The profile folder is shown as `<profile>`, the name as `<name>`.

### What was changed, and how

The master's call: leave the re-export aside and rewrite only the path fields of the old files. `D:/Dev/fbx_reexport/scripts/fbxtree.py` got a `patch <in> <out>` command (the same node reader and writer as `roundtrip`). In every `Objects/Texture/FileName`, every `Objects/Video/Filename` and every `Objects/Video/Properties70/P "Path"` the string value is set to `.` (the value of their `RelativeFilename`); no other node is touched. House: 10 + 10 + 10 = 30 fields; Nature: 4 + 4 + 4 = 12. Output: `D:/Dev/fbx_reexport/patched/House.fbx` (759 132 bytes) and `Nature.fbx` (1 555 916 bytes).
Starting point: `git hash-object` of `old/House.fbx` = 3097f66f..., `old/Nature.fbx` = d70f18f5... (the committed blobs; same as the first attempt's table).
One thing besides the strings had to follow from the shorter file: the footer's alignment padding (Blender pads the file to a multiple of 16 before the footer's version word). The patch command recomputes it by that rule, after checking that the rule reproduces the old files' own padding; the footer's 140 trailing bytes (version, zeros, magic) and the first 27 header bytes are byte-identical to the old files'. All node offsets and lengths are recomputed by the writer.

### Checks (script `scripts/check_patch.sh`, output `D:/Dev/fbx_reexport/checks_step1.txt`; rung: run, read to its end)

- Roundtrip of each old file: `cmp` identical for House and Nature ("House roundtrip identical", "Nature roundtrip identical"). Roundtrip of each patched file: identical too.
- Scan of the patched files (every form the first scan used: as is, forward slashes, JSON-escaped, upper and lower case, the name alone, UTF-8 and UTF-16, raw bytes and inside every decompressed array): `private-path hits total: 0` for both. The old files, scanned by the same command: 60 (House) and 24 (Nature) (30 + 30 and 12 + 12: the folder and the name). The only absolute paths left in any string property (listed from the tree dump, `S'/...'`, drive letters, UNC): `/foobar.fbx` in `DocumentUrl`, `SrcDocumentUrl` and `Original|FileName`, in both files. (The scan's own "abs path (raw)" lines for the patched files are random bytes inside compressed arrays, as in the first attempt.)
- Tree diff old against patched (`fbxtree.py tree`, `diff`): House **30** changed lines, Nature **12**; "old lines that are not a path field: 0"; the new lines are `FileName S'.'` x10, `Filename S'.'` x10, `P S'Path' ... S'.'` x10 (House), 4, 4, 4 (Nature). Header time stamp lines in the diff: 0 (`CreationTimeStamp` is the same). UIDs are unchanged, so no renaming was needed.
- Known-bad cases, each shown to be caught:
  - `bad/House_oneleft.fbx`: the same patch with one `Video/Filename` (the 18th field) left as it was (`patch ... 17`; 29 fields patched). The scan finds it: `private-path hits total: 2`, `path at /Objects/Video/Filename`. Its tree diff against the patched file is that one line (`Filename S'.'` against the path).
  - `bad/House_vertex.fbx` (one vertex of the first Geometry moved by 0.1), tree diff against old: `Vertices d[](n=432,sha1=4eea847e32c4ca8c)` against `sha1=3635319f1c46eb3e`, shown.
- Lazy way to pass the scan: patch fewer fields or none and scan only one form. Guarded against by: the one-left copy is caught, the old files give 60 and 24 in the same run, and the 30 and 12 counts are required by the tree diff.

### Blender import dump (Blender 4.4.0, `-b --factory-startup`, `scripts/blender_dump.py`, `run_dumps.sh`, outputs `D:/Dev/fbx_reexport/dumps/`; all five runs exit 0, `DUMP_DONE` in each log)

The dump holds objects (type, parent, transform, data, material slots), per mesh counts and hashes of positions, faces, face materials, loop normals, UVs and colours, materials and their images.
- old against patched, House: the only differing lines are the 10 `material ... images=` lines and the one `image` line (11 per side; mended by the master from "11" after the sceptic's count), whose file path is the folder the image was looked for in (`D:/Dev/fbx_reexport/old` against `.../patched`: the path field `.` resolves next to the FBX file). Nature: the same, 5 lines. No mesh, object or transform line differs.
- known-bad (vertex moved): `mesh Fence positions=71313f4836fd4a66` against `a6a2a3c73c10f49f` and `loop_normals` differ: the dump catches it.

### Unity comparison (D:/Dev/WG, branch claude/wip-round, Unity 6000.6.0f1)

Tool: `Assets/_WonderGather/Tests/PlayMode/FbxReexportCheck.cs` (an Explicit PlayMode test class with two tests, `Dump` and `Still`; the project's test script runs only PlayMode, and the dump reads the AssetDatabase under `#if UNITY_EDITOR`; it holds no private path). Copy in `D:/Dev/fbx_reexport/scripts/`. Runs by `scripts/run_unity.sh <name>` (the project's `unity_tests.sh`, LIMIT 1800 s; each run ended by itself with `total 2 passed 2`, its log and result read; the result XML's `asserts` count was not used).
`Dump`, for House.fbx and Nature.fbx: every sub-asset's type, name and local file ID (`TryGetGUIDAndLocalFileIdentifier`: 130 assets); for each mesh (22) the vertex count, submesh count, index counts, bounds, and hashes of vertices, normals, UVs and **indices**; the materials' names and textures (the import makes no Material sub-assets: the renderers' material names are in the hierarchy lines); the hierarchy with local transforms; and which local IDs the project's scenes, prefabs, assets, materials and controllers reference, with the file's GUID (39 references, `missing=0`, every one found among the import's IDs).
`Still`: the scene TheOrdinaryPlace, three views (day, dusk, wide), 960 x 540, `CaptureTools.Render`, as `DuskZoomCapture` does; the project's own default (not `-ownAsPlayed 1`: the picture is of the models, the miner stands by the body as it was).

The runs, in order (rung for each: run on the rig in the editor's batch mode, read to its end):

| Run | Files in the worktree | Result |
|---|---|---|
| `fbx_old` | old | Dump **failed**: 1 of 39 references "missing" (`House.fbx` id 100100000 from `TheOrdinaryPlace.unity`). 100100000 is the model-prefab id references have always used; the dump had not counted it. Fixed in the tool (counted by name, comment in the code): a failure of the check itself, found by running it on the old files. |
| `fbx_old2` | old | Dump passed, 39 references, 0 missing. Its stills differed from the first run's in about a third of their pixels (186 032 of 518 400): the wind and the clock move the grass. The tool now sets the wind strength to zero for the still (read back from the running scene: `windStrength=0`). Old's and old2's stills are not used. |
| `fbx_old3`, `fbx_old4` | old (sha1 fa7bfd96, 5b29e868 read back before the run; the log shows both files imported) | passed |
| `fbx_p1`, `fbx_p2`, `fbx_p3` | patched (sha1 bba34e2e, 17142468), copied over the old ones with `cp`; `.meta` untouched; the log shows both files imported | passed |
| `fbx_bad1` | old House replaced by the fence-vertex copy (0.1) | the run passed; its dump differs, see below |
| `fbx_bad2` | old House replaced by `bad/House_roofvertex.fbx`: one vertex of House_Roof moved 0.5 | the run passed; its dump and stills differ, see below |

After `fbx_p3` the patched files stay in the worktree (sha1 read back: bba34e2e, 17142468).

- **Dump, old (old2, old3, old4) against patched (p1, p2, p3): identical.** `diff` of the pairs prints nothing (206 lines each, 39 reference lines each, so a diff of nothing is not an empty file against an empty file). Local file IDs, mesh counts, bounds, vertex, normal, UV and index hashes (the curtain's included: `House_Curtain ... indexHash=5b508991188401a5` in both), hierarchy and transforms, the references and their GUIDs are the same.
- **`git diff` of the two `.meta` files: empty** (0 lines), read after the last run. No `.meta` was touched by me and none changed; the FBX import settings were not touched.
- **The dump can fail**: it failed on the old files in its first run (above); `fbx_bad1` (fence vertex moved 0.1) differs from patched in exactly one value, `Fence ... posHash=8f190c750ec1a665` against `e9e0adff4d794c5e` (index, normal and UV hashes and bounds are the same, as they should be); `fbx_bad2` differs in the `House_Roof` line. Lazy way to meet the dump's bar: print few fields. Guarded against by hashing each of vertices, normals, UVs and indices, and by the known-bad copies.
- **Stills.** The still tool exists and was used. Two runs of the same files are not pixel-identical even with the wind off: the chimney's smoke and the walking miner move by the clock (about 150 to 17 000 differing pixels of 518 400, by view). So the stills were compared with a noise mask: the 8 x 8 pixel cells that hold a differing pixel in any of three same-file pairs (p1/p2, p1/p3, old3/old4), widened by 3 cells (`scripts/mask_compare.py`, output `unity/mask_compare.txt`).
  - old (old3, old4) against patched (p1, p2, p3), 6 pairs per view, 3 views: **0 differing pixels outside the mask** in all 18 comparisons. Everything differing lies in the smoke and the miner. (A first comparison of old3 with p1, with a mask made from p1 and p2 alone, had found one pixel outside it, in the grass at x 732, y 323, one channel by 1 of 255: p1, p2 and the fence copy had 128 there and old3 129; with old4 and p3 in the mask source it lies in the noise.)
  - Known-bad: the roof-vertex copy (0.5) against patched and against old: **369 differing pixels outside the mask in the day view (x 343..380, y 200..260), 102 in the dusk view, 0 in the wide view**: the still sees a 0.5 vertex move in two of three views. The fence copy (0.1): 0 outside the mask in all views: the still cannot see a 0.1 move of a fence vertex; the dump does. The still is a weaker check than the dump, and "no pixel differs outside the noise" says: nothing of about half a metre, from these three cameras. Lazy way to meet the still's bar: a mask that swallows the picture. The mask holds 1 567 of 8 228 cells in the day view, 2 598 in dusk, 394 in wide, and the roof copy shows through it.

### What was not tested

- Not as played (`-ownAsPlayed 1`) and not with all three miners: the still is of the models in the place, the miner is in the mask; the stills' wind was set to zero to make them repeatable, so wind-driven foliage sway was not compared (it is a shader of the clock, not of the file).
- Only three cameras at two hours; nothing at night, no play-through of the game, no navmesh or collider bake (the scenes reference House.fbx and Nature.fbx by file ID; those IDs are the same, the dump shows).
- No other Unity version, no Blender 4.5 import, no other FBX reader; the patched files were read by my reader, Blender 4.4.0 and Unity 6000.6.0f1.
- The FBX header time stamp is unchanged (the files still say they were made on 2026-10-02).
- Not seen by the Game Director; the passes are of the rig and the editor, not of feel or looks.
- One scratch file of mine, `patched/House_30.fbx` (the patch run a second time to show it repeats; `cmp` against `patched/House.fbx`: identical), was deleted at once; nothing else in `D:/Dev/fbx_reexport` was deleted.

### Found, not looked into

`house.py`'s curtain face order is not fixed from one export to the next: the first attempt's re-export put 44 of the curtain's 284 faces in another place, with the same vertices, UVs and material. Whether it changes at every export (a second export without `--paint` would show it) or only between machines or Blender builds is not known; it only matters if the house is ever exported again from `house.py`.

### Committed, pushed, handed over

- D:/Dev/WG, branch claude/wip-round: commit `b76f311` (by path: House.fbx, Nature.fbx, `FbxReexportCheck.cs` and its new `.meta`); the committed blobs equal the patched files by `git hash-object` (House `d65e1d59387288ced1684759976e0027bae0aeaf`, Nature `0881aaab9cfd61a7df80198242233c51578f43b5`). The two FBX `.meta` files are not in the commit (unchanged). `git push origin claude/wip-round`: `7c6edf0..b76f311`. Left out as told: the three ProjectSettings files, Assets/ML-Agents, the four `.onnx` files.
- For the main project's commit, which the master makes: `D:/Dev/fbx_reexport/final/House.fbx` and `Nature.fbx` (same blobs as above; sha1 bba34e2e..., 17142468...). Nothing was committed or changed in the main project besides this report.
- Search of everything written (scripts, outputs, dumps, logs, this report, the test file) for the profile path and the name, built at run time (`scripts/private_search.py`; FBX, image and binary files of the old and bad copies excluded, they hold the path by nature): no hit.
- Rung reached: compiled, tested on a rig (Unity editor batch mode, the dump tool and a masked still), the moved-vertex copies shown to be caught by the dump and, for the larger one, by the still; not tested as played; not seen by or accepted by the Game Director. `D:/Dev/fbx_reexport` is left whole (including `old/`, `bad/`, `new/` of the first attempt) for the sceptic.

### Read by the master, and the main project (November 17, about 18:30)

- The workflow `wf_85116e9c-5ed` ran 18:04-18:21; this section and `review_fbx_patch.md` were read to their end.
  The sceptic, with its own FBX reader, found no claim of substance contradicted, and one miscount (House's Blender
  dump has 10 material lines, not 11), mended above.
- Checked again by the master: `git hash-object` of `old/` gives 3097f66f... and d70f18f5..., the committed blobs;
  `final/` gives d65e1d59... and 0881aaab..., the blobs of `b76f311`; `D:/Dev/WG` HEAD and `origin/claude/wip-round`
  are both `b76f311` (pushed); the name, searched for at run time, is in neither final file; and the two `.meta` files
  are the same on `claude/wip-round` and `claude/worker-showcase` (an empty `git diff` between the branches), so the
  Unity comparison, made under `claude/wip-round`'s import settings, holds for both.
- The two final files were copied over the main project's (no Unity running; `git hash-object` after the copy:
  d65e1d59..., 0881aaab...) and committed on `claude/worker-showcase` with this report and the review.
- `D:/Dev/fbx_reexport` (66 MB) is kept for now: this report cites its checks and dumps, and its tools serve the
  curtain question above. It goes to the Recycle Bin when that question is closed or dropped.
