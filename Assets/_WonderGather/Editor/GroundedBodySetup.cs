using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // Strength and Burden, checkpoint A. Gives the shared biped heel-to-ball feet with toe
    // segments so its gait can strike, roll and push off, and sets the Living Worker to the
    // natural walking pace chosen by Luis (decision D2). Refuses to author twice.
    public static class GroundedBodySetup
    {
        private const string BodyPath = "Assets/_WonderGather/Prefabs/LivingBodyBiped.prefab";
        // A natural walk for this body (Froude ≈ 0.23 at 1.43 m hip height), matching the
        // Living Body's accepted measured pace. Higher Movement % becomes a jog.
        public const float WorkerWalkSpeed = 1.8f, WorkerAcceleration = 4;

        [MenuItem("Wonder Gather/Ground the Living Body")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play mode before authoring the grounded body.");
            var body = PrefabUtility.LoadPrefabContents(BodyPath);
            try
            {
                var biped = body.GetComponent<ProceduralBiped>();
                if (biped == null) throw new InvalidOperationException("The Living Body prefab has no ProceduralBiped.");
                var serialized = new SerializedObject(biped);
                var existing = serialized.FindProperty("toes");
                if (existing.arraySize > 0) throw new InvalidOperationException("The Living Body already has toes; inspect it before re-authoring.");
                var feet = serialized.FindProperty("feet");
                var toes = new Transform[2];
                for (int i = 0; i < 2; i++)
                {
                    var foot = feet.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                    if (foot == null || !Mathf.Approximately(foot.localScale.z, .44f))
                        throw new InvalidOperationException("The authored feet changed; review them before reshaping.");
                    // Heel to ball. The toe continues beyond the ball and bends at toe-off.
                    foot.localScale = new Vector3(.13f, .09f, .26f);
                    var toe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Object.DestroyImmediate(toe.GetComponent<Collider>());
                    SceneManager.MoveGameObjectToScene(toe, body.scene);
                    toe.name = (i == 0 ? "Left " : "Right ") + "toe";
                    toe.transform.SetParent(foot.parent, false);
                    toe.transform.SetPositionAndRotation(foot.position + foot.rotation * new Vector3(0, -.015f, .185f), foot.rotation);
                    toe.transform.localScale = new Vector3(.12f, .06f, .11f);
                    toe.GetComponent<MeshRenderer>().sharedMaterial = foot.GetComponent<MeshRenderer>().sharedMaterial;
                    toes[i] = toe.transform;
                }
                biped.ConfigureToes(toes);
                PrefabUtility.SaveAsPrefabAsset(body, BodyPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(body); }

            var worker = PrefabUtility.LoadPrefabContents(LivingWorkerSetup.PrefabPath);
            try
            {
                var agent = worker.GetComponent<NavMeshAgent>();
                if (agent == null || !Mathf.Approximately(agent.speed, 3.2f))
                    throw new InvalidOperationException("The Living Worker pace changed; review it before setting the natural walk.");
                agent.speed = WorkerWalkSpeed;
                agent.acceleration = WorkerAcceleration;
                PrefabUtility.SaveAsPrefabAsset(worker, LivingWorkerSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(worker); }
            AssetDatabase.SaveAssets();
            Debug.Log("GROUNDED_BODY_SETUP_OK");
        }

        public static void BuildWindows()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { FactionCreatorSetup.ScenePath, FactionCreatorSetup.MapPath, "Assets/_WonderGather/Scenes/EquipmentPlaytest.unity" },
                locationPathName = "Builds/WindowsGroundedBody/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Grounded Body Windows build failed: " + report.summary.result);
            Debug.Log("GROUNDED_BODY_BUILD_OK");
        }
    }
}
