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
