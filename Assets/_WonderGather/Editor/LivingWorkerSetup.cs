using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // Integrates the body with the existing faction template. Stable blueprint IDs and
    // the earlier non-civilization production recipe deliberately remain unchanged.
    public static class LivingWorkerSetup
    {
        private const string Root = "Assets/_WonderGather";
        public const string PrefabPath = Root + "/Prefabs/LivingWorker.prefab";
        public const string RecipePath = Root + "/Data/LivingWorkerProduction.asset";
        private const string BodyPath = Root + "/Prefabs/LivingBodyBiped.prefab";
        private const string BasePath = Root + "/Prefabs/CivilizationBase.prefab";
        private const string WorkerPath = Root + "/Data/Civilizations/Worker.asset";
        private const string PreviousRecipePath = Root + "/Data/WorkerProduction.asset";
        private const string CargoMaterialPath = Root + "/Materials/LivingWorkerCargo.mat";
        private const string RackMaterialPath = Root + "/Materials/LivingWorkerRack.mat";

        [MenuItem("Wonder Gather/Integrate Living Workers")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play mode before authoring Living Workers.");
            foreach (var path in new[] { PrefabPath, RecipePath, CargoMaterialPath, RackMaterialPath })
                RequireNewAsset(path);
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var bodyPrefab = RequireAsset<GameObject>(BodyPath);
            var basePrefab = RequireAsset<GameObject>(BasePath);
            var worker = RequireAsset<UnitBlueprint>(WorkerPath);
            var previousRecipe = RequireAsset<WorkerProductionDefinition>(PreviousRecipePath);
            var supplyMaterial = RequireAsset<Material>(Root + "/Materials/Supplies.mat");
            if (worker.Production != previousRecipe || worker.Id != "worker")
                throw new InvalidOperationException("Worker template differs from the reviewed baseline; inspect it before integration.");
            if (bodyPrefab.GetComponent<ProceduralBiped>() == null || bodyPrefab.GetComponent<NavMeshAgent>() == null)
                throw new InvalidOperationException("The authored Living Body prefab is incomplete.");
            if (basePrefab.GetComponent<ResourceDepot>() == null || basePrefab.GetComponent<ResourceWorkplace>() != null)
                throw new InvalidOperationException("The starting base is missing its depot or already has authored work positions.");
            var baseCollider = basePrefab.GetComponent<BoxCollider>();
            if (baseCollider == null || !Mathf.Approximately(baseCollider.size.x, 3) || !Mathf.Approximately(baseCollider.size.z, 3))
                throw new InvalidOperationException("The starting base footprint changed; review its delivery positions.");

            var scene = EditorSceneManager.OpenScene(FactionCreatorSetup.MapPath, OpenSceneMode.Single);
            var resources = Object.FindObjectsByType<ResourceNode>();
            var cameras = Object.FindObjectsByType<RtsCamera>();
            if (resources.Length != 1 || cameras.Length != 1 || resources[0].GetComponent<ResourceWorkplace>() != null)
                throw new InvalidOperationException("Expected one unmodified supply node and RTS camera in FactionPlaytest.");
            var originalResourceVisual = resources[0].transform.Find("Supplies");
            if (originalResourceVisual == null || originalResourceVisual.GetComponent<Renderer>() == null)
                throw new InvalidOperationException("The supply placeholder changed; inspect it before replacing its presentation.");

            var cargoMaterial = CreateMaterial(CargoMaterialPath, new Color(.64f, .55f, .38f));
            var rackMaterial = CreateMaterial(RackMaterialPath, new Color(.35f, .31f, .25f));
            var livingWorker = CreateWorker(bodyPrefab, cargoMaterial, rackMaterial);
            var recipe = Object.Instantiate(previousRecipe);
            recipe.name = "LivingWorkerProduction";
            recipe.Configure(livingWorker);
            AssetDatabase.CreateAsset(recipe, RecipePath);

            AuthorDepot(rackMaterial);
            AuthorResource(resources[0], originalResourceVisual, cargoMaterial, rackMaterial, supplyMaterial);
            var cameraData = new SerializedObject(cameras[0]);
            cameraData.FindProperty("terrainAware").boolValue = true;
            cameraData.ApplyModifiedPropertiesWithoutUndo();
            // The existing 0.005 zoom preference is intentionally not reassigned.
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save FactionPlaytest integration.");

            var workerData = new SerializedObject(worker);
            workerData.FindProperty("production").objectReferenceValue = recipe;
            workerData.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("LIVING_WORKER_SETUP_OK");
        }

        public static void BuildWindows()
        {
            RequireAsset<GameObject>(PrefabPath);
            RequireAsset<WorkerProductionDefinition>(RecipePath);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { FactionCreatorSetup.ScenePath, FactionCreatorSetup.MapPath },
                locationPathName = "Builds/WindowsLivingWorker/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Living Worker Windows build failed: " + report.summary.result);
            Debug.Log("LIVING_WORKER_BUILD_OK");
        }

        // Bounded refresh for the new interaction surfaces; leaves references and IDs intact.
        public static void RefreshInteractionColliders()
        {
            var root = PrefabUtility.LoadPrefabContents(BasePath);
            try
            {
                var ports = root.transform.Find("Delivery positions");
                if (ports == null) throw new InvalidOperationException("Living Worker delivery positions are missing.");
                foreach (var renderer in ports.GetComponentsInChildren<MeshRenderer>()) MakePickable(renderer.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, BasePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            var scene = EditorSceneManager.OpenScene(FactionCreatorSetup.MapPath);
            var node = Object.FindAnyObjectByType<ResourceNode>();
            var rack = node.transform.Find("Provisional supply rack");
            if (rack == null) throw new InvalidOperationException("Living Worker supply rack is missing.");
            foreach (var renderer in rack.GetComponentsInChildren<MeshRenderer>()) MakePickable(renderer.gameObject);
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("LIVING_WORKER_INTERACTION_OK");
        }

        private static void MakePickable(GameObject visual)
        {
            if (visual.GetComponent<Collider>() != null) return;
            visual.AddComponent<MeshCollider>().sharedMesh = visual.GetComponent<MeshFilter>().sharedMesh;
        }

        private static GameObject CreateWorker(GameObject source, Material cargoMaterial, Material bindingMaterial)
        {
            var root = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try
            {
                root.name = "Living Worker";
                var agent = root.GetComponent<NavMeshAgent>();
                // Preserve the existing economy unit's 100-percent movement baseline.
                agent.speed = 3.2f;
                agent.acceleration = 10;
                agent.angularSpeed = 280;
                agent.radius = .38f;
                agent.height = 2.2f;
                var gatherer = root.AddComponent<Gatherer>();
                root.AddComponent<Builder>();
                root.AddComponent<ProducedWorker>();
                var bundle = new GameObject("Provisional carried supplies").transform;
                bundle.SetParent(root.transform, false);
                Visual("Supply bundle", PrimitiveType.Cube, bundle, Vector3.zero,
                    new Vector3(.56f, .28f, .32f), cargoMaterial);
                Visual("Binding", PrimitiveType.Cube, bundle, Vector3.zero,
                    new Vector3(.075f, .30f, .34f), bindingMaterial);
                bundle.gameObject.SetActive(false);
                var body = root.GetComponent<ProceduralBiped>();
                body.SetTuning(.8f, .16f, .28f);
                body.ConfigureWork(gatherer, bundle);
                var result = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (result == null) throw new IOException("Could not create the Living Worker prefab.");
                return result;
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void AuthorDepot(Material rackMaterial)
        {
            var root = PrefabUtility.LoadPrefabContents(BasePath);
            try
            {
                var positions = new Transform[8];
                var contacts = new Transform[8];
                var ports = new GameObject("Delivery positions").transform;
                ports.SetParent(root.transform, false);
                // Two positions on each edge avoid the square collider's corners.
                for (int side = 0; side < 4; side++)
                {
                    var orientation = Quaternion.Euler(0, side * 90, 0);
                    for (int along = 0; along < 2; along++)
                    {
                        int index = side * 2 + along;
                        float tangent = along == 0 ? -.75f : .75f;
                        var stand = orientation * new Vector3(tangent, 0, 2.3f);
                        var contact = orientation * new Vector3(tangent, 1.35f, 1.8f);
                        positions[index] = Point("Stand " + (index + 1), ports, stand);
                        contacts[index] = Point("Contact " + (index + 1), ports, contact);
                        var shelf = Visual("Delivery shelf " + (index + 1), PrimitiveType.Cube, ports,
                            orientation * new Vector3(tangent, 1.29f, 1.625f),
                            new Vector3(1.1f, .12f, .45f), rackMaterial, true);
                        shelf.localRotation = orientation;
                    }
                }
                root.AddComponent<ResourceWorkplace>().Configure(positions, contacts);
                if (PrefabUtility.SaveAsPrefabAsset(root, BasePath) == null)
                    throw new IOException("Could not save delivery positions on CivilizationBase.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void AuthorResource(ResourceNode node, Transform originalVisual,
            Material cargoMaterial, Material rackMaterial, Material supplyMaterial)
        {
            // Keep the existing selection collider and ResourceNode identity. The new
            // presentation is purely visual; navigation is reserved by one root obstacle.
            originalVisual.name = "Supply interaction collider";
            originalVisual.GetComponent<Renderer>().enabled = false;
            var presentation = new GameObject("Provisional supply rack").transform;
            presentation.SetParent(node.transform, false);
            Visual("Supply rack", PrimitiveType.Cylinder, presentation, Vector3.up * .425f,
                new Vector3(2.8f, .425f, 2.8f), rackMaterial, true);
            Visual("Supply identification marker", PrimitiveType.Sphere, presentation,
                Vector3.up * 1.65f, Vector3.one * .42f, supplyMaterial, true);
            var positions = new Transform[8];
            var contacts = new Transform[8];
            var ports = new GameObject("Gathering positions").transform;
            ports.SetParent(node.transform, false);
            for (int i = 0; i < 8; i++)
            {
                var orientation = Quaternion.Euler(0, i * 45, 0);
                positions[i] = Point("Stand " + (i + 1), ports, orientation * Vector3.forward * 2.05f);
                contacts[i] = Point("Contact " + (i + 1), ports,
                    orientation * new Vector3(0, 1.35f, 1.55f));
                var supplies = Visual("Collectable supplies " + (i + 1), PrimitiveType.Cube,
                    presentation, orientation * new Vector3(0, 1.10f, 1.325f),
                    new Vector3(.62f, .50f, .45f), cargoMaterial, true);
                supplies.localRotation = orientation;
            }
            var obstacle = node.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.center = Vector3.up * .75f;
            obstacle.radius = 1.35f;
            obstacle.height = 1.5f;
            obstacle.carving = true;
            node.gameObject.AddComponent<ResourceWorkplace>().Configure(positions, contacts);
        }

        private static Transform Point(string name, Transform parent, Vector3 position)
        {
            var result = new GameObject(name).transform;
            result.SetParent(parent, false);
            result.localPosition = position;
            return result;
        }

        private static Transform Visual(string name, PrimitiveType type, Transform parent,
            Vector3 position, Vector3 scale, Material material, bool pickable = false)
        {
            var result = GameObject.CreatePrimitive(type);
            result.name = name;
            Object.DestroyImmediate(result.GetComponent<Collider>());
            result.transform.SetParent(parent, false);
            result.transform.localPosition = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            if (pickable) MakePickable(result);
            return result.transform;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
            var result = new Material(shader) { name = Path.GetFileNameWithoutExtension(path), color = color };
            result.SetFloat("_Smoothness", .12f);
            AssetDatabase.CreateAsset(result, path);
            return result;
        }

        private static T RequireAsset<T>(string path) where T : Object
        {
            var result = AssetDatabase.LoadAssetAtPath<T>(path);
            if (result == null) throw new FileNotFoundException("Required Living Worker source asset is missing.", path);
            return result;
        }

        private static void RequireNewAsset(string path)
        {
            if (File.Exists(path) || AssetDatabase.LoadMainAssetAtPath(path) != null)
                throw new IOException("Refusing to overwrite an existing Living Worker asset: " + path);
        }
    }
}
