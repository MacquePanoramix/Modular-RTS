export const meta = {
  name: 'fbx-reexport-clean',
  description: 'Export House.fbx and Nature.fbx again from their Blender scripts outside the profile folder, prove them the same models in Blender and in Unity, commit in D:/Dev/WG',
  phases: [
    { title: 'Export', detail: 'one builder: where the path sits, the export again, Blender and Unity compared old against new' },
    { title: 'Check', model: 'sonnet', detail: 'a sceptic: each claim of sameness against the dumps' },
  ],
}

const MAIN = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer'
const FOLDER = MAIN + '/Docs/Research/2026-11-16_HarnessGuideReview'
const ART = 'Assets/_WonderGather/Art/OrdinaryPlace'

const RULES = [
  'Rules for this task:',
  '- The profile path is private. The user\'s name is the last part of $USERPROFILE. Never write that name, or the profile folder\'s path in any form, into a file, a script, a report, a commit message or your final text. In scripts, build it at run time from $USERPROFILE; everywhere else show it as <profile> or <name>. Search every file you wrote for it with a search built at run time, and mask what it finds.',
  '- Work outside the profile folder: use D:/Dev/fbx_reexport (make it) for every output of Blender, never a temporary folder under the profile.',
  '- Running Unity: only in D:/Dev/WG (branch claude/wip-round), one Unity at a time, with nothing heavy beside it, and always with a LIMIT:',
  '  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> "<testFilter>" [args]',
  '  It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. Read logs through grep, not whole. If a run hangs past its LIMIT, stop only the Unity whose command line names D:/Dev/WG. A run stopped half-way can fail the next launch once with "Scripts have compiler errors"; then launch again.',
  '- Before the first Unity run, check that no Unity runs on D:/Dev/WG (another workflow used it today). If one does, wait, and check every few minutes, up to an hour.',
  '- Gameplay and tools stay under Assets/_WonderGather. No UnityEditor in runtime code. Never delete, recreate or edit a .meta file: the GUIDs and import settings must stay as they are. Do not change any FBX import settings.',
  '- In D:/Dev/WG commit by path only; never stage everything, never commit with -a. Leave out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.',
  '- Do not touch the main project ' + MAIN + ' except to read it and to write your report there: it holds the user\'s own uncommitted files. Commit nothing there.',
  '- A guard hook refuses some git commands, even when a command\'s text only mentions them. These are: staging everything; discarding work; a test run and a commit in one command; a heredoc holding a doubled backslash; and a commit or push carrying the profile path. So write commit messages and scripts with the Write tool, commit with git commit -F <file>, and end each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>',
  '- Downloads need the user\'s approval: download nothing.',
  '- The plan\'s usage is watched. Check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch "select:mcp__ccd_session_mgmt__get_usage") before you begin, before each Blender or Unity run, and before copying the new files over the old ones in D:/Dev/WG. If the 5-hour window passes 80% or the week passes 85%, stop at a point where your report is true, and return. Never stop with the new files copied over the old ones in D:/Dev/WG but not yet compared: either finish that comparison, or copy the old files back from D:/Dev/fbx_reexport/old first.',
].join('\n')

const BUILD = [
  'Two models in a Unity 6000.6.0f1 project, ' + ART + '/House.fbx and Nature.fbx, carry the path of the user\'s own profile folder inside them. The path is that of a temporary folder they were exported through, and it names the person. The user chose to export both again without it: the same models, with the same links in the scenes. The old versions stay in the git history.',
  'They are made by Blender scripts in the repository, Art/Blender/OrdinaryPlace/house.py and nature.py. Each takes the output path as --fbx (see the usage lines at their tops). Assets/_WonderGather/Editor/OrdinaryPlaceSetup.cs is the Unity side. Docs/Validation.md and Docs/Research/2026-11-15_TheBestVisualQuality/world_and_props.md, in the main project, say how they were made: search them for House and Nature.',
  'Work in the validation worktree D:/Dev/WG (branch claude/wip-round). It holds the same two files. First say whether they are the same blobs as on the main project\'s branch claude/worker-showcase (git rev-parse of each branch\'s path).',
  '',
  'Do, in order:',
  '1. Find every place where the old files hold the path, and any other absolute path. List them in the report, masked: the FBX node and property, and the form of the path. Find which Blender wrote them (the FBX header names it) and which Blender this machine has. Find which version of each script made the committed file: compare the script\'s git log with the file\'s.',
  '2. Export both again into D:/Dev/fbx_reexport with the script version that made the committed file, as the usage lines say. Search the new files for the path in every form: as it is, with slashes, JSON-escaped, upper case, UTF-16. It must not be there. List the absolute paths the new files still hold.',
  '3. Compare old with new in Blender, in background with --factory-startup. Import each into an empty scene and dump, in a form a script can diff:',
  '   - every object: name, type, parent, transform;',
  '   - every mesh: counts of vertices, faces and loops, and its vertex positions, normals and UVs, rounded and hashed;',
  '   - every material: names, slots, and the images they name.',
  '   Diff the two dumps, and also the two files\' FBX node trees beyond the header. Every difference is listed and explained. If the models differ in anything but the header and the paths, stop: commit nothing, and report.',
  '4. Compare in Unity, in D:/Dev/WG.',
  '   - Write a small editor method or EditMode test under Assets/_WonderGather that dumps each FBX\'s import:',
  '     - every sub-asset\'s type, name and local file ID (AssetDatabase.TryGetGUIDAndLocalFileIdentifier);',
  '     - meshes\' vertex and submesh counts and bounds;',
  '     - materials\' names;',
  '     - the hierarchy.',
  '   - Also list which of those local IDs the project\'s scenes and prefabs reference, with the file\'s GUID.',
  '   - Run the dump on the old files.',
  '   - Copy the old files to D:/Dev/fbx_reexport/old first. Then copy the new files over the old ones (the .meta files untouched), run the dump again, and diff.',
  '   - git diff of the two .meta files must be empty.',
  '   - If the project has a tool that renders a still of the place, take one before and one after and compare them; otherwise say so.',
  '   - If anything a scene or a prefab references is lost, or changes its ID, copy the old files back from D:/Dev/fbx_reexport/old (by cp: the guard refuses git checkout and git restore of files), let Unity import them again, commit nothing, and report.',
  '5. If all is the same:',
  '   - commit the two FBX files in D:/Dev/WG by path, and the dump tool if worth keeping, and push with git push origin claude/wip-round;',
  '   - copy the new files to D:/Dev/fbx_reexport/final, for the main project\'s commit, which the master makes.',
  '',
  'The report: write ' + FOLDER + '/fbx_reexport.md early, and keep it current. It says:',
  '- where the path was, and what replaced it;',
  '- the Blender versions;',
  '- each dump\'s diff, quoted;',
  '- the Unity runs by name, each read to its end;',
  '- what was not tested.',
  'Follow AGENTS.md "Claims and checks", which you have: say the rung reached, and show that each check can fail. For example, a dump diff of a deliberately changed copy, such as one vertex moved, must show the change.',
  '',
  RULES,
  '',
  'Return through the structured output:',
  '- done: true only if both files were committed and pushed in D:/Dev/WG and copied to D:/Dev/fbx_reexport/final; false if you stopped for any reason (a difference, the usage guard, an error);',
  '- worktreeClean: true if D:/Dev/WG holds no change of yours that is not committed (say which in the summary if false);',
  '- summary, in at most 200 words: whether the two files are the same blobs on both branches; where the path was; the diffs in Blender and in Unity; the commit; the path of the final files; where you stopped and why, if you did; anything not done.',
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
  'You check, as a sceptic, a claim that two re-exported models are the same models as before. You did not make them. In a Unity project, Assets/_WonderGather/Art/OrdinaryPlace/House.fbx and Nature.fbx were exported again from their Blender scripts so that they no longer hold the path of the user\'s profile folder.',
  'Shown to you:',
  '- the report, ' + FOLDER + '/fbx_reexport.md;',
  '- the dumps and diffs it names, in D:/Dev/fbx_reexport and D:/Dev/WG/TestRuns;',
  '- the commit in D:/Dev/WG: git -C D:/Dev/WG log --oneline -3, and its diff;',
  '- the old files, from git history.',
  'Do not run Unity, do not edit anything but your file, and commit nothing. Your file: ' + FOLDER + '/review_fbx_reexport.md. Write it early.',
  'Your question: is each claim in the report what its dumps and logs say?',
  '- Is the path gone in every form?',
  '- Did each dump cover what it claims: every object, mesh, material, and every sub-asset with its local ID?',
  '- Was each check shown failing on a changed copy?',
  '- Is anything a scene or a prefab references changed?',
  '- Are the .meta files untouched?',
  'Quote both sides of every number you check. Say what you were shown and did not read. "No finding" is a good result.',
  RULES,
  'Return as your final text, in at most 150 words: your findings, most serious first.',
].join('\n')

phase('Export')
const built = await agent(BUILD, { label: 'builder', phase: 'Export', effort: 'high', schema: BUILT })
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
