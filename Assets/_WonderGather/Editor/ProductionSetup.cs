using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WonderGather.Editor
{
    public static class ProductionSetup
    {
        public const string ScenePath="Assets/_WonderGather/Scenes/TheProduction.unity";
        [MenuItem("Wonder Gather/Create Production Scene")]
        public static void Create()
        {
            if(File.Exists(ScenePath)) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene(SettlementSetup.ScenePath);
            var worker=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_WonderGather/Prefabs/Wanderer.prefab"));
            worker.AddComponent<Gatherer>();worker.AddComponent<Builder>();worker.AddComponent<ProducedWorker>();
            var workerPrefab=PrefabUtility.SaveAsPrefabAsset(worker,"Assets/_WonderGather/Prefabs/ProducedWorker.prefab");Object.DestroyImmediate(worker);
            var definition=ScriptableObject.CreateInstance<WorkerProductionDefinition>();definition.Configure(workerPrefab);
            AssetDatabase.CreateAsset(definition,"Assets/_WonderGather/Data/WorkerProduction.asset");
            var workshop=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_WonderGather/Prefabs/Workshop.prefab"));
            workshop.AddComponent<UnitProducer>().SetDefinition(definition);
            var ring=new GameObject("Workshop selection");ring.transform.SetParent(workshop.transform,false);ring.transform.localPosition=Vector3.up*.08f;
            var line=ring.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.widthMultiplier=.08f;line.positionCount=4;
            line.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_WonderGather/Materials/Selection.mat");
            line.SetPositions(new[]{new Vector3(-1.8f,0,-1.8f),new Vector3(1.8f,0,-1.8f),new Vector3(1.8f,0,1.8f),new Vector3(-1.8f,0,1.8f)});
            workshop.GetComponent<BuildingSite>().ConfigureSelection(ring);ring.SetActive(false);
            var prefab=PrefabUtility.SaveAsPrefabAsset(workshop,"Assets/_WonderGather/Prefabs/ProductionWorkshop.prefab");Object.DestroyImmediate(workshop);
            var construction=new SerializedObject(Object.FindAnyObjectByType<ConstructionController>());
            construction.FindProperty("prefab").objectReferenceValue=prefab.GetComponent<BuildingSite>();construction.ApplyModifiedPropertiesWithoutUndo();
            if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new System.Exception("Production scene save failed");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(SettlementSetup.ScenePath,true),new EditorBuildSettingsScene(GathererSetup.ScenePath,true),new EditorBuildSettingsScene(GroupSetup.ScenePath,true),new EditorBuildSettingsScene(WandererSetup.ScenePath,true)};
            AssetDatabase.SaveAssets();Debug.Log("PRODUCTION_SETUP_OK");
        }
        public static void BuildWindows()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/WindowsProduction/WonderGather.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Production build failed");
            Debug.Log("PRODUCTION_BUILD_OK");
        }
    }
}
