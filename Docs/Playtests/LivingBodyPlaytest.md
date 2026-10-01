# The Living Body playtest

Open `Assets/_WonderGather/Scenes/TheLivingBody.unity` and press Play,
or run the local `Builds/WindowsLivingBody/WonderGather.exe` with its data
folders. The faction creator remains available separately in
`TheFactionCreator`; this experiment does not change saved factions.

This scene compares the same provisional articulated biped at a measured and
a brisk pace. The goal is to judge planted feet, terrain adaptation, weight,
starts, stops and turns. The geometry and colors are testing aids; body
proportions, species, final art and movement personality remain open.

The on-screen route buttons send both walkers across flat ground, up the
slope, or back to their starting positions; Stop both interrupts them. The
measured walker starts on the left and the brisk walker on the right.

Use the existing RTS controls: click or drag to select, Shift to add/remove
units, right-click walkable ground to move, F to focus the selection, WASD or
arrows to pan, Q/E to rotate, and the wheel to zoom. There is no separate
locomotion-control scheme to learn.

1. **Flat walk:** select one biped and give it a distant destination on flat
   ground. Focus and zoom in, then rotate to a side view. The supporting foot
   should stay planted while the other foot lifts and advances. Watch for
   sustained sliding, knee flips, feet passing through the ground or abrupt
   changes in leg length.
2. **Pace comparison:** select both and send them across the same flat area.
   The brisk unit should travel faster while keeping readable steps. Compare
   rhythm and weight, not only arrival time. Repeat with each unit individually
   if mutual avoidance obscures the motion.
3. **Starts and arrivals:** send a resting unit several body lengths away and
   let it arrive. Its body should respond smoothly to acceleration and braking,
   then settle. Wait a few seconds: planted feet should not keep drifting or
   tapping indefinitely.
4. **Turns and changed orders:** while it walks, issue a destination to one
   side, then behind it. Navigation should respond to the latest order while
   feet and body recover into a readable new heading. Small recovery steps
   are acceptable; skating, limb snaps and endless shuffling are issues.
5. **Uphill and downhill:** send it up the ramp to the plateau, pause there,
   then return to the flat ground. View from the side and front. Feet should
   find the slope, knees should bend within their reach, and the body should
   cross the ramp transitions without popping or sinking into the surface.
6. **Strategic view:** zoom back out and command both units again. Their
   direction, selection and destinations should remain easy to read. Note
   whether body movement is visible enough to add presence without making
   the units look unstable.
7. **Repeated play:** leave Play Mode and re-enter, or restart the standalone
   scene. The two initial comparison units and clean starting poses should
   return; this scene has no persistence of its own.

Please report which pace feels closest to Wonder Gather, whether the body
feels too stiff, floaty, heavy or bouncy, and the order or terrain transition
that reveals any awkward motion. A close view and a strategic view of the
same movement are especially useful for judging the intended tone.

This is a procedural presentation experiment driven by existing NavMesh
movement. It does not simulate balance forces, falling, active ragdolls,
personality, equipment, gathering animations or combat. The gentle ramp is
the current terrain test; stairs, jumps, steep slopes, moving platforms and
arbitrary creature bodies are not established capabilities. User playtesting
decides whether it reaches the grounded, expressive visual bar.
