using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace WonderGather.Editor
{
    public static class FactionCreatorSetup
    {
        public const string ScenePath="Assets/_WonderGather/Scenes/TheFactionCreator.unity";
        public const string MapPath="Assets/_WonderGather/Scenes/FactionPlaytest.unity";
        [MenuItem("Wonder Gather/Create Faction Creator")]
        public static void Create()
        {
            if(File.Exists(ScenePath) || File.Exists(MapPath)) return;
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var map=EditorSceneManager.OpenScene(CivilizationSetup.ScenePath);
            var session=Object.FindAnyObjectByType<CivilizationSession>();
            var data=new SerializedObject(session);data.FindProperty("initializeOnStart").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();
            session.gameObject.AddComponent<FactionPlaytestBridge>().Configure(session,session.GetComponent<RtsInput>());
            if(!EditorSceneManager.SaveScene(map,MapPath)) throw new System.Exception("Creator playtest map save failed.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Creator background").AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.075f,.095f,.105f);camera.cullingMask=0;
            var root=new GameObject("Faction creator");
            var creator=root.AddComponent<FactionCreator>();
            creator.Configure(AssetDatabase.LoadAssetAtPath<CivilizationDefinition>("Assets/_WonderGather/Data/Civilizations/LittleSettlement.asset"),camera);
            root.AddComponent<FactionCreatorView>();
            if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new System.Exception("Creator scene save failed.");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene(MapPath,true)}.Concat(EditorBuildSettings.scenes).ToArray();
            AssetDatabase.SaveAssets();Debug.Log("CREATOR_SETUP_OK");
        }
        public static void BuildWindows()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath,MapPath},locationPathName="Builds/WindowsFactionCreator/WonderGather.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Creator build failed.");
            Debug.Log("CREATOR_BUILD_OK");
        }
    }
}
