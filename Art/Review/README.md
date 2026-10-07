# Art/Review — the model quality method's tools and reports

This folder belongs to the
[model quality method](../../Docs/ArtDirection/ModelQualityMethod.md).

| Item | What it is |
|---|---|
| `sheets.py` | Turns the frames of `MinerCloseCapture` into labelled contact sheets: one per zone, one per walk view across the eight phases, and so on. `python Art/Review/sheets.py <frames> <sheets>` (needs Pillow) |
| `sweep_sheets.py` | Turns the pictures of the sweep of extreme poses into contact sheets, six poses a sheet. `python Art/Review/sweep_sheets.py <sweep folder> <sheets>` (needs Pillow) |
| `clip.py` | Puts the frames of the capture's `clip` set together: the three miners side by side as a looping moving picture (one swing, 25 frames a second) and as a strip of moments. `python Art/Review/clip.py <frames> <out>` (needs Pillow). The frames come from `MinerCloseCapture` with `-captureSets clip` |
| `bench_clip.py` | Puts the frames of the physical bench (`PhysicalBench` with `-benchFrames`) together: a grid of strengths by pickaxe weights as a moving picture, or one row of chosen pairs, and a strip of each. `python Art/Review/bench_clip.py <frames> <out> <miner> [panel size] [strength:weight,...]` (needs Pillow) |
| `row_clip.py` | Puts several bench clips side by side as one moving picture, each with its label. `python Art/Review/row_clip.py <out.gif> <panel size> <frames> <frames folder>/<tag>=<label> ...` (needs Pillow) |
| `balance_clip.py` | Puts bench clips of a body keeping its balance side by side, each with a drawing from above of its boots, where they press and its weight's point, taken from the bench's `BALANCE` lines in the run's `log.txt` (`PhysicalBalanceBench`, or `PhysicalBench` with `-benchBalance`). `python Art/Review/balance_clip.py <out.gif> <panel size> <first> <last> <side|front> <frames folder>/<tag>=<label> ...` |
| `Miners/weights_<Name>.txt` | What each miner weighs (S3): the whole, each part of the body with what it bears (its mass, where its centre is along its bone, its three moments), and each piece it wears or carries |
| `Miners/audit_<Name>.txt` | The automatic audit's last report for each miner, on the game's recorded movement: every check, its limit, its value at rest, its worst value, and where |
| `Miners/hands_<Name>.txt` | How each free hand closes on handles of several thicknesses, its own tool's among them: for each digit, how near it lies (`meets`) and how deep any of it is (`enters`); for the palm, how the handle rests |
| `Miners/tool_<Name>.txt` | The pickaxe made for each miner: its size, its grips and the handle's thickness there, its striking point, and (S3) what it weighs, where its weight is and how hard it is to turn, as measured on the model |
| `Miners/sweep_<Name>.txt` | The last report of the sweep of extreme poses (27 poses beyond the game's own movement): the same lines, with a value for each kind of pose in place of the phases. `pinched` is the percentage a sleeve or a trouser leg thins at a joint; `stretched` is how many times its length a cloth's longest edge is pulled |

**How to read a report line:**

```
FAIL sinks  HammerHandle into Apron  limit 3.5  rest 0.0  worst 3.9 at walk122  (6/118 frames)  stand 0.0 walk 3.9 turn 3.7 stop 3.3  [x, y, z]
```

- **The check** (`sinks`) and its parts.
- **`limit`:** the tolerance, in millimetres.
- **`rest`:** the value in the modelled rest pose.
- **`worst`:** the worst value on the recorded frames, and the frame.
- **`(6/118 frames)`:** how many audited frames fail.
- **`stand … stop`:** the worst value in each phase of the recording. Since
  October 6 there is a fifth, `mine`: two swings at a rock face, with the
  pickaxe in the hands (its parts are named `WorkPick…`).
- **`[x, y, z]`:** where, in the model's space, in metres.

The audit also renders a close view of each failure (`snap_*.png`) beside the
report it writes. Those and the capture frames are not kept in the
repository: they are made again by the commands in the method's
[tools table](../../Docs/ArtDirection/ModelQualityMethod.md#5-the-tools).

Remaining failures are explained in the round's log
([TheMiners.md](../../Docs/ArtDirection/TheMiners.md#where-the-round-ended)),
and the sweep's in
[its own section](../../Docs/ArtDirection/TheMiners.md#the-sweep-of-extreme-poses-october-5).
