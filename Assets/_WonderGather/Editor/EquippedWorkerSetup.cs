using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // One-time, guarded authoring for the equipment slice. The old supply map and
    // its economy stay intact; both maps receive workers from the same blueprint.
    public static class EquippedWorkerSetup
    {
        private const string Root = "Assets/_WonderGather";
        public const string MapPath = Root + "/Scenes/EquipmentPlaytest.unity";
        public const string ToolPath = Root + "/Data/Pickaxe.asset";
        public const string PrefabPath = Root + "/Prefabs/Pickaxe.prefab";
        private const string ExamplePath = Root + "/Data/Civilizations/LittleSettlement.asset";
        private const string WorkerPath = Root + "/Data/Civilizations/Worker.asset";
        private const string WoodPath = Root + "/Materials/PickaxeWood.mat";
        private const string MetalPath = Root + "/Materials/PickaxeMetal.mat";
        private const string RockPath = Root + "/Materials/EquipmentMineral.mat";
        private const string MeshPath = Root + "/Art/EquipmentMineralMesh.asset";

        [MenuItem("Wonder Gather/Create Equipped Worker Slice")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play mode before authoring the Equipped Worker.");
            foreach (var path in new[] { MapPath, ToolPath, PrefabPath, WoodPath, MetalPath, RockPath, MeshPath })
                RequireNewAsset(path);
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var workerPrefab = RequireAsset<GameObject>(LivingWorkerSetup.PrefabPath);
            var worker = RequireAsset<UnitBlueprint>(WorkerPath);
            var example = RequireAsset<CivilizationDefinition>(ExamplePath);
            var recipe = RequireAsset<WorkerProductionDefinition>(LivingWorkerSetup.RecipePath);
            RequireAsset<SceneAsset>(FactionCreatorSetup.ScenePath);
            RequireAsset<SceneAsset>(FactionCreatorSetup.MapPath);
            if (worker.Id != "worker" || worker.Production != recipe || worker.Tool != null || example.Tools.Count != 0)
                throw new InvalidOperationException("The creator template differs from the reviewed equipment baseline.");
            if (workerPrefab.GetComponent<ProceduralBiped>() == null || workerPrefab.GetComponent<Gatherer>() == null ||
                workerPrefab.GetComponent<EquippedTool>() != null || recipe.Prefab != workerPrefab)
                throw new InvalidOperationException("The Living Worker prefab differs from the reviewed equipment baseline.");

            // Inspect the source before creating anything, but never save it.
            EditorSceneManager.OpenScene(FactionCreatorSetup.MapPath, OpenSceneMode.Single);
            var resources = Object.FindObjectsByType<ResourceNode>();
            if (resources.Length != 1 || resources[0].GetComponent<ResourceWorkplace>() == null ||
                resources[0].GetComponent<MineableResource>() != null ||
                resources[0].transform.Find("Provisional supply rack") == null ||
                Object.FindObjectsByType<FactionPlaytestBridge>().Length != 1)
                throw new InvalidOperationException("Expected the existing Living Worker supply map and bridge.");
            if (!AssetDatabase.CopyAsset(FactionCreatorSetup.MapPath, MapPath))
                throw new IOException("Could not copy the existing faction map for the equipment test.");
            var scene = EditorSceneManager.OpenScene(MapPath, OpenSceneMode.Single);

            var wood = CreateMaterial(WoodPath, new Color(.38f, .23f, .12f), .16f);
            var metal = CreateMaterial(MetalPath, new Color(.38f, .43f, .47f), .38f, .65f);
            var rock = CreateMaterial(RockPath, new Color(.37f, .40f, .42f), .08f);
            var toolPrefab = CreatePickaxe(wood, metal);
            var pickaxe = ScriptableObject.CreateInstance<ToolDefinition>();
            pickaxe.name = "Pickaxe";
            pickaxe.Configure("pickaxe", "Pickaxe", toolPrefab,
                new Vector3(0, -.20f, 0), new Vector3(0, .12f, 0), new Vector3(0, .55f, .26f), .055f);
            AssetDatabase.CreateAsset(pickaxe, ToolPath);

            AuthorWorker();
            example.ConfigureTools(pickaxe);
            EditorUtility.SetDirty(example);
            // Equipment is an explicit creative choice. Existing drafts and the
            // template worker continue to mean None until the player selects it.
            var node = Object.FindAnyObjectByType<ResourceNode>();
            AuthorMineral(node, rock);
            if (!EditorSceneManager.SaveScene(scene))
                throw new IOException("Could not save the Equipped Worker test map.");
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existing = scenes.FindIndex(entry => entry.path == MapPath);
            if (existing >= 0) scenes[existing] = new EditorBuildSettingsScene(MapPath, true);
            else scenes.Add(new EditorBuildSettingsScene(MapPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("EQUIPPED_WORKER_SETUP_OK");
        }

        public static void BuildWindows()
        {
            RequireAsset<GameObject>(PrefabPath);
            RequireAsset<ToolDefinition>(ToolPath);
            RequireAsset<SceneAsset>(MapPath);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { FactionCreatorSetup.ScenePath, FactionCreatorSetup.MapPath, MapPath },
                locationPathName = "Builds/WindowsEquippedWorker/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Equipped Worker Windows build failed: " + report.summary.result);
            Debug.Log("EQUIPPED_WORKER_BUILD_OK");
        }

        private static GameObject CreatePickaxe(Material wood, Material metal)
        {
            var root = new GameObject("Provisional pickaxe");
            try
            {
                Visual("Wooden shaft", PrimitiveType.Cylinder, root.transform, Vector3.zero,
                    new Vector3(.065f, .45f, .065f), wood);
                Visual("Head collar", PrimitiveType.Cube, root.transform, new Vector3(0, .47f, 0),
                    new Vector3(.105f, .14f, .115f), metal);
                Visual("Metal head", PrimitiveType.Cube, root.transform, new Vector3(0, .55f, 0),
                    new Vector3(.11f, .12f, .36f), metal);
                Visual("Striking tip", PrimitiveType.Cube, root.transform, new Vector3(0, .55f, .24f),
                    new Vector3(.085f, .10f, .14f), metal);
                Visual("Rear tip", PrimitiveType.Cube, root.transform, new Vector3(0, .55f, -.24f),
                    new Vector3(.085f, .10f, .14f), metal);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (prefab == null) throw new IOException("Could not save the pickaxe prefab.");
                return prefab;
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void AuthorWorker()
        {
            var root = PrefabUtility.LoadPrefabContents(LivingWorkerSetup.PrefabPath);
            try
            {
                root.AddComponent<EquippedTool>();
                if (PrefabUtility.SaveAsPrefabAsset(root, LivingWorkerSetup.PrefabPath) == null)
                    throw new IOException("Could not attach equipment to the Living Worker prefab.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void AuthorMineral(ResourceNode node, Material material)
        {
            // Preserve the ResourceNode and its file ID: CivilizationSession/HUD
            // already reference it. Replace only this copied node's presentation.
            node.name = "Provisional mineral boulder";
            var workplace = node.GetComponent<ResourceWorkplace>();
            for (int i = node.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(node.transform.GetChild(i).gameObject);
            foreach (var collider in node.GetComponents<Collider>()) Object.DestroyImmediate(collider);
            var mesh = CreateBoulderMesh();
            AssetDatabase.CreateAsset(mesh, MeshPath);
            var surface = new GameObject("Mineral surface");
            surface.transform.SetParent(node.transform, false);
            surface.AddComponent<MeshFilter>().sharedMesh = mesh;
            surface.AddComponent<MeshRenderer>().sharedMaterial = material;
            var surfaceCollider = surface.AddComponent<MeshCollider>();
            surfaceCollider.sharedMesh = mesh;
            node.gameObject.AddComponent<MineableResource>().Configure(surfaceCollider);

            var positions = new Transform[8];
            var contacts = new Transform[8];
            var ports = new GameObject("Mining positions").transform;
            ports.SetParent(node.transform, false);
            for (int i = 0; i < positions.Length; i++)
            {
                var orientation = Quaternion.Euler(0, i * 45, 0);
                positions[i] = Point("Stand " + (i + 1), ports, orientation * Vector3.forward * 1.85f);
                contacts[i] = Point("Contact " + (i + 1), ports,
                    orientation * new Vector3(0, 1.25f, 1.05f));
            }
            workplace.Configure(positions, contacts);
            var obstacle = node.GetComponent<NavMeshObstacle>();
            if (obstacle == null) obstacle = node.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.center = Vector3.up * .825f;
            obstacle.radius = 1.1f;
            obstacle.height = 1.65f;
            obstacle.carving = true;
        }

        private static Mesh CreateBoulderMesh()
        {
            // The eight contact markers coincide with this surface at y=1.25.
            // Duplicate face vertices keep the provisional rock faceted. Its
            // renderer and collision use the same mesh, with no invisible shell.
            const int sides = 16;
            float[] heights = { 0, .75f, 1.25f, 1.65f };
            float[] radii = { .92f, 1.10f, 1.05f, .43f };
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            Vector3 Ring(int level, int side)
            {
                float angle = side * Mathf.PI * 2 / sides;
                return new Vector3(Mathf.Sin(angle) * radii[level], heights[level], Mathf.Cos(angle) * radii[level]);
            }
            void Triangle(Vector3 a, Vector3 b, Vector3 c)
            {
                int first = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                triangles.Add(first); triangles.Add(first + 1); triangles.Add(first + 2);
            }
            for (int side = 0; side < sides; side++)
            {
                for (int level = 0; level < heights.Length - 1; level++)
                {
                    var a = Ring(level, side); var b = Ring(level, side + 1);
                    var c = Ring(level + 1, side); var d = Ring(level + 1, side + 1);
                    Triangle(a, b, c); Triangle(b, d, c);
                }
                Triangle(Vector3.zero, Ring(0, side + 1), Ring(0, side));
                Triangle(Vector3.up * heights[3], Ring(3, side), Ring(3, side + 1));
            }
            var mesh = new Mesh { name = "Provisional mineral boulder" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        private static Transform Point(string name, Transform parent, Vector3 position)
        {
            var point = new GameObject(name).transform;
            point.SetParent(parent, false); point.localPosition = position;
            return point;
        }

        private static void Visual(string name, PrimitiveType type, Transform parent,
            Vector3 position, Vector3 scale, Material material)
        {
            var visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position; visual.transform.localScale = scale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material CreateMaterial(string path, Color color, float smoothness, float metallic = 0)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
            var material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path), color = color };
            material.SetFloat("_Smoothness", smoothness); material.SetFloat("_Metallic", metallic);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static T RequireAsset<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new FileNotFoundException("Required Equipped Worker source asset is missing.", path);
            return asset;
        }

        private static void RequireNewAsset(string path)
        {
            if (File.Exists(path) || AssetDatabase.LoadMainAssetAtPath(path) != null)
                throw new IOException("Refusing to overwrite an existing Equipped Worker asset: " + path);
        }
    }
}
