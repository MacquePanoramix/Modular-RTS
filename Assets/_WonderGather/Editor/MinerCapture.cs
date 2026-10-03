using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // S1d: the three miners from Art/Blender/Worker/workers.py (Small, Long, Round), seen in the Ordinary
    // Place's own look (E). It builds their painted materials from the manifest the Blender script writes
    // (workers.json), adds the drawn outline, stands them on the path before the lit house (the scene itself
    // is not saved), and renders them at dusk, by day and at night, from the Strategy camera's height, close
    // by each face, and turning around each miner.
    // In the Editor it asks before leaving a modified scene, never saves the miners into the scene, and leaves
    // them standing for a look in the Scene view, with the worker, hour and look put back.
    // Batch use (with graphics):
    //   Unity -batchmode -projectPath <project> -executeMethod WonderGather.Editor.MinerCapture.Capture -captureOut <folder> -quit
    public static class MinerCapture
    {
        public const string Folder = "Assets/_WonderGather/Art/Worker/Miners";
        public const string ModelPath = Folder + "/Workers.fbx";
        private const string MaterialPath = "Assets/_WonderGather/Materials/Miners";

        [Serializable] private class Manifest { public string[] characters; public Entry[] materials; }

        [Serializable]
        private class Entry
        {
            public string name, kind, texture;
            public float[] colour, emission;
            public float gloss;
        }

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        [MenuItem("Wonder Gather/Capture the Miners")]
        public static void Capture()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string folder = Argument("-captureOut") ?? "Captures/Miners";
            Directory.CreateDirectory(folder);
            bool previous = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try
            {
                var model = Import(out var outline, out var manifest);
                EditorSceneManager.OpenScene(OrdinaryPlaceSetup.ScenePath);
                var camera = Camera.main ?? throw new InvalidOperationException("The Ordinary Place has no main camera.");
                var time = Object.FindAnyObjectByType<TimeOfDay>();
                var look = Object.FindAnyObjectByType<LookDevControls>();
                var ground = Object.FindAnyObjectByType<OrdinaryGround>();
                var worker = Object.FindAnyObjectByType<SelectableUnit>();
                float hourBefore = time.Hour;
                int lookBefore = look.CandidateIndex;
                if (worker != null) worker.gameObject.SetActive(false);
                look.Select(4);

                // The three on the path before the door, turned towards each other as if talking: Small in front.
                var step = ground.Path[0];
                var places = new Dictionary<string, (Vector2 Spot, float Yaw)>
                {
                    ["Long"] = (new Vector2(-1.2f, -2.45f), 156f),
                    ["Small"] = (new Vector2(0.05f, -2.15f), 182f),
                    ["Round"] = (new Vector2(1.25f, -2.6f), 210f),
                };
                var figures = new Dictionary<string, Transform>();
                foreach (string character in manifest.characters)
                {
                    var figure = Stand(model, "Worker_" + character, outline);
                    var (spot, yaw) = places.TryGetValue(character, out var place) ? place : (Vector2.zero, 180f);
                    var at = new Vector2(step.x, step.y) + spot;
                    figure.SetPositionAndRotation(new Vector3(at.x, ground.Height(at.x, at.y), at.y), Quaternion.Euler(0, yaw, 0));
                    figures[character] = figure;
                }

                Vector3 Ground(Vector2 xz, float up) => new Vector3(xz.x, ground.Height(xz.x, xz.y) + up, xz.y);
                var stand = new Vector2(step.x, step.y);
                var shots = new List<(string Name, float Hour, Vector3 Eye, Vector3 Target, int Width, int Height, string Alone)>
                {
                    ("lineup_dusk", 19.2f, Ground(stand + new Vector2(0.3f, -7.0f), .95f), Ground(stand + new Vector2(0, -2.2f), 1.0f), 1600, 900, null),
                    ("lineup_day", 10, Ground(stand + new Vector2(0.3f, -7.0f), .95f), Ground(stand + new Vector2(0, -2.2f), 1.0f), 1600, 900, null),
                    ("lineup_night", 23, Ground(stand + new Vector2(0.3f, -7.0f), .95f), Ground(stand + new Vector2(0, -2.2f), 1.0f), 1600, 900, null),
                    ("strategy_dusk", 19.2f, Ground(stand + new Vector2(-4f, -14f), 11f), Ground(stand + new Vector2(0, -2.4f), 0), 1600, 900, null),
                    ("strategy_day", 10, Ground(stand + new Vector2(-4f, -14f), 11f), Ground(stand + new Vector2(0, -2.4f), 0), 1600, 900, null),
                };
                foreach (var pair in figures)
                {
                    var figure = pair.Value;
                    string name = pair.Key.ToLowerInvariant();
                    // A portrait: the face and shoulders, from a little to one side.
                    var head = figure.GetComponentsInChildren<MeshRenderer>().First(x => x.name.EndsWith("_Skin")).bounds;
                    var face = head.center;
                    var eye = face + figure.forward * 1.15f + figure.right * .45f + Vector3.up * .02f;
                    shots.Add(($"portrait_{name}_dusk", 19.2f, eye, face - Vector3.up * .2f, 1200, 900, null));
                    shots.Add(($"portrait_{name}_day", 10, eye, face - Vector3.up * .2f, 1200, 900, null));
                    shots.Add(($"portrait_{name}_night", 23, eye, face - Vector3.up * .2f, 1200, 900, null));
                    // Turning around the miner, by day, the others stepped aside.
                    var middle = figure.position + Vector3.up * head.center.y * .5f - Vector3.up * figure.position.y * .5f;
                    for (int k = 0; k < 4; k++)
                    {
                        var around = Quaternion.AngleAxis(k * 90 + 25, Vector3.up) * figure.forward;
                        shots.Add(($"turn_{name}_{k}", 10, middle + around * 2.6f + Vector3.up * .25f, middle, 900, 1100, pair.Key));
                    }
                }
                foreach (var shot in shots)
                {
                    time.Hour = shot.Hour;
                    foreach (var pair in figures) pair.Value.gameObject.SetActive(shot.Alone == null || shot.Alone == pair.Key);
                    camera.transform.SetPositionAndRotation(shot.Eye, Quaternion.LookRotation(shot.Target - shot.Eye));
                    Render(camera, Path.Combine(folder, shot.Name + ".png"), shot.Width, shot.Height);
                }
                foreach (var pair in figures) pair.Value.gameObject.SetActive(true);
                if (!Application.isBatchMode)
                {
                    time.Hour = hourBefore;
                    look.Select(lookBefore);
                    if (worker != null) worker.gameObject.SetActive(true);
                }
                Debug.Log("MINERS_CAPTURE_OK " + folder);
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation = previous;
            }
        }

        // One miner from the model: its parts, turned to face forward, outlined, with a warm light at each lamp.
        private static Transform Stand(GameObject model, string prefix, Material outline)
        {
            var figure = new GameObject(prefix) { hideFlags = HideFlags.DontSaveInEditor }.transform;
            // The model's front comes in along -Z; turn it so the figure's forward is its face.
            var body = new GameObject("Model") { hideFlags = HideFlags.DontSaveInEditor }.transform;
            body.SetParent(figure, false);
            body.localRotation = Quaternion.Euler(0, 180, 0);
            foreach (var child in model.GetComponentsInChildren<MeshFilter>(true).Where(x => x.name.StartsWith(prefix + "_")))
            {
                var part = Object.Instantiate(child.gameObject, body, false);
                part.name = child.name;
                part.hideFlags = HideFlags.DontSaveInEditor;
                var renderer = part.GetComponent<MeshRenderer>();
                bool glass = renderer.sharedMaterial != null && renderer.sharedMaterial.name == "Glass";
                if (!glass) renderer.sharedMaterials = renderer.sharedMaterials.Append(outline).ToArray();
                if (glass)
                {
                    // A lamp: a small warm light, so light finds the miner who carries it.
                    var light = new GameObject(part.name + " light") { hideFlags = HideFlags.DontSaveInEditor }.AddComponent<Light>();
                    light.transform.SetParent(part.transform, false);
                    // A little below the glass, so the hand that carries it is not burnt white.
                    light.transform.position = renderer.bounds.center + Vector3.down * .06f;
                    light.type = LightType.Point;
                    light.color = new Color(1f, .72f, .42f);
                    light.intensity = .7f;
                    light.range = 2.4f;
                    light.shadows = LightShadows.None;
                }
            }
            return figure;
        }

        // The model, its materials made from the manifest (once, then reused), and the outline.
        private static GameObject Import(out Material outline, out Manifest manifest)
        {
            if (!File.Exists(ModelPath)) throw new FileNotFoundException("Export the miners from Art/Blender/Worker/workers.py first.", ModelPath);
            manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Folder + "/workers.json"));
            Directory.CreateDirectory(MaterialPath);
            AssetDatabase.Refresh();
            var shader = Shader.Find("Wonder Gather/Painted") ?? throw new InvalidOperationException("The painted shader did not compile.");
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            foreach (var entry in manifest.materials)
            {
                var material = MaterialAt(MaterialPath + "/" + entry.name + ".mat", shader);
                var colour = new Color(entry.colour[0], entry.colour[1], entry.colour[2]);
                bool textured = !string.IsNullOrEmpty(entry.texture);
                material.SetTexture("_BaseMap", textured ? Texture(entry.texture) : null);
                material.SetColor("_BaseColor", textured ? Color.white : colour);
                bool skin = entry.kind == "skin";
                // The painting already carries the colour variation; the shader keeps a little, and its brush-broken light.
                material.SetFloat("_Variation", skin ? .1f : textured ? .12f : .3f);
                material.SetFloat("_Brush", skin ? .2f : .3f);
                // A person is smaller than a house: the shader's own brush marks are made finer to match.
                material.SetFloat("_BrushScale", 14);
                material.SetFloat("_Softness", skin ? .45f : .3f);
                material.SetFloat("_Translucency", skin ? .25f : .08f);
                material.SetFloat("_Gloss", entry.gloss);
                material.SetFloat("_VertexColor", 0);
                material.SetColor("_EmissionColor", entry.kind == "glow" ? new Color(entry.emission[0], entry.emission[1], entry.emission[2]) * 3f : Color.black);
                // Drawn, not only modelled: the paint filter leaves beings clear, so their few marks hold.
                material.SetFloat("_Drawn", 1);
                EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), entry.name), material);
            }
            importer.importNormals = ModelImporterNormals.Import;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.bakeAxisConversion = true;
            importer.SaveAndReimport();
            outline = MaterialAt(MaterialPath + "/Outline.mat",
                Shader.Find("Wonder Gather/Outline") ?? throw new InvalidOperationException("The outline shader did not compile."));
            outline.SetFloat("_MaxWidth", .012f);
            outline.SetFloat("_Behind", .035f);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) ?? throw new InvalidOperationException("The miners did not import.");
        }

        private static Material MaterialAt(string path, Shader shader)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }

        private static Texture2D Texture(string file)
        {
            string path = Folder + "/" + file;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path) ?? throw new FileNotFoundException("A painted texture is missing.", path);
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.anisoLevel = 4;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            // Faces are a few thin strokes: a sharper mip keeps them from fading into the skin at a distance,
            // without changing them up close.
            importer.mipMapBias = file.StartsWith("Face_") ? -1.2f : -0.3f;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void Render(Camera camera, string path, int width, int height)
        {
            var target = new RenderTexture(width, height, 24);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            for (int i = 0; i < 4; i++) RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(target);
        }
    }
}
