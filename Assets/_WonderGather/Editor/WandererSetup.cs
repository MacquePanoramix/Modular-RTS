using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.AI.Navigation;

namespace WonderGather.Editor
{
    public static class WandererSetup
    {
        private const string Root = "Assets/_WonderGather";
        public const string ScenePath = Root + "/Scenes/TheWanderer.unity";

        public static void BuildWindows()
        {
            EditorBuildSettings.RemoveConfigObject("com.unity.input.settings.actions");
            AssetDatabase.SaveAssets();
            CapturePreview();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Windows build failed: " + report.summary.result);
            Debug.Log("WANDERER_BUILD_OK");
        }

        public static void CapturePreview()
        { CapturePreview(ScenePath, "Docs/Images/TheWanderer.png"); }

        public static void CapturePreview(string scenePath, string imagePath)
        {
            bool previousAsyncCompilation = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.OpenScene(scenePath);
            var camera = Object.FindFirstObjectByType<Camera>();
            var target = new RenderTexture(1280, 720, 24);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            texture.Apply();
            Directory.CreateDirectory("Docs/Images");
            File.WriteAllBytes(imagePath, texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(target);
            ShaderUtil.allowAsyncCompilation = previousAsyncCompilation;
        }

        [MenuItem("Wonder Gather/Create Wanderer Scene")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Wanderer scene already exists. Open it from Assets/_WonderGather/Scenes."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string folder in new[] { "Scenes", "Materials", "Prefabs", "Art", "Audio", "Data", "Settings" })
                Directory.CreateDirectory(Root + "/" + folder);
            AssetDatabase.Refresh();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(6).stringValue = "Walkable";
            tags.ApplyModifiedPropertiesWithoutUndo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var groundMaterial = Material("Meadow", new Color(.22f, .34f, .26f));
            var rockMaterial = Material("Stone", new Color(.38f, .43f, .40f));
            var unitMaterial = Material("Wanderer", new Color(.83f, .64f, .34f));
            var ringMaterial = Material("Selection", new Color(.65f, .96f, .75f));
            var markerMaterial = Material("Destination", new Color(.96f, .85f, .40f));
            var world = new GameObject("Walkable World");
            var ground = Primitive("Meadow", PrimitiveType.Cube, new Vector3(0, -.5f, 0), new Vector3(64, 1, 64), groundMaterial);
            ground.layer = 6;
            ground.transform.SetParent(world.transform);
            for (int i = 0; i < 4; i++)
            {
                var rock = Primitive("Stone " + (i + 1), PrimitiveType.Cube, new Vector3(4, 1.3f, -6 + i * 3), new Vector3(2.5f, 2.6f, 2), rockMaterial);
                rock.transform.SetParent(world.transform);
            }
            // An isolated island exercises rejection of complete-but-disconnected destinations.
            var island = Primitive("Unreachable island", PrimitiveType.Cube, new Vector3(39, -.5f, 0), new Vector3(4, 1, 4), groundMaterial);
            island.layer = 6;
            island.transform.SetParent(world.transform);
            var surface = world.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            if (surface.navMeshData == null) throw new System.Exception("Navigation bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, Root + "/Scenes/WandererNavMesh.asset");

            var unit = new GameObject("Wanderer");
            unit.transform.position = new Vector3(-4, 0, 0);
            var body = Primitive("Body", PrimitiveType.Capsule, new Vector3(-4, 1, 0), Vector3.one, unitMaterial);
            body.transform.SetParent(unit.transform, true);
            var agent = unit.AddComponent<NavMeshAgent>();
            agent.speed = 3.2f; agent.acceleration = 10; agent.angularSpeed = 280;
            agent.radius = .5f; agent.height = 2; agent.stoppingDistance = .15f;
            unit.AddComponent<UnitMotor>();
            var selectable = unit.AddComponent<SelectableUnit>();
            var ring = Ring("Selection ring", ringMaterial, .8f);
            ring.transform.SetParent(unit.transform, false);
            ring.transform.localPosition = Vector3.up * .06f;
            selectable.Configure(ring);
            PrefabUtility.SaveAsPrefabAssetAndConnect(unit, Root + "/Prefabs/Wanderer.prefab", InteractionMode.AutomatedAction);

            var marker = Ring("Move destination", markerMaterial, .5f);
            marker.SetActive(false);
            var cameraObject = new GameObject("RTS Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = .1f; camera.farClipPlane = 300;
            camera.backgroundColor = new Color(.13f, .21f, .24f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.transform.SetPositionAndRotation(new Vector3(0, 17, -19), Quaternion.Euler(42, 0, 0));
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            var controls = new GameObject("Player Controls");
            var input = controls.AddComponent<RtsInput>();
            var selection = controls.AddComponent<SelectionController>();
            selection.Configure(input, camera, marker.transform);
            cameraObject.AddComponent<RtsCamera>().Configure(input, selection);
            controls.AddComponent<WandererHud>().Configure(selection);
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.4f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .60f, .65f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.companyName = "Wonder Gather";
            PlayerSettings.productName = "Wonder Gather - The Wanderer";
            EditorSettings.serializationMode = SerializationMode.ForceText;
            AssetDatabase.SaveAssets();
            Debug.Log("WANDERER_SETUP_OK");
        }

        private static Material Material(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new System.Exception("URP Lit shader is unavailable.");
            var material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, Root + "/Materials/" + name + ".mat");
            return material;
        }
        private static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var result = GameObject.CreatePrimitive(type);
            result.name = name; result.transform.position = position; result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }
        private static GameObject Ring(string name, Material material, float radius)
        {
            var result = new GameObject(name);
            var line = result.AddComponent<LineRenderer>();
            line.sharedMaterial = material; line.useWorldSpace = false; line.loop = true;
            line.widthMultiplier = .055f; line.positionCount = 64;
            for (int i = 0; i < 64; i++)
            {
                float angle = i * Mathf.PI * 2 / 64;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius);
            }
            return result;
        }
    }
}
