# The Living Worker playtest

Open `Assets/_WonderGather/Scenes/TheFactionCreator.unity` and press Play.
The new body is integrated into the creator's faction playtest; there is no
separate Living Worker scene to open. The local standalone build is
`Builds/WindowsLivingWorker/WonderGather.exe`, with its data folders. Build
availability and executed checks are recorded in `Docs/Validation.md`.

Luis approved this scope on September 26, after accepting The Living Body
as adequate for the prototype on September 24. This test asks whether a
player-created worker feels present while approaching, collecting, carrying
and delivering supplies. Approval of the plan is not acceptance of its feel.

1. **Make a small faction.** In the creator, give one worker gathering and
   workshop-building permissions, one starting unit, and 40 starting supplies.
   Keep its performance at 100% movement, five carrying capacity, 100%
   gathering and 100% construction. Ensure a workshop can train it. Save
   before entering the playtest if you want to retain this design.
2. **Watch one work loop.** Select the worker and right-click the supply
   station marked by the green sphere. Press F and zoom close. It should
   approach a free position, settle its feet, face the supplies, and reach
   with its right hand. Its bundle appears only after it collects supplies.
   The selected-unit HUD reports activity and actual cargo/capacity.
3. **Follow delivery.** Let the worker fill its load, walk to the blue base,
   and deliver at a shelf. The short delivery gesture should finish before
   stored supplies rise and the bundle disappears. It should then return to
   work. Compare the HUD counts with the visible load; bundle size is a
   rough fullness cue, not a separate resource counter.
4. **Change your mind.** While it holds a partial load, right-click ordinary
   ground. It should leave work immediately, retain its cargo, and stop
   reaching at the old station. Right-click the blue base to deliver that
   load; this explicit delivery ends there. Right-click supplies to restart
   the repeating loop. Also redirect it during approach and delivery.
5. **Compare creator settings.** Return to the creator and duplicate the
   unit. Give the copy a different name, movement rate, carrying capacity
   and gathering rate, plus a starting unit. Re-enter and compare them.
   Rates should change travel/work throughput, while both bodies retain
   readable steps and carrying poses. The supported movement controls
   remain 25–200%; their 100% baseline remains 3.2 units per second.
6. **Share the station.** Try eight starting workers with gathering enabled,
   select them together, and order them to collect. They should use separate
   nearby work positions, deliver, and repeat without sharing a contact
   point. There are eight gathering and eight delivery positions. If more
   workers are trained and positions are full, they wait and retry. Let the
   finite resource run out: remaining loads should reach the base, then
   workers should stop gathering.
7. **Build and train.** With an allowed worker selected, place and finish a
   workshop, then train another unit through its production controls. The
   new unit should use the same articulated body, inherit its blueprint's
   settings and permissions, and complete the work loop. Construction keeps
   its existing behavior; a building gesture is outside this milestone.
8. **Return and retain your design.** Return to the creator, confirm the
   roster, links and settings, then save and reopen the faction. Existing
   version-4 saves and the earlier migration rules are unchanged. Save
   before stopping Editor Play Mode; the live map and its supplies are not
   saved as a match.

The usual controls apply: click/drag to select, Shift to add or toggle,
right-click to move/gather/deliver, F to focus, WASD/arrows to pan, Q/E to
rotate, and the wheel to zoom. Check from strategic height as well as close
up. The original `TheLivingBody.unity` comparison remains available.

Please judge the restraint and weight of the routine: whether the reach
feels purposeful, the carried load feels supported, and the transitions
remain readable when orders change. Note any prolonged foot sliding, limb
stretching, intersections with the station, stale gestures, or cargo/count
disagreements. The fastest setting is a useful stress case, not a commitment
to the final game's pace.

The rack, bundle, shelves, biped proportions and collecting gesture are
provisional test shapes. They do not choose a species, culture, final
resource fiction or equipment system. Navigation still owns movement;
hands visualize gameplay state rather than controlling resource accounting.
No physical balance, load penalties, construction animation, morale,
combat, multiplayer or arbitrary anatomy is established by this slice.
Several working units do not establish an RTS-scale performance budget.
