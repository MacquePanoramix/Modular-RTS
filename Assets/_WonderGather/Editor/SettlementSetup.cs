using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
namespace WonderGather.Editor
{
    public static class SettlementSetup
    {
        public const string ScenePath="Assets/_WonderGather/Scenes/TheSettlement.unity";
        [MenuItem("Wonder Gather/Create Settlement Scene")]
        public static void Create()
        {
            if(File.Exists(ScenePath)) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene(GathererSetup.ScenePath);
            var definition=ScriptableObject.CreateInstance<BuildingDefinition>();
            AssetDatabase.CreateAsset(definition,"Assets/_WonderGather/Data/Workshop.asset");
            var good=Material("BuildValid",new Color(.3f,.9f,.4f));var bad=Material("BuildInvalid",new Color(.95f,.25f,.2f));
            var material=Material("Workshop",new Color(.6f,.4f,.2f));
            var root=new GameObject("Workshop");
            var model=GameObject.CreatePrimitive(PrimitiveType.Cube);model.name="Workshop structure";model.transform.SetParent(root.transform,false);
            model.transform.localScale=new Vector3(3,.25f,3);model.transform.localPosition=Vector3.up*.125f;
            model.GetComponent<Renderer>().sharedMaterial=material;
            // Full footprint is reserved immediately, including during construction.
            Object.DestroyImmediate(model.GetComponent<Collider>());
            var collider=root.AddComponent<BoxCollider>();collider.size=new Vector3(3,2.5f,3);collider.center=Vector3.up*1.25f;
            var obstacle=root.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=collider.size;obstacle.center=collider.center;obstacle.carving=true;
            root.AddComponent<BuildingSite>().Configure(definition,model.transform);
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/_WonderGather/Prefabs/Workshop.prefab");Object.DestroyImmediate(root);
            foreach(var unit in Object.FindObjectsByType<SelectableUnit>()) unit.gameObject.AddComponent<Builder>();
            var selection=Object.FindAnyObjectByType<SelectionController>();
            selection.gameObject.AddComponent<ConstructionController>().Configure(selection,Object.FindAnyObjectByType<ResourceDepot>(),prefab.GetComponent<BuildingSite>(),definition,good,bad);
            if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new System.Exception("Settlement save failed");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(GathererSetup.ScenePath,true),new EditorBuildSettingsScene(GroupSetup.ScenePath,true),new EditorBuildSettingsScene(WandererSetup.ScenePath,true)};
            AssetDatabase.SaveAssets();Debug.Log("SETTLEMENT_SETUP_OK");
        }
        private static Material Material(string name,Color color)
        {
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};
            AssetDatabase.CreateAsset(material,"Assets/_WonderGather/Materials/"+name+".mat");return material;
        }
        public static void BuildWindows()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/WindowsSettlement/WonderGather.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Settlement build failed");
            Debug.Log("SETTLEMENT_BUILD_OK");
        }
    }
}
