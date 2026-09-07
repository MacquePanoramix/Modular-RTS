using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace WonderGather.Editor
{
    public static class GathererSetup
    {
        public const string ScenePath = "Assets/_WonderGather/Scenes/TheGatherer.unity";
        [MenuItem("Wonder Gather/Create Gatherer Scene")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Gatherer scene already exists."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(GroupSetup.ScenePath);
            var node = Marker("Supplies", new Vector3(-10, 0, 8), new Color(.25f,.7f,.3f), PrimitiveType.Sphere).AddComponent<ResourceNode>();
            var home = Marker("Supply depot", new Vector3(-10, 0, -9), new Color(.25f,.5f,.9f), PrimitiveType.Cube).AddComponent<ResourceDepot>();
            var units = Object.FindObjectsByType<SelectableUnit>();
            for (int i = 0; i < units.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / units.Length;
                units[i].gameObject.AddComponent<Gatherer>().Configure(home, new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle)) * 3.4f);
            }
            Object.FindAnyObjectByType<WandererHud>().ConfigureEconomy(home,node);
            var camera = new SerializedObject(Object.FindAnyObjectByType<RtsCamera>());
            camera.FindProperty("zoomSensitivity").floatValue = .005f; camera.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new System.Exception("Gatherer scene save failed");
            EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(GroupSetup.ScenePath,true),new EditorBuildSettingsScene(WandererSetup.ScenePath,true)};
            AssetDatabase.SaveAssets();
            Debug.Log("GATHERER_SETUP_OK");
        }
        private static GameObject Marker(string name, Vector3 position, Color color, PrimitiveType shape)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = name;
            go.transform.position = position + Vector3.up; go.transform.localScale = new Vector3(2,2,2);
            // Root sits on the ground so interaction offsets share the NavMesh height.
            var root = new GameObject(name); root.transform.position = position; go.transform.SetParent(root.transform,true);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) {color=color};
            AssetDatabase.CreateAsset(material, "Assets/_WonderGather/Materials/" + name.Replace(" ","") + ".mat");
            go.GetComponent<Renderer>().sharedMaterial = material;
            return root;
        }
        public static void BuildWindows()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) WandererSetup.CapturePreview(ScenePath,"Docs/Images/TheGatherer.png");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{ScenePath}, locationPathName="Builds/WindowsGatherer/WonderGather.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Gatherer build failed");
            Debug.Log("GATHERER_BUILD_OK");
        }
    }
}
