using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WonderGather.Editor
{
    public static class CivilizationSetup
    {
        public const string ScenePath="Assets/_WonderGather/Scenes/TheCivilization.unity";
        public const string VariantPath="Assets/_WonderGather/Scenes/TheProvisionedCivilization.unity";
        private const string Data="Assets/_WonderGather/Data/Civilizations/";
        [MenuItem("Wonder Gather/Create Civilization Scenes")]
        public static void Create()
        {
            if(File.Exists(ScenePath) || File.Exists(VariantPath)) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Data);AssetDatabase.Refresh();
            var scene=EditorSceneManager.OpenScene(ProductionSetup.ScenePath);
            var worker=Save<UnitBlueprint>("Worker");var workshop=Save<BuildingBlueprint>("Workshop");var home=Save<BuildingBlueprint>("StartingBase");
            worker.Configure("worker","Worker",AssetDatabase.LoadAssetAtPath<WorkerProductionDefinition>("Assets/_WonderGather/Data/WorkerProduction.asset"),true,workshop);
            workshop.Configure("workshop",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_WonderGather/Prefabs/ProductionWorkshop.prefab").GetComponent<BuildingSite>(),worker);
            var baseObject=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_WonderGather/Prefabs/ProductionWorkshop.prefab"));
            Object.DestroyImmediate(baseObject.GetComponent<UnitProducer>());baseObject.AddComponent<ResourceDepot>();
            var baseData=Save<BuildingDefinition>("BaseConstruction");
            var properties=new SerializedObject(baseData);properties.FindProperty("displayName").stringValue="Starting base";properties.ApplyModifiedPropertiesWithoutUndo();
            var site=baseObject.GetComponent<BuildingSite>();
            var siteData=new SerializedObject(site);siteData.FindProperty("definition").objectReferenceValue=baseData;siteData.ApplyModifiedPropertiesWithoutUndo();
            baseObject.GetComponentInChildren<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_WonderGather/Materials/Supplydepot.mat");
            var basePrefab=PrefabUtility.SaveAsPrefabAsset(baseObject,"Assets/_WonderGather/Prefabs/CivilizationBase.prefab");Object.DestroyImmediate(baseObject);
            home.Configure("base",basePrefab.GetComponent<BuildingSite>());
            var standard=Save<CivilizationDefinition>("LittleSettlement");
            standard.Configure("Little Settlement",home,0,new[]{new StartingUnit(worker,8)},new[]{worker},new[]{home,workshop});
            var variant=Save<CivilizationDefinition>("ProvisionedSettlement");
            variant.Configure("Provisioned Settlement",home,40,new[]{new StartingUnit(worker,3)},new[]{worker},new[]{home,workshop});
            foreach(var asset in new Object[]{worker,workshop,home,baseData,standard,variant}) EditorUtility.SetDirty(asset);
            foreach(var unit in Object.FindObjectsByType<SelectableUnit>()) Object.DestroyImmediate(unit.gameObject);
            Object.DestroyImmediate(Object.FindAnyObjectByType<ResourceDepot>().gameObject);
            var selection=Object.FindAnyObjectByType<SelectionController>();selection.ConfigureUnits(System.Array.Empty<SelectableUnit>());
            var construction=Object.FindAnyObjectByType<ConstructionController>();var hud=Object.FindAnyObjectByType<WandererHud>();var node=Object.FindAnyObjectByType<ResourceNode>();
            hud.ConfigureEconomy(null,node);
            var session=selection.gameObject.AddComponent<CivilizationSession>();session.Configure(standard,selection,construction,hud,node);
            if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new System.Exception("Civilization scene save failed.");
            session.Configure(variant,selection,construction,hud,node);
            if(!EditorSceneManager.SaveScene(scene,VariantPath)) throw new System.Exception("Civilization variant save failed.");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(VariantPath,true)}.Concat(EditorBuildSettings.scenes).ToArray();
            AssetDatabase.SaveAssets();
            if(!CivilizationValidator.Validate(standard).CanInstantiate || !CivilizationValidator.Validate(variant).CanInstantiate) throw new System.Exception("Sample civilization validation failed.");
            Debug.Log("CIVILIZATION_SETUP_OK");
        }
        private static T Save<T>(string name) where T:ScriptableObject
        {var asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,Data+name+".asset");return asset;}
        public static void BuildWindows()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath,VariantPath},locationPathName="Builds/WindowsCivilization/WonderGather.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Civilization build failed.");
            Debug.Log("CIVILIZATION_BUILD_OK");
        }
    }
}
