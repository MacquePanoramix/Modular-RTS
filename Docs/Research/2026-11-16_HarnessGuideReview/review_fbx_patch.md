# Sceptic review: the FBX path patch (November 17, evening)

Reviewer: a sceptic who did not make the patch. Rung of this review: run (my own scripts, read to the end). Nothing committed, no Unity or Blender run, nothing edited but this file. Profile folder shown as `<profile>`, name as `<name>`.

## Verdict

No finding that contradicts a claim of substance. One small miscount in the report (below). Each claim was checked against bytes, not against the author's tool: I wrote my own FBX binary reader (`D:/Dev/fbx_reexport/scripts/sceptic_cmp.py`, `sceptic_abs.py`, `sceptic_bad.py`), which shares no code with `fbxtree.py`.

## Byte and tree comparison (old against patched), both sides

- Hashes: `sha1sum` of `old/House.fbx` = fa7bfd96... and the file at `b76f311^` in D:/Dev/WG = fa7bfd96... (same; Nature 5b29e868 and 5b29e868). Patched = final = committed blob: House bba34e2e (`git hash-object` d65e1d59..., the report's value), Nature 17142468 (0881aaab..., the report's value). The old files are what git history holds.
- Parse: both versions of both files parse with no error (FBX 7400, every node's end offset and property length consistent; old House 1225 nodes, patched 1225, same paths in the same order; Nature 963 and 963).
- Nodes whose property bytes differ: House **30** (`/Objects/Texture/FileName` 10, `/Objects/Video/Filename` 10, `/Objects/Video/Properties70/P` 10), Nature **12** (4, 4, 4). Report: 30 and 12. In all of them the non-string bytes of the property block are equal, and the only difference is one string, 149 bytes before and `.` after. In the `P` nodes the strings are `Path`, `KString`, `Url`, empty (equal) and then the changed one. Every other node: property bytes identical (House 731 164 of 736 124 property bytes lie in identical nodes; the 4 960 left are the property bytes of the 30 changed nodes; Nature 1 533 285 of 1 535 269).
- Sizes: House 763 580 - 759 132 = 4 448 = 30 x 148 (4 440) + 8 bytes of footer padding (17 zero bytes before, 9 after). Nature 1 557 692 - 1 555 916 = 1 776 = 12 x 148, padding 8 and 8 (no change). Matches the report's "30 fields, footer padding recomputed".
- Fixed parts: first 27 header bytes equal in both files; the 16-byte footer id equal; the padding all zero in both, and the end of the padding lands on a multiple of 16 (position 0 mod 16) in old and patched; the last 140 bytes equal. Header time stamp lives in a node whose bytes are identical (not in the differing list).
- What I did not do: compare the 4 bytes of each node's end-offset and property-length header word by word against a recomputation; I checked that each is consistent in the patched file (parse passes), and the sizes add up. The "lengths and offsets that follow" are therefore consistent, not individually compared.
- `RelativeFilename` in the old files: `.` in every node (House and Nature), as the report says; the patched fields now equal it.

## Is the path gone in every form

Searched at run time (built from `$USERPROFILE`): the whole folder path, with forward slashes, with doubled backslashes, the name alone, each as given, lower and upper case, UTF-8 and UTF-16-LE, over the raw bytes and over every decompressed array: old House 60 hits and old Nature 24 (the report's 60 and 24); **patched House 0, Nature 0**; decompressed arrays 0 in both. Absolute-looking strings left in the patched files: only `/foobar.fbx` (both), as reported. Not searched: other encodings (UTF-32, base64, reversed text), any file besides the two FBX.

## Was each check shown failing

- Scan: the one-left copy `bad/House_oneleft.fbx` differs from the patched file in exactly one node (`/Objects/Video/Filename`) and holds 1 hit of the name by my search; the report: "private-path hits total: 2" (folder and name: one field). Consistent. Old files as the other known-bad: 60 and 24 above.
- Tree diff: `bad/House_vertex.fbx` and `House_roofvertex.fbx` are built from the old file (they still hold the path, 30 name hits each); my reader shows 31 differing nodes each: the 30 path fields plus `/Objects/Geometry/Vertices`. So my instrument also catches a moved vertex.
- Blender dump: `diff old_House.txt bad_House_vertex.txt` shows `mesh Fence positions=71313f4836fd4a66` against `a6a2a3c73c10f49f` and `loop_normals`, as quoted. Each of the 5 dumps ends with `DUMP_DONE` (1 per log).
- Unity dump: `dump_p1.txt` against `dump_bad1.txt` differs in one line (Fence `posHash=8f190c750ec1a665` against `e9e0adff4d794c5e`); against `dump_bad2.txt` in the `House_Roof` line (posHash differs). Quoted in the report, confirmed.
- Not shown failing: the still comparison's own "0 outside the mask" was read only through the report's numbers and `mask_compare.txt` was not opened by me; I did not recompute any pixel comparison (the PPM files and `mask_compare.py` were not read).

## Did each dump cover what it claims

- Unity dump (`unity/dump_*.txt`, 206 lines each): 130 `asset` lines = GameObject 32, Transform 32, Mesh 22, MeshFilter 22, MeshRenderer 22 (House and Nature together), each with its local ID; 2 `file` lines with GUIDs; 2 hierarchy blocks; 39 `ref` lines + `references=39`. old2, old3, old4 each `cmp` identical to p1, p2, p3 (9 pairs, byte-identical files of 206 lines). Report: 206 lines, 39 references: confirmed. The code loads `LoadAllAssetsAtPath`, so every sub-asset is listed; there are no Material or Texture sub-assets (the report says the import makes none; the dump prints none: consistent). The dump does **not** print the model-root prefab (100100000) as an asset line; it counts it by name, as stated.
- References: independent grep of the worktree (Assets, ProjectSettings, Packages) for the two FBX GUIDs: only the two `.meta` files and `Scenes/TheOrdinaryPlace.unity`. In the scene: House GUID 61 occurrences, 27 distinct `fileID`s; Nature 108 occurrences, 12 distinct. Dump: 27 + 12 = 39 `ref` lines (28 and 13 lines holding the GUID, each with its `file` line). Equal. The dump's reference search covers `.unity .prefab .asset .mat .controller`; my search covered every text file, and found no other referencing file type. (Binary-only files, such as a baked NavMesh or lightmap, would not show in a text grep; none seen.)
- Blender dump: House 118 lines, Nature 113 in each of old and patched; lines are object 36, mesh 70, material 11, image 1 in House. Old against patched: House 22 differing diff-lines = 10 `material` + 1 `image` (each side); Nature 10 = 4 + 1. All of them are the `images=[...]` file location, `...\old` against `...\patched`. No mesh or object line differs. **Report miscount:** it says House has "11 `material ... images=` lines and the one `image` line" (that would be 12 per side); the files show 10 material lines and 1 image line (11 per side). Nature "5 lines" is right (4 + 1). Harmless, but the number as written is not what the dump says.
- Blender checks: all objects, meshes (hashes of positions, faces, face materials, loop normals, UVs, colours) are in the dump by its listing; I did not read `blender_dump.py` itself to see whether, for example, vertex groups or shape keys are included.

## Is anything a scene or prefab references changed

No. Only `TheOrdinaryPlace.unity` references the two files (above), by the same 39 (GUID, local ID) pairs, all `present` in the patched import, `missing=0`; the sub-asset list and each mesh's index, vertex, normal and UV hashes are identical between old and patched runs. Commit `b76f311` touches exactly four paths: House.fbx, Nature.fbx, `FbxReexportCheck.cs` and its `.cs.meta`.

## `.meta` files

`git show --stat b76f311` lists no `House.fbx.meta` or `Nature.fbx.meta`. Their last commit is `c02df06` (the Ordinary Place build), not this one. `git diff HEAD --stat -- '*.meta'` in D:/Dev/WG: empty. `git status --short` there lists only the ProjectSettings and ML-Agents / onnx items the brief said to leave out. The two FBX GUIDs: House 8b3d0a49..., Nature 4633a267..., as in the dumps.

## Shown and not read, or not checked

- Not re-run: any Unity or Blender run, any still comparison, `mask_compare.txt`, the logs of `fbx_*` runs (I read only the dumps, not the runs' `total 2 passed 2` lines or the "imported" lines).
- Not read: `FbxReexportCheck.cs` apart from a grep of its asset and reference lines; `blender_dump.py`; `fbxtree.py` (my reader replaced it).
- The report's claims of push (`7c6edf0..b76f311`) and of nothing changed in the main project: not checked.
- Textures: in both old and patched the image path resolves to nothing real (old: a scratch folder that no longer exists; patched: the folder next to the file). Nothing here tests that a texture will be found; that is outside the claim.

## Result

The patched FBX files differ from the old only in 30 (House) and 12 (Nature) path strings, the node lengths and offsets that follow, and the footer padding; the path is gone from every form I searched; the dumps cover what they say (with the House miscount above); no reference or `.meta` changed. Rung of the claims as I find them: compiled and tested on a rig, as the report says; not seen by the Game Director.
