using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace WonderGather.Editor
{
    public static class GroupSetup
    {
        public const string ScenePath = "Assets/_WonderGather/Scenes/TheGroup.unity";

        [MenuItem("Wonder Gather/Create Group Scene")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Group scene already exists; open TheGroup from Scenes."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WandererSetup.ScenePath);
            var first = Object.FindFirstObjectByType<SelectableUnit>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_WonderGather/Prefabs/Wanderer.prefab");
            var units = new SelectableUnit[8];
            for (int i = 0; i < units.Length; i++)
            {
                var unit = i == 0 ? first : ((GameObject)PrefabUtility.InstantiatePrefab(prefab, scene)).GetComponent<SelectableUnit>();
                unit.name = "Wanderer " + (i + 1);
                unit.transform.position = new Vector3(-9 + i % 4 * 2, 0, -2 + i / 4 * 2);
                var agent = unit.GetComponent<NavMeshAgent>();
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.avoidancePriority = 30 + i * 5;
                PrefabUtility.RecordPrefabInstancePropertyModifications(unit.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(agent);
                units[i] = unit;
            }
            var selection = Object.FindFirstObjectByType<SelectionController>();
            selection.ConfigureUnits(units);
            EditorUtility.SetDirty(selection);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new System.Exception("Could not save TheGroup.");
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(WandererSetup.ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("GROUP_SETUP_OK");
        }

        public static void CapturePreview() => WandererSetup.CapturePreview(ScenePath, "Docs/Images/TheGroup.png");

        public static void BuildWindows()
        {
            // Headless validation must not replace a rendered preview with a blank image.
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) CapturePreview();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsGroup/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Group build failed: " + report.summary.result);
            Debug.Log("GROUP_BUILD_OK");
        }
    }
}
