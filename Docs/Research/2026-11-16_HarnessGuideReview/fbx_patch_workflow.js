export const meta = {
  name: 'fbx-patch-paths',
  description: 'Rewrite only the texture path fields of House.fbx and Nature.fbx (every other byte kept), prove them the same models in Blender and in Unity, commit in D:/Dev/WG',
  phases: [
    { title: 'Patch', model: 'sonnet', detail: 'one builder: the path fields rewritten, checked in the node trees, in Blender and in Unity' },
    { title: 'Check', model: 'sonnet', detail: 'a sceptic: each claim of sameness against the dumps' },
  ],
}

const MAIN = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer'
const FOLDER = MAIN + '/Docs/Research/2026-11-16_HarnessGuideReview'
const ART = 'Assets/_WonderGather/Art/OrdinaryPlace'
const WORK = 'D:/Dev/fbx_reexport'

const RULES = [
  'Rules for this task:',
  '- The profile path is private. The user\'s name is the last part of $USERPROFILE. Never write that name, or the profile folder\'s path in any form, into a file, a script, a report, a commit message or your final text. In scripts, build it at run time from $USERPROFILE; everywhere else show it as <profile> or <name>. Search every file you wrote for it with a search built at run time, and mask what it finds.',
  '- Work outside the profile folder: every output goes under ' + WORK + ', never a temporary folder under the profile. For Blender, set TEMP and TMP to ' + WORK + '/tmp, as the first attempt did.',
  '- Running Unity: only in D:/Dev/WG (branch claude/wip-round), one Unity at a time, with nothing heavy beside it, and always with a LIMIT:',
  '  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> "<testFilter>" [args]',
  '  It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. Read logs through grep, not whole. The result XML says asserts="0" even for a failed test: that count is no evidence; read the test\'s own result and its log lines. If a run hangs past its LIMIT, stop only the Unity whose command line names D:/Dev/WG. A run stopped half-way can fail the next launch once with "Scripts have compiler errors"; then launch again.',
  '- Before the first Unity run, check that no Unity runs on D:/Dev/WG. If one does, wait, and check every few minutes, up to an hour.',
  '- Gameplay and tools stay under Assets/_WonderGather. No UnityEditor in runtime code. Never delete, recreate or edit an existing .meta file: the GUIDs and import settings must stay as they are. Do not change any FBX import settings. A new script of yours gets its own new .meta from Unity; that is fine.',
  '- In D:/Dev/WG commit by path only; never stage everything, never commit with -a. Leave out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.',
  '- Do not touch the main project ' + MAIN + ' except to read it and to write your report there: it holds the user\'s own uncommitted files. Commit nothing there.',
  '- A guard hook refuses some git commands, even when a command\'s text only mentions them: staging everything; discarding work (git checkout -- and git restore of files: copy files back with cp instead); a test run and a commit in one command; a heredoc holding a doubled backslash; and a commit or push carrying the profile path. So write commit messages and scripts with the Write tool, commit with git commit -F <file>, and end each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>',
  '- Downloads need the user\'s approval: download nothing.',
  '- Delete nothing in ' + WORK + ': the sceptic reads it after you, and the master tidies it once the work is committed.',
  '- The plan\'s usage is watched. Check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch "select:mcp__ccd_session_mgmt__get_usage") before you begin, before each Blender or Unity run, and before copying the patched files over the old ones in D:/Dev/WG. If the 5-hour window passes 80% or the week passes 85%, stop at a point where your report is true, and return. Never stop with the patched files copied over the old ones in D:/Dev/WG but not yet compared: either finish that comparison, or copy the old files back from ' + WORK + '/old first.',
].join('\n')

const BUILD = [
  'Two models in a Unity 6000.6.0f1 project, ' + ART + '/House.fbx and Nature.fbx, carry the path of the user\'s own profile folder in their texture path fields. The path is that of a temporary folder they were exported through, and it names the person. The user chose to have the same models without it, with the same links in the scenes; the old versions stay in the git history.',
  'A first attempt exported both again from their Blender scripts. Nature came out the same, but House\'s curtain mesh came out with 44 of its 284 faces in another order, so that attempt stopped by its own rule. Read its report first, to its end: ' + FOLDER + '/fbx_reexport.md.',
  'The master\'s call, within the user\'s choice: leave the re-export aside, and rewrite only the path fields of the old files, keeping every other byte. Then the models are the same models, face for face.',
  'The first attempt\'s tools are in ' + WORK + '/scripts:',
  '- fbxtree.py, a binary FBX reader and writer, with the commands scan, tree and roundtrip;',
  '- normtree.py;',
  '- blender_dump.py, with run_dumps.sh;',
  '- FbxReexportCheck.cs, a Unity dump, written but never compiled;',
  '- curtain.py.',
  'The old files are in ' + WORK + '/old. First check that they are the committed blobs: git hash-object must give House 3097f66fcd8d5d9a3c2dbe47e40f734038d2f233 and Nature d70f18f5280da707deb641fc2998a7cec2f90592.',
  '',
  'Do, in order:',
  '1. Add a patch command to fbxtree.py. In every Texture node\'s FileName, every Video node\'s Filename and every Video node\'s Properties70 P "Path", set the value to "." (the same as their RelativeFilename). Change nothing else. Write ' + WORK + '/patched/House.fbx and Nature.fbx. Then show that the patch did only that:',
  '   - the roundtrip of each old file is still byte-identical;',
  '   - the scan of the patched files finds the path in no form (every form the first scan used), and the only absolute path left is Blender\'s placeholder /foobar.fbx;',
  '   - the tree diff of old against patched shows the path fields and nothing else. The UIDs are the same, so no renaming is needed. Count the changed fields: House should have 30, Nature 12. The header\'s time stamp must be unchanged;',
  '   - known-bad cases: a patched copy with one path field left as it was must be caught by the scan; the moved-vertex copy in ' + WORK + '/bad must show in the tree diff.',
  '2. Run the Blender import dump on old, patched and the known-bad copy: scripts/run_dumps.sh with blender_dump.py; Blender 4.4.0 at D:/Programs/Blender/blender.exe, -b --factory-startup. Old against patched must be identical except the images\' file paths. The known-bad copy must differ in its mesh hash.',
  '3. Compare in Unity, in D:/Dev/WG.',
  '   - Put FbxReexportCheck.cs under Assets/_WonderGather (an Editor folder, or an EditMode test) and make it compile. For each FBX it dumps:',
  '     - every sub-asset\'s type, name and local file ID (AssetDatabase.TryGetGUIDAndLocalFileIdentifier);',
  '     - each mesh\'s vertex, submesh and index counts, bounds, and a hash of its vertices and of its indices;',
  '     - the materials\' names and the textures they hold;',
  '     - the hierarchy with transforms;',
  '     - which of those local IDs the project\'s scenes and prefabs reference, with the file\'s GUID.',
  '   - Run it on the old files.',
  '   - Copy the patched files over the old ones, the .meta files untouched, and let Unity import them.',
  '   - Run it again, and diff. Everything must be identical.',
  '   - git diff of the two .meta files must be empty.',
  '   - Show that the dump can fail: run it once on the moved-vertex copy, then put the patched file back.',
  '   - If the project\'s still tool can render the place in one run each (CaptureTools.Render, as DuskZoomCapture uses it), take a still before and after and compare them pixel by pixel. Otherwise say so.',
  '   - If anything differs, copy the old files back from ' + WORK + '/old, let Unity import them, commit nothing, and report.',
  '4. If all is the same:',
  '   - commit the two FBX files in D:/Dev/WG by path, with the dump tool if it holds no private path and is worth keeping;',
  '   - push with git push origin claude/wip-round;',
  '   - copy the patched files to ' + WORK + '/final, for the main project\'s commit, which the master makes.',
  '',
  'The report: add a section to ' + FOLDER + '/fbx_reexport.md, "Patched instead (November 17, evening)". Write it early and keep it current. Keep the first attempt\'s sections as they are. It says:',
  '- what was changed, and how;',
  '- each check\'s result, quoted;',
  '- the Unity runs by name, each read to its end;',
  '- what was not tested;',
  '- that house.py\'s curtain face order is not fixed from one export to the next (found, not looked into).',
  'Follow AGENTS.md "Claims and checks", which you have: say the rung reached, and show that each check can fail.',
  '',
  RULES,
  '',
  'Return through the structured output:',
  '- done: true only if both files were committed and pushed in D:/Dev/WG and copied to ' + WORK + '/final; false if you stopped for any reason (a difference, the usage guard, an error);',
  '- worktreeClean: true if D:/Dev/WG holds no change of yours that is not committed (say which in the summary if false);',
  '- summary, in at most 200 words: the patch, each check, the Unity diff, the commit, the path of the final files, where you stopped and why if you did, and anything not done.',
].join('\n')

const BUILT = {
  type: 'object',
  properties: {
    done: { type: 'boolean' },
    worktreeClean: { type: 'boolean' },
    summary: { type: 'string' },
  },
  required: ['done', 'worktreeClean', 'summary'],
}

const CHECK = [
  'You check, as a sceptic, a claim that two models are the same models as before, with only their texture path fields rewritten. You did not make them.',
  'In a Unity project, Assets/_WonderGather/Art/OrdinaryPlace/House.fbx and Nature.fbx held the path of the user\'s profile folder in those fields. Their bytes were rewritten so that the fields read "." and nothing else changed.',
  'Shown to you:',
  '- the report, ' + FOLDER + '/fbx_reexport.md, and its section "Patched instead";',
  '- the files and dumps it names, in ' + WORK + ' (old, patched, final, bad, scripts) and D:/Dev/WG/TestRuns;',
  '- the commit in D:/Dev/WG: git -C D:/Dev/WG log --oneline -3, and its diff;',
  '- the old files, from git history.',
  'Do not run Unity, do not edit anything but your file, and commit nothing. You may run the Python tools in ' + WORK + '/scripts yourself to check a claim. Your file: ' + FOLDER + '/review_fbx_patch.md. Write it early.',
  'Your question: is each claim in the report what its files, dumps and logs say?',
  '- Byte for byte, do the patched files differ from the old only in the path fields and in the lengths and offsets that follow from them? Check it yourself with a byte or tree comparison.',
  '- Is the path gone in every form?',
  '- Did each dump cover what it claims: every object, mesh, material, and every sub-asset with its local ID?',
  '- Was each check shown failing on a changed copy?',
  '- Is anything a scene or a prefab references changed?',
  '- Are the .meta files untouched?',
  'Quote both sides of every number you check. Say what you were shown and did not read. "No finding" is a good result.',
  RULES,
  'Return as your final text, in at most 150 words: your findings, most serious first.',
].join('\n')

phase('Patch')
const built = await agent(BUILD, { label: 'builder', phase: 'Patch', model: 'sonnet', effort: 'high', schema: BUILT })
if (!built) {
  log('The builder returned nothing; the check is skipped.')
  return { built: null }
}
if (!built.done) {
  log('The builder stopped before the end; the check is skipped until the work is finished.')
  return { built, checked: null }
}
phase('Check')
const checked = await agent(CHECK, { label: 'sceptic', phase: 'Check', model: 'sonnet', effort: 'medium' })
return { built, checked }
