using System;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // A separate motion laboratory. Creation preserves existing scenes and build entries.
    public static class LivingBodySetup
    {
        private const string Root = "Assets/_WonderGather";
        public const string ScenePath = Root + "/Scenes/TheLivingBody.unity";
        private const string PrefabPath = Root + "/Prefabs/LivingBodyBiped.prefab";
        private const string NavMeshPath = Root + "/Scenes/LivingBodyNavMesh.asset";
        private const string RampPath = Root + "/Art/LivingBodyRamp.asset";
        private static readonly string[] MaterialNames = {
            "LivingBodyGround", "LivingBodySlope", "LivingBodyTorso", "LivingBodyLimbs",
            "LivingBodyFeet", "LivingBodySelection", "LivingBodyDestination"
        };

        [MenuItem("Wonder Gather/Create Living Body Scene")]
        public static void Create()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log("The Living Body scene already exists. Open Assets/_WonderGather/Scenes/TheLivingBody.unity; creation does not overwrite it.");
                return;
            }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (LayerMask.LayerToName(6) != "Walkable")
                throw new InvalidOperationException("The existing Walkable layer must remain at index 6.");
            RequireNewAsset(PrefabPath);
            RequireNewAsset(NavMeshPath);
            RequireNewAsset(RampPath);
            foreach (var name in MaterialNames) RequireNewAsset(Root + "/Materials/" + name + ".mat");
            foreach (var folder in new[] { "Scenes", "Materials", "Prefabs", "Art" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var groundMaterial = CreateMaterial(MaterialNames[0], new Color(.25f, .33f, .29f));
            var slopeMaterial = CreateMaterial(MaterialNames[1], new Color(.35f, .40f, .34f));
            var torsoMaterial = CreateMaterial(MaterialNames[2], new Color(.65f, .70f, .62f));
            var limbMaterial = CreateMaterial(MaterialNames[3], new Color(.43f, .48f, .43f));
            var footMaterial = CreateMaterial(MaterialNames[4], new Color(.25f, .29f, .27f));
            var ringMaterial = CreateMaterial(MaterialNames[5], new Color(.65f, .96f, .75f));
            var markerMaterial = CreateMaterial(MaterialNames[6], new Color(.96f, .85f, .40f));

            var world = new GameObject("Walkable World");
            TerrainCube("Flat ground", new Vector3(0, -.5f, -6), new Vector3(28, 1, 24), groundMaterial, world.transform);
            CreateRamp(slopeMaterial, world.transform);
            TerrainCube("Raised plateau", new Vector3(0, .5f, 14), new Vector3(16, 3, 8), slopeMaterial, world.transform);
            var surface = world.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.layerMask = 1 << 6;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.overrideVoxelSize = true;
            surface.voxelSize = .08f;
            surface.BuildNavMesh();
            if (surface.navMeshData == null) throw new InvalidOperationException("Living Body navigation bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath);

            var measured = CreateBiped(torsoMaterial, limbMaterial, footMaterial, ringMaterial);
            if (PrefabUtility.SaveAsPrefabAssetAndConnect(measured, PrefabPath, InteractionMode.AutomatedAction) == null)
                throw new IOException("Could not save the Living Body prefab.");
            measured.name = "Measured walker";
            measured.transform.position = new Vector3(-2, 0, -9);
            PrefabUtility.RecordPrefabInstancePropertyModifications(measured);
            PrefabUtility.RecordPrefabInstancePropertyModifications(measured.transform);
            var brisk = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
            brisk.name = "Brisk walker";
            brisk.transform.position = new Vector3(2, 0, -9);
            var briskAgent = brisk.GetComponent<NavMeshAgent>();
            briskAgent.speed = 2.5f;
            briskAgent.avoidancePriority = 45;
            var briskBody = brisk.GetComponent<ProceduralBiped>();
            briskBody.SetTuning(.8f, .18f, .28f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(brisk);
            PrefabUtility.RecordPrefabInstancePropertyModifications(brisk.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(briskAgent);
            PrefabUtility.RecordPrefabInstancePropertyModifications(briskBody);
            var units = new[] { measured.GetComponent<SelectableUnit>(), brisk.GetComponent<SelectableUnit>() };

            var marker = Ring("Move destination", markerMaterial, .5f);
            marker.SetActive(false);
            var cameraObject = new GameObject("RTS Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 150;
            camera.backgroundColor = new Color(.16f, .22f, .24f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.SetPositionAndRotation(new Vector3(0, 17, -19), Quaternion.Euler(42, 0, 0));
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            var controls = new GameObject("Player Controls");
            var input = controls.AddComponent<RtsInput>();
            var selection = controls.AddComponent<SelectionController>();
            selection.Configure(input, camera, marker.transform);
            selection.ConfigureUnits(units);
            var rtsCamera = cameraObject.AddComponent<RtsCamera>();
            rtsCamera.Configure(input, selection);
            var cameraSettings = new SerializedObject(rtsCamera);
            cameraSettings.FindProperty("zoomSensitivity").floatValue = .005f;
            cameraSettings.FindProperty("terrainAware").boolValue = true;
            cameraSettings.ApplyModifiedPropertiesWithoutUndo();
            controls.AddComponent<LivingBodyDemo>().Configure(input, selection, units,
                new[] { new Vector3(-2, 0, -1.5f), new Vector3(2, 0, -1.5f) },
                new[] { new Vector3(-2, 2, 14), new Vector3(2, 2, 14) });

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.58f, .62f, .64f);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save TheLivingBody scene.");
            var buildScenes = EditorBuildSettings.scenes;
            if (!buildScenes.Any(entry => entry.path == ScenePath))
                EditorBuildSettings.scenes = buildScenes.Concat(new[] { new EditorBuildSettingsScene(ScenePath, true) }).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("LIVING_BODY_SETUP_OK");
        }

        // Bounded authoring refresh for this new prototype's visual references and rest pose.
        public static void RefreshPresentation()
        {
            var root=PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                root.GetComponent<ProceduralBiped>().ConfigureRing(root.transform.Find("Selection ring"));
                PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
            }
            finally {PrefabUtility.UnloadPrefabContents(root);}
            var scene=EditorSceneManager.OpenScene(ScenePath);
            foreach(var body in Object.FindObjectsByType<ProceduralBiped>())
            {
                body.ConfigureRing(body.transform.Find("Selection ring"));body.ResetPose();
                EditorUtility.SetDirty(body);
            }
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("LIVING_BODY_PRESENTATION_OK");
        }

        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Create The Living Body scene before building.", ScenePath);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/WindowsLivingBody/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Living Body Windows build failed: " + report.summary.result);
            Debug.Log("LIVING_BODY_BUILD_OK");
        }

        private static GameObject CreateBiped(Material torsoMaterial, Material limbMaterial, Material footMaterial, Material ringMaterial)
        {
            var unit = new GameObject("Living Body Biped");
            var collider = unit.AddComponent<CapsuleCollider>();
            collider.height = 2.2f;
            collider.center = new Vector3(0, 1.1f, 0);
            collider.radius = .38f;
            var agent = unit.AddComponent<NavMeshAgent>();
            agent.speed = 1.8f;
            agent.acceleration = 3;
            agent.angularSpeed = 180;
            agent.radius = .38f;
            agent.height = 2.2f;
            agent.stoppingDistance = .12f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 40;
            unit.AddComponent<UnitMotor>();
            var selectable = unit.AddComponent<SelectableUnit>();
            var ring = Ring("Selection ring", ringMaterial, .72f);
            ring.transform.SetParent(unit.transform, false);
            ring.transform.localPosition = Vector3.up * .06f;
            selectable.Configure(ring);
            var visual = new GameObject("Provisional articulated body").transform;
            visual.SetParent(unit.transform, false);
            var pelvis = BodyPart("Pelvis", PrimitiveType.Cube, visual, new Vector3(0, 1.20f, 0), new Vector3(.50f, .22f, .30f), torsoMaterial);
            var torso = BodyPart("Torso", PrimitiveType.Capsule, visual, new Vector3(0, 1.62f, 0), new Vector3(.56f, .30f, .34f), torsoMaterial);
            var head = BodyPart("Head", PrimitiveType.Sphere, visual, new Vector3(0, 1.96f, 0), new Vector3(.34f, .38f, .34f), torsoMaterial);
            var thighs = new Transform[2];
            var shins = new Transform[2];
            var feet = new Transform[2];
            var upperArms = new Transform[2];
            var forearms = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1 : 1;
                string label = i == 0 ? "Left " : "Right ";
                var hip = new Vector3(side * .18f, 1.20f, 0);
                var knee = new Vector3(side * .19f, .65f, .23f);
                var ankle = new Vector3(side * .20f, .12f, 0);
                thighs[i] = Limb(label + "thigh", visual, hip, knee, .15f, limbMaterial);
                shins[i] = Limb(label + "shin", visual, knee, ankle, .12f, limbMaterial);
                feet[i] = BodyPart(label + "foot", PrimitiveType.Cube, visual, new Vector3(side * .20f, .07f, .07f), new Vector3(.24f, .14f, .44f), footMaterial);
                var shoulder = new Vector3(side * .36f, 1.79f, 0);
                var elbow = shoulder + new Vector3(side * .07f, -.35f, -.02f);
                var wrist = elbow + new Vector3(side * .01f, -.33f, .07f);
                upperArms[i] = Limb(label + "upper arm", visual, shoulder, elbow, .12f, limbMaterial);
                forearms[i] = Limb(label + "forearm", visual, elbow, wrist, .10f, limbMaterial);
            }
            var body = unit.AddComponent<ProceduralBiped>();
            body.Configure(pelvis, torso, head, thighs, shins, feet, upperArms, forearms);
            body.ConfigureRing(ring.transform);
            body.SetTuning(.65f, .16f, .34f);
            return unit;
        }

        private static void CreateRamp(Material material, Transform parent)
        {
            // Closed wedge with a continuous 2m rise over 10m. Its collider is also the foot-placement surface.
            var mesh = new Mesh { name = "Living Body gentle ramp" };
            mesh.vertices = new[] {
                new Vector3(-8, 0, 0), new Vector3(8, 0, 0), new Vector3(-8, 2, 10), new Vector3(8, 2, 10),
                new Vector3(-8, -1, 0), new Vector3(8, -1, 0), new Vector3(-8, -1, 10), new Vector3(8, -1, 10)
            };
            mesh.triangles = new[] { 0,2,1, 1,2,3, 4,5,6, 5,7,6, 4,0,5, 5,0,1, 6,7,2, 7,3,2, 4,6,0, 6,2,0, 5,1,7, 7,1,3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, RampPath);
            var ramp = new GameObject("Gentle slope - 11 degrees") { layer = 6 };
            ramp.transform.SetParent(parent, false);
            ramp.AddComponent<MeshFilter>().sharedMesh = mesh;
            ramp.AddComponent<MeshRenderer>().sharedMaterial = material;
            ramp.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        private static Transform Limb(string name, Transform parent, Vector3 start, Vector3 end, float width, Material material)
        {
            var direction = end - start;
            var part = BodyPart(name, PrimitiveType.Capsule, parent, (start + end) * .5f,
                new Vector3(width, direction.magnitude * .5f, width), material);
            part.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
            return part;
        }

        private static Transform BodyPart(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var part = Primitive(name, type, position, scale, material);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            return part.transform;
        }

        private static void TerrainCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var terrain = Primitive(name, PrimitiveType.Cube, position, scale, material);
            terrain.layer = 6;
            terrain.transform.SetParent(parent, true);
        }

        private static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }

        private static GameObject Ring(string name, Material material, float radius)
        {
            var result = new GameObject(name);
            var line = result.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = .045f;
            line.positionCount = 64;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius);
            }
            return result;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
            var material = new Material(shader) { name = name, color = color };
            material.SetFloat("_Smoothness", .15f);
            AssetDatabase.CreateAsset(material, Root + "/Materials/" + name + ".mat");
            return material;
        }

        private static void RequireNewAsset(string path)
        {
            if (File.Exists(path)) throw new IOException("Refusing to replace existing Living Body asset: " + path);
        }
    }
}
