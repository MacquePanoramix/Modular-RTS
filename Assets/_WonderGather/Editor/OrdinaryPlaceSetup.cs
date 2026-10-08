using System;
using System.Collections.Generic;
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
    // S1b: authors The Ordinary Place, the in-engine Visual Soul look test. A small house,
    // grassland, path and worker, lit by a time-of-day palette, with live-switchable
    // rendering candidates. Guarded: it never overwrites an existing scene or asset.
    public static class OrdinaryPlaceSetup
    {
        private const string Root = "Assets/_WonderGather";
        public const string ArtPath = Root + "/Art/OrdinaryPlace";
        public const string HousePath = ArtPath + "/House.fbx";
        public const string NaturePath = ArtPath + "/Nature.fbx";
        public const string ScenePath = Root + "/Scenes/TheOrdinaryPlace.unity";
        private const string MaterialPath = Root + "/Materials/OrdinaryPlace";
        private const string VolumePath = ArtPath + "/OrdinaryPlaceVolume.asset";
        private const string NavigationPath = Root + "/Scenes/OrdinaryPlaceNavMesh.asset";
        private const string WorkerPath = Root + "/Prefabs/OrdinaryWorker.prefab";
        private const string BodyPath = Root + "/Prefabs/LivingBodyBiped.prefab";
        private static readonly Vector2 HouseCenter = new Vector2(0, 8);

        private static Material Painted(string name, Color color, float variation = .6f, float brush = .6f, float softness = .2f,
            float translucency = 0, float gloss = 0, float sway = 0, bool vertexColour = false, float brushScale = 3)
        {
            string path = MaterialPath + "/" + name + ".mat";
            RequireNew(path);
            var shader = Shader.Find("Wonder Gather/Painted") ?? throw new InvalidOperationException("The painted shader did not compile.");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Variation", variation);
            material.SetFloat("_Brush", brush);
            material.SetFloat("_Softness", softness);
            material.SetFloat("_Translucency", translucency);
            material.SetFloat("_Gloss", gloss);
            material.SetFloat("_Sway", sway);
            material.SetFloat("_VertexColor", vertexColour ? 1 : 0);
            material.SetFloat("_BrushScale", brushScale);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material FromShader(string name, string shaderName, Action<Material> configure)
        {
            string path = MaterialPath + "/" + name + ".mat";
            RequireNew(path);
            var shader = Shader.Find(shaderName) ?? throw new InvalidOperationException(shaderName + " did not compile.");
            var material = new Material(shader) { name = name };
            configure(material);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void RequireNew(string path)
        {
            if (File.Exists(path)) throw new InvalidOperationException(path + " already exists; the Ordinary Place is authored once.");
        }

        [MenuItem("Wonder Gather/Create The Ordinary Place")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play mode first.");
            foreach (var path in new[] { ScenePath, VolumePath, NavigationPath, WorkerPath }) RequireNew(path);
            if (!File.Exists(HousePath)) throw new FileNotFoundException("Export the house from Art/Blender/OrdinaryPlace/house.py first.", HousePath);
            if (!File.Exists(NaturePath)) throw new FileNotFoundException("Export nature from Art/Blender/OrdinaryPlace/nature.py first.", NaturePath);
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(MaterialPath);
            AssetDatabase.Refresh();

            // Materials: base colours only; the painted shader supplies the light.
            var mats = new Dictionary<string, Material>
            {
                ["Plaster"] = Painted("Plaster", new Color(.80f, .70f, .57f), .75f, .8f, .22f),
                ["Interior"] = Painted("Interior", new Color(.86f, .60f, .36f), .5f, .5f, .3f),
                ["Timber"] = Painted("Timber", new Color(.33f, .23f, .15f), .7f, .9f, .2f, brushScale: 5),
                ["Roof"] = Painted("Roof", new Color(.27f, .24f, .24f), .8f, .7f, .2f, gloss: .15f),
                ["Stone"] = Painted("Stone", new Color(.44f, .41f, .37f), .8f, .8f, .25f, gloss: .1f),
                ["Glow"] = Painted("Glow", new Color(.45f, .26f, .12f), .2f, .2f, .3f),
                ["Clay"] = Painted("Clay", new Color(.62f, .35f, .21f), .6f, .6f, .25f),
                ["Foliage"] = Painted("Foliage", new Color(.20f, .33f, .16f), .7f, .7f, .45f, translucency: .5f, sway: .5f),
                ["Cloth"] = Painted("Cloth", new Color(.62f, .12f, .08f), .5f, .5f, .35f, translucency: .35f),
                ["Firewood"] = Painted("Firewood", new Color(.50f, .37f, .24f), .7f, .8f, .2f),
                ["Iron"] = Painted("Iron", new Color(.15f, .14f, .14f), .3f, .4f, .2f, gloss: .35f),
            };
            var groundMaterial = Painted("Ground", Color.white, .8f, .5f, .35f, vertexColour: true, brushScale: 1.5f);
            groundMaterial.SetFloat("_DirectOcclusion", 1);
            var coat = Painted("Worker coat", new Color(.20f, .23f, .17f), .6f, .7f, .3f);
            var trousers = Painted("Worker trousers", new Color(.27f, .21f, .17f), .5f, .6f, .3f);
            var boots = Painted("Worker boots", new Color(.13f, .10f, .08f), .4f, .5f, .25f, gloss: .15f);
            var skin = Painted("Worker skin", new Color(.88f, .67f, .54f), .3f, .3f, .4f, translucency: .2f);
            var hair = Painted("Worker hair", new Color(.08f, .065f, .07f), .3f, .5f, .3f, gloss: .2f);
            var scarf = Painted("Worker scarf", new Color(.62f, .21f, .12f), .5f, .6f, .35f, translucency: .3f);
            var blade = FromShader("Grass blades", "Wonder Gather/Grass", m => { });
            var plume = FromShader("Grass plumes", "Wonder Gather/Grass", m =>
            {
                m.SetColor("_Root", new Color(.06f, .10f, .08f));
                m.SetColor("_Mid", new Color(.30f, .34f, .20f));
                m.SetColor("_Tip", new Color(.74f, .62f, .48f));
                m.SetFloat("_DryAmount", .1f);
                m.SetFloat("_Translucency", .6f);
                m.SetFloat("_Stiffness", .35f);
            });
            var sky = FromShader("Painted sky", "Wonder Gather/Sky", m => { });
            var smoke = FromShader("Chimney smoke", "Wonder Gather/Smoke", m => { });
            var natureMats = new Dictionary<string, Material>
            {
                ["Bark"] = Painted("Bark", new Color(.23f, .19f, .16f), .7f, .9f, .25f, brushScale: 4),
                ["Leaves"] = Painted("Leaves", new Color(.15f, .27f, .14f), .8f, .9f, .5f, translucency: .45f, sway: .3f, brushScale: 1.6f),
                ["LeavesLight"] = Painted("Leaves light", new Color(.28f, .38f, .16f), .8f, .9f, .5f, translucency: .5f, sway: .3f, brushScale: 1.6f),
                ["Rock"] = Painted("Rock", new Color(.27f, .27f, .27f), .8f, .9f, .3f, gloss: .12f, brushScale: 2.5f),
            };

            // The house: map the Blender materials onto the painted ones.
            var importer = (ModelImporter)AssetImporter.GetAtPath(HousePath);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            foreach (var pair in mats) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), pair.Value);
            importer.importNormals = ModelImporterNormals.Import;
            importer.isReadable = true;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.bakeAxisConversion = true;
            importer.SaveAndReimport();
            var houseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(HousePath) ?? throw new InvalidOperationException("The house did not import.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("Walkable World");

            // Land: configure the flat house site first, place the house, then lay the path from its door.
            var land = new GameObject("Ordinary ground");
            land.transform.SetParent(world.transform);
            var ground = land.AddComponent<OrdinaryGround>();
            ground.Configure(new Vector2[0], HouseCenter, new Vector2(8.8f, 8), 0);
            float baseHeight = ground.Height(HouseCenter.x, HouseCenter.y);
            var house = (GameObject)PrefabUtility.InstantiatePrefab(houseAsset);
            house.name = "The lit house";
            house.transform.SetParent(world.transform);
            house.transform.SetPositionAndRotation(new Vector3(HouseCenter.x, baseHeight, HouseCenter.y), Quaternion.identity);
            var doorAnchor = Find(house.transform, "Light_Door");
            // The front faces the meadow and the camera's first view (towards -Z).
            if (doorAnchor.position.z > HouseCenter.y) house.transform.rotation = Quaternion.Euler(0, 180, 0);
            // The front is whichever of the house's own axes points towards its door.
            var toDoor = doorAnchor.position - house.transform.position;
            var front = Vector3.Dot(house.transform.forward, toDoor) > 0 ? house.transform.forward : -house.transform.forward;
            var door = doorAnchor.position;
            var doorstep = new Vector2(door.x, door.z) + new Vector2(front.x, front.z) * 1.6f;
            var route = new List<Vector2> { doorstep };
            foreach (var step in new[] { new Vector2(.3f, -3), new Vector2(.6f, -7), new Vector2(-1.8f, -12.5f), new Vector2(-7.5f, -19),
                                         new Vector2(-17, -25.5f), new Vector2(-31, -30.5f), new Vector2(-48, -35), new Vector2(-70, -39) })
                route.Add(new Vector2(door.x, door.z) + step);
            // The land's meshes are rebuilt from this definition whenever the scene loads.
            ground.Configure(route.ToArray(), HouseCenter, new Vector2(8.8f, 8), 0, groundMaterial);
            ground.Regenerate();

            // House colliders: the Explore camera can come close to every surface but not in.
            foreach (var filter in house.GetComponentsInChildren<MeshFilter>())
            {
                string name = filter.gameObject.name;
                if (name.Contains("Glass") || name.Contains("Curtain")) continue;
                filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                var modifier = filter.gameObject.AddComponent<NavMeshModifier>();
                modifier.overrideArea = true;
                modifier.area = 1;
            }
            var blocker = Find(house.transform, "Blocker_Door").gameObject.AddComponent<BoxCollider>();
            blocker.size = new Vector3(1.1f, 2.0f, .3f);
            PlaceNature(world.transform, ground, natureMats, front);
            var smokeAnchor = Find(house.transform, "Smoke_Chimney");
            smokeAnchor.gameObject.AddComponent<SmokePlume>().Configure(smoke);

            // Warm lights at the Blender anchors.
            var down = Vector3.down;
            var warm = new Color(1f, .62f, .32f);
            var lights = new List<Light>
            {
                AddLight(house, "Light_Interior", LightType.Point, warm, 5, 4.5f, LightShadows.Soft, Vector3.forward),
                AddLight(house, "Light_Door", LightType.Spot, warm, 14, 10, LightShadows.Soft, (front * .75f + down * .65f).normalized, 115, 45),
                AddLight(house, "Light_BigWindow", LightType.Spot, warm, 6, 7, LightShadows.Soft, (front * .8f + down * .55f).normalized, 125, 60),
                AddLight(house, "Light_SmallWindow", LightType.Spot, warm, 3.5f, 5, LightShadows.Soft, (front * .8f + down * .55f).normalized, 115, 55),
                AddLight(house, "Light_SideWindow", LightType.Spot, warm, 3, 5, LightShadows.None, (Vector3.Cross(Vector3.up, front) * -.8f + down * .5f).normalized, 115, 55),
                AddLight(house, "Light_Lantern", LightType.Point, new Color(1f, .7f, .38f), 2.5f, 3.5f, LightShadows.None, Vector3.forward),
            };
            var sunObject = new GameObject("Sun and moon");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;

            var look = new GameObject("Look");
            var time = look.AddComponent<TimeOfDay>();
            time.Configure(sun, sky, mats["Glow"], lights.ToArray());
            var grass = new GameObject("Grass").AddComponent<GrassField>();
            grass.Configure(ground, blade, plume);
            var post = look.AddComponent<LookPostEffects>();
            post.Configure(Shader.Find("Hidden/Wonder Gather/Look Post") ?? throw new InvalidOperationException("The look post shader did not compile."));
            var controlsLook = look.AddComponent<LookDevControls>();
            controlsLook.Configure(time, post, grass);
            look.AddComponent<LookBenchmark>().Configure(controlsLook, time);
            time.Hour = 19.2f;
            RenderSettings.skybox = sky;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumePath);
            var bloom = profile.Add<Bloom>(true);
            // Local glow around windows; a wide scatter would wash warm haze over the night sky.
            bloom.intensity.Override(.35f);
            bloom.threshold.Override(1.1f);
            bloom.scatter.Override(.5f);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.Neutral);
            var grade = profile.Add<ColorAdjustments>(true);
            grade.contrast.Override(6);
            grade.saturation.Override(8);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(.16f);
            vignette.smoothness.Override(.5f);
            foreach (var component in profile.components)
            {
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);
            var volume = new GameObject("Look volume").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            // Navigation over the meadow; the house and fence are obstacles.
            var surface = world.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            if (surface.navMeshData == null) throw new InvalidOperationException("Navigation bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, NavigationPath);

            // Camera and controls: both camera modes.
            var marker = new GameObject("Move destination");
            marker.SetActive(false);
            var cameraObject = new GameObject("RTS Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 4000;
            camera.fieldOfView = 50;
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.transform.SetPositionAndRotation(new Vector3(-3, 12, -12), Quaternion.Euler(36, 10, 0));
            cameraObject.AddComponent<AudioListener>();
            var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
            var controls = new GameObject("Player Controls");
            var input = controls.AddComponent<RtsInput>();
            var selection = controls.AddComponent<SelectionController>();
            selection.Configure(input, camera, marker.transform);
            var rig = cameraObject.AddComponent<RtsCamera>();
            rig.Configure(input, selection);
            var rigData = new SerializedObject(rig);
            rigData.FindProperty("terrainAware").boolValue = true;
            rigData.FindProperty("zoomSensitivity").floatValue = .005f;
            rigData.FindProperty("boundary").floatValue = 48;
            rigData.FindProperty("minDistance").floatValue = 4;
            rigData.ApplyModifiedPropertiesWithoutUndo();

            // The worker: today's procedural body, dressed for the look test (costume remains open).
            var workerPrefab = CreateWorker(coat, trousers, boots, skin, hair, scarf);
            var worker = (GameObject)PrefabUtility.InstantiatePrefab(workerPrefab);
            var spawn = route[2];
            worker.transform.position = new Vector3(spawn.x + 1.4f, ground.Height(spawn.x + 1.4f, spawn.y), spawn.y);
            worker.transform.rotation = Quaternion.Euler(0, 20, 0);
            selection.ConfigureUnits(new[] { worker.GetComponent<SelectableUnit>() });

            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save " + ScenePath);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("ORDINARY_PLACE_SETUP_OK grass " + grass.BladeCount);
        }

        // Trees, bushes and stones: a large tree behind the house as in A, a far tree line for
        // depth, and boulders in the grass, all solid to the Explore camera.
        private static void PlaceNature(Transform world, OrdinaryGround ground, Dictionary<string, Material> materials, Vector3 front)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(NaturePath);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            foreach (var pair in materials) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), pair.Value);
            importer.importNormals = ModelImporterNormals.Import;
            importer.isReadable = true;
            importer.animationType = ModelImporterAnimationType.None;
            importer.bakeAxisConversion = true;
            importer.SaveAndReimport();
            var meshes = new Dictionary<string, Mesh>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(NaturePath)) if (asset is Mesh mesh) meshes[mesh.name] = mesh;
            var root = new GameObject("Nature").transform;
            root.SetParent(world, false);
            var behind = -front;
            var side = Vector3.Cross(Vector3.up, front);
            var house = new Vector3(HouseCenter.x, 0, HouseCenter.y);
            var random = new System.Random(5);
            float R(float a, float b) => a + (float)random.NextDouble() * (b - a);

            void Tree(string kind, Vector3 at, float scale)
            {
                var tree = new GameObject(kind).transform;
                tree.SetParent(root, false);
                tree.position = new Vector3(at.x, ground.Height(at.x, at.z) - .08f, at.z);
                tree.rotation = Quaternion.Euler(0, R(0, 360), 0);
                tree.localScale = Vector3.one * scale;
                foreach (var part in new[] { "_Wood", "_Leaves" }) Piece(tree, kind + part, meshes, materials);
            }

            Tree("Tree_Large", house + behind * 6.5f + side * 5.5f, 1);
            Tree("Tree_Small_A", house + behind * 9 - side * 9, .9f);
            Tree("Tree_Small_B", house - side * 13 + front * 2, 1);
            for (int i = 0; i < 16; i++)
            {
                float angle = R(-1.4f, 1.4f);
                var direction = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0) * behind;
                Tree(i % 2 == 0 ? "Tree_Small_A" : "Tree_Small_B", house + direction * R(32, 52), R(.8f, 1.3f));
            }
            for (int i = 0; i < 6; i++)
            {
                var at = house + behind * R(-1, 4) + side * (i < 3 ? R(6, 11) : -R(6, 12));
                var bush = new GameObject("Bush").transform;
                bush.SetParent(root, false);
                bush.position = new Vector3(at.x, ground.Height(at.x, at.z) - .1f, at.z);
                bush.rotation = Quaternion.Euler(0, R(0, 360), 0);
                bush.localScale = Vector3.one * R(.8f, 1.3f);
                Piece(bush, i % 2 == 0 ? "Bush_A" : "Bush_B", meshes, materials);
            }
            int placed = 0;
            for (int attempt = 0; attempt < 200 && placed < 10; attempt++)
            {
                var at = house + new Vector3(R(-28, 28), 0, R(-26, 4));
                if (ground.PathDistance(at.x, at.z) < 2.5f || ground.FlatWeight(at.x, at.z) > .1f) continue;
                var rock = new GameObject("Boulder").transform;
                rock.SetParent(root, false);
                float scale = R(.8f, 1.8f);
                rock.position = new Vector3(at.x, ground.Height(at.x, at.z) - .12f * scale, at.z);
                rock.rotation = Quaternion.Euler(0, R(0, 360), 0);
                rock.localScale = Vector3.one * scale;
                Piece(rock, "Rock_" + (placed % 4 + 1), meshes, materials);
                placed++;
            }
        }

        private static void Piece(Transform parent, string meshName, Dictionary<string, Mesh> meshes, Dictionary<string, Material> materials)
        {
            if (!meshes.TryGetValue(meshName, out var mesh)) throw new InvalidOperationException("Nature.fbx has no " + meshName + ".");
            var piece = new GameObject(meshName);
            piece.transform.SetParent(parent, false);
            piece.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = piece.AddComponent<MeshRenderer>();
            var slots = new Material[mesh.subMeshCount];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = meshName.Contains("Leaves") || meshName.StartsWith("Bush") ? (i == 1 ? materials["LeavesLight"] : materials["Leaves"])
                         : meshName.Contains("Wood") ? materials["Bark"] : materials["Rock"];
            renderer.sharedMaterials = slots;
            piece.AddComponent<MeshCollider>().sharedMesh = mesh;
            var modifier = piece.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.area = 1;
        }

        private static Transform Find(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child;
            throw new InvalidOperationException("The house has no " + name + " anchor; re-export it from house.py.");
        }

        // S1c, second pass: hand-painted textures from Art/Blender/OrdinaryPlace (painting.py) are applied to
        // the existing materials in place, so asset GUIDs and scene references stay the same. Look E
        // (painted light + paint filter + ink), Luis's favourite, becomes the scene's starting look.
        public const string TexturePath = ArtPath + "/Textures";
        private static readonly Dictionary<string, string> PaintedMaterials = new Dictionary<string, string>
        {
            ["Plaster"] = "Plaster", ["Interior"] = "Interior", ["Timber"] = "Timber", ["Roof"] = "Roof", ["Stone"] = "Stone",
            ["Clay"] = "Clay", ["Foliage"] = "Foliage", ["Cloth"] = "Cloth", ["Firewood"] = "Firewood", ["Iron"] = "Iron",
            ["Bark"] = "Bark", ["Leaves"] = "Leaves", ["LeavesLight"] = "Leaves light", ["Rock"] = "Rock",
        };

        [MenuItem("Wonder Gather/Apply Ordinary Place Painted Textures")]
        public static void ApplyPaintedTextures()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play mode first.");
            if (!AssetDatabase.IsValidFolder(TexturePath)) throw new DirectoryNotFoundException("Paint the models first: " + TexturePath);
            int applied = 0;
            foreach (var pair in PaintedMaterials)
            {
                string texturePath = TexturePath + "/" + pair.Key + ".jpg";
                var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer == null) throw new FileNotFoundException("Missing painted texture.", texturePath);
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.anisoLevel = 4;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
                var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath + "/" + pair.Value + ".mat")
                               ?? throw new FileNotFoundException("Missing material.", pair.Value);
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
                material.SetColor("_BaseColor", Color.white);
                // The painting already carries the colour variation; the shader keeps a little, and its brush-broken light.
                material.SetFloat("_Variation", .25f);
                // Wood takes a calmer brush on its light edge, so lamplight reads as grain rather than dirt.
                if (pair.Key == "Timber" || pair.Key == "Firewood" || pair.Key == "Bark") material.SetFloat("_Brush", .3f);
                EditorUtility.SetDirty(material);
                applied++;
            }
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            // The window glow is a fixed emission; TimeOfDay scales it with a global instead of rewriting the asset.
            var time = Object.FindAnyObjectByType<TimeOfDay>() ?? throw new InvalidOperationException("The scene has no time of day.");
            var glow = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath + "/Glow.mat");
            glow.SetColor("_EmissionColor", time.GlowEmission);
            EditorUtility.SetDirty(glow);
            var look = Object.FindAnyObjectByType<LookDevControls>() ?? throw new InvalidOperationException("The scene has no look controls.");
            var lookData = new SerializedObject(look);
            lookData.FindProperty("candidate").intValue = 4;
            lookData.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("ORDINARY_PLACE_PAINTED_OK " + applied);
        }

        // The dusk details over the hand-painted look, each switchable in Play (keys 7, 8, 9):
        // fireflies, the hearth's flicker and the window glow, all on since Luis tried them.
        // Updates the existing scene in place and can run again.
        [MenuItem("Wonder Gather/Add The Dusk Details To The Ordinary Place")]
        public static void AddDuskDetails()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play mode first.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var time = Object.FindAnyObjectByType<TimeOfDay>() ?? throw new InvalidOperationException("The scene has no time of day.");
            var look = Object.FindAnyObjectByType<LookDevControls>() ?? throw new InvalidOperationException("The scene has no look controls.");
            var house = GameObject.Find("The lit house") ?? throw new InvalidOperationException("The scene has no house.");

            var flies = time.GetComponent<Fireflies>() ?? time.gameObject.AddComponent<Fireflies>();
            // Each has its own home in the meadow (one to every 20 m2 of grass): fewer than there were,
            // twice (Luis, October 2 and 8), and no longer a patch that follows the camera.
            flies.Configure(MaterialFor("Fireflies", "Wonder Gather/Fireflies"), time, UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>());
            flies.enabled = true;

            var windows = house.GetComponentsInChildren<Light>(true)
                .Where(x => x.name.StartsWith("Door") || x.name.Contains("Window")).ToList();
            if (windows.Count < 3) throw new InvalidOperationException("The house's window and door lights were not found.");
            var glow = time.GetComponent<WindowGlow>() ?? time.gameObject.AddComponent<WindowGlow>();
            glow.Configure(MaterialFor("Window glow", "Wonder Gather/Window Glow"), windows);
            glow.enabled = true;

            var hearth = new SerializedObject(time);
            hearth.FindProperty("hearth").boolValue = true;
            hearth.ApplyModifiedPropertiesWithoutUndo();
            look.ConfigureDusk(flies, glow);
            foreach (var item in new Object[] { flies, glow, time, look }) EditorUtility.SetDirty(item);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"ORDINARY_PLACE_DUSK_OK windows {windows.Count}");
        }

        private static Material MaterialFor(string name, string shaderName)
        {
            string path = MaterialPath + "/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var shader = Shader.Find(shaderName) ?? throw new InvalidOperationException(shaderName + " did not compile.");
            var material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        // A release build, so frame times in the benchmark are representative. -buildOut names
        // the folder under Builds/ (default WindowsOrdinaryPlace).
        public static void BuildWindows()
        {
            PlayerSettings.enableFrameTimingStats = true;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/" + (Argument("-buildOut") ?? "WindowsOrdinaryPlace") + "/WonderGather.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Ordinary Place Windows build failed: " + report.summary.result);
            Debug.Log("ORDINARY_PLACE_BUILD_OK");
        }

        private static Light AddLight(GameObject house, string anchor, LightType type, Color color, float intensity, float range,
            LightShadows shadows, Vector3 direction, float angle = 90, float inner = 40)
        {
            var light = new GameObject(anchor.Replace("Light_", "") + " light").AddComponent<Light>();
            light.transform.SetParent(Find(house.transform, anchor), false);
            light.transform.rotation = Quaternion.LookRotation(direction);
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows;
            if (type == LightType.Spot)
            {
                light.spotAngle = angle;
                light.innerSpotAngle = inner;
            }
            return light;
        }

        private static GameObject CreateWorker(Material coat, Material trousers, Material boots, Material skin, Material hair, Material scarf)
        {
            var body = AssetDatabase.LoadAssetAtPath<GameObject>(BodyPath) ?? throw new FileNotFoundException(BodyPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(body);
            instance.name = "Ordinary worker";
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                if (name.Contains("Torso") || name.Contains("upper arm") || name.Contains("forearm")) renderer.sharedMaterial = coat;
                else if (name.Contains("Pelvis") || name.Contains("thigh") || name.Contains("shin")) renderer.sharedMaterial = trousers;
                else if (name.Contains("foot") || name.Contains("toe")) renderer.sharedMaterial = boots;
                else if (name.Contains("Head")) renderer.sharedMaterial = skin;
            }
            var head = FindChild(instance.transform, "Head");
            Accessory("Hair", PrimitiveType.Sphere, head, new Vector3(0, .16f, -.07f), new Vector3(1.1f, .82f, 1.12f), hair);
            var torso = FindChild(instance.transform, "Torso");
            Accessory("Scarf", PrimitiveType.Cylinder, torso, new Vector3(0, .5f, .02f), new Vector3(.62f, .2f, .95f), scarf);
            Accessory("Scarf tail", PrimitiveType.Cube, torso, new Vector3(-.16f, -.2f, .53f), new Vector3(.22f, 1.45f, .1f), scarf).localRotation = Quaternion.Euler(-6, 0, 8);
            var agent = instance.GetComponent<NavMeshAgent>();
            agent.speed = 1.8f;
            agent.acceleration = 4;
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, WorkerPath);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child;
            throw new InvalidOperationException("The body has no " + name + ".");
        }

        private static Transform Accessory(string name, PrimitiveType shape, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var piece = GameObject.CreatePrimitive(shape);
            piece.name = name;
            Object.DestroyImmediate(piece.GetComponent<Collider>());
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = position;
            piece.transform.localScale = scale;
            piece.GetComponent<MeshRenderer>().sharedMaterial = material;
            return piece.transform;
        }
    }
}
