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
    // S1d, first step: the worker concepts from Art/Blender/Worker/worker_concepts.py, seen in the
    // Ordinary Place's own look (E). It imports the concepts with painted materials and a drawn outline,
    // stands them on the path before the lit house (the scene itself is not saved), and renders them
    // at dusk, by day and at night, from the Strategy camera's height, and close by each face.
    // In the Editor it asks before leaving a modified scene, never saves the figures into the scene, and
    // leaves them standing for a look in the Scene view, with the worker, hour and look put back.
    // Batch use (with graphics):
    //   Unity -batchmode -projectPath <project> -executeMethod WonderGather.Editor.WorkerConceptCapture.Capture -captureOut <folder> -quit
    public static class WorkerConceptCapture
    {
        public const string ConceptFolder = "Assets/_WonderGather/Art/Worker/Concepts";
        public const string ConceptPath = ConceptFolder + "/Workers.fbx";
        private const string MaterialPath = "Assets/_WonderGather/Materials/WorkerConcepts";
        private static readonly string[] Concepts = { "Worker_Round", "Worker_Long", "Worker_Small" };

        // The Blender materials' painted equivalents: colour (sRGB), variation, brush, softness, translucency, gloss.
        // Value design: dark hair and coats, mid cloth, light faces and hands.
        private static readonly Dictionary<string, (Color Colour, float Variation, float Brush, float Softness, float Translucency, float Gloss)> Paint =
            new Dictionary<string, (Color, float, float, float, float, float)>
            {
                ["Skin_Round"] = (Color.white, .12f, .2f, .45f, .25f, 0),
                ["Skin_Long"] = (Color.white, .12f, .2f, .45f, .25f, 0),
                ["Skin_Small"] = (Color.white, .12f, .2f, .45f, .25f, 0),
                ["Hair"] = (new Color(.10f, .075f, .075f), .3f, .5f, .3f, 0, .25f),
                ["HairBrown"] = (new Color(.24f, .14f, .09f), .35f, .55f, .3f, 0, .2f),
                ["Smock"] = (new Color(.36f, .42f, .27f), .55f, .7f, .3f, .15f, 0),
                ["Apron"] = (new Color(.70f, .64f, .52f), .55f, .7f, .3f, .15f, 0),
                ["Coat"] = (new Color(.21f, .155f, .13f), .55f, .7f, .3f, .1f, 0),
                ["CoatBlue"] = (new Color(.17f, .20f, .31f), .55f, .7f, .3f, .1f, 0),
                ["Patch"] = (new Color(.40f, .31f, .22f), .55f, .7f, .3f, .1f, 0),
                ["Trousers"] = (new Color(.31f, .26f, .20f), .5f, .6f, .3f, .1f, 0),
                ["Boots"] = (new Color(.17f, .115f, .085f), .4f, .5f, .25f, 0, .2f),
                ["Leather"] = (new Color(.46f, .29f, .17f), .45f, .55f, .25f, 0, .2f),
                ["Accent"] = (new Color(.68f, .26f, .15f), .45f, .55f, .3f, .1f, 0),
                ["Mug"] = (new Color(.84f, .80f, .70f), .3f, .4f, .25f, 0, .3f),
            };

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        [MenuItem("Wonder Gather/Capture Worker Concepts")]
        public static void Capture()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string folder = Argument("-captureOut") ?? "Captures/WorkerConcepts";
            Directory.CreateDirectory(folder);
            bool previous = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try
            {
                var model = Import(out var outline);
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

                // The three on the path before the door, turned towards each other as if talking.
                var step = ground.Path[0];
                var spots = new[] { new Vector2(-1.15f, -2.5f), new Vector2(0.1f, -2.0f), new Vector2(1.25f, -2.6f) };
                var yaws = new[] { 150f, 186f, 212f };
                var figures = new List<Transform>();
                for (int i = 0; i < Concepts.Length; i++)
                {
                    var figure = new GameObject(Concepts[i]) { hideFlags = HideFlags.DontSaveInEditor }.transform;
                    // The model's front comes in along -Z; turn it so the figure's forward is its face.
                    var body = new GameObject("Model") { hideFlags = HideFlags.DontSaveInEditor }.transform;
                    body.SetParent(figure, false);
                    body.localRotation = Quaternion.Euler(0, 180, 0);
                    foreach (var child in model.GetComponentsInChildren<MeshFilter>(true).Where(x => x.name.StartsWith(Concepts[i] + "_")))
                    {
                        var part = Object.Instantiate(child.gameObject, body, false);
                        part.name = child.name;
                        part.hideFlags = HideFlags.DontSaveInEditor;
                        var renderer = part.GetComponent<MeshRenderer>();
                        renderer.sharedMaterials = renderer.sharedMaterials.Append(outline).ToArray();
                    }
                    var at = new Vector2(step.x, step.y) + spots[i];
                    figure.SetPositionAndRotation(new Vector3(at.x, ground.Height(at.x, at.y), at.y), Quaternion.Euler(0, yaws[i], 0));
                    figures.Add(figure);
                }

                Vector3 Ground(Vector2 xz, float up) => new Vector3(xz.x, ground.Height(xz.x, xz.y) + up, xz.y);
                var stand = new Vector2(step.x, step.y);
                var shots = new List<(string Name, float Hour, Vector3 Eye, Vector3 Target, int Width, int Height)>
                {
                    ("lineup_dusk", 19.2f, Ground(stand + new Vector2(0.3f, -7.2f), .95f), Ground(stand + new Vector2(0, -2.2f), 1.0f), 1600, 900),
                    ("lineup_day", 10, Ground(stand + new Vector2(0.3f, -7.2f), .95f), Ground(stand + new Vector2(0, -2.2f), 1.0f), 1600, 900),
                    ("lineup_night", 23, Ground(stand + new Vector2(0.3f, -7.2f), .95f), Ground(stand + new Vector2(0, -2.2f), 1.0f), 1600, 900),
                    ("strategy_dusk", 19.2f, Ground(stand + new Vector2(-4f, -14f), 11f), Ground(stand + new Vector2(0, -2.4f), 0), 1600, 900),
                    ("strategy_day", 10, Ground(stand + new Vector2(-4f, -14f), 11f), Ground(stand + new Vector2(0, -2.4f), 0), 1600, 900),
                };
                for (int i = 0; i < figures.Count; i++)
                {
                    // A portrait: the face and shoulders, from a little to one side.
                    var head = figures[i].GetComponentsInChildren<MeshRenderer>().First(x => x.name.EndsWith("_Skin")).bounds;
                    var face = head.center;
                    var eye = face + figures[i].forward * 1.2f + figures[i].right * .45f + Vector3.up * .02f;
                    string concept = Concepts[i].Replace("Worker_", "").ToLowerInvariant();
                    shots.Add(($"portrait_{concept}_dusk", 19.2f, eye, face - Vector3.up * .22f, 1200, 900));
                    shots.Add(($"portrait_{concept}_day", 10, eye, face - Vector3.up * .22f, 1200, 900));
                }
                foreach (var shot in shots)
                {
                    time.Hour = shot.Hour;
                    camera.transform.SetPositionAndRotation(shot.Eye, Quaternion.LookRotation(shot.Target - shot.Eye));
                    Render(camera, Path.Combine(folder, shot.Name + ".png"), shot.Width, shot.Height);
                }
                if (!Application.isBatchMode)
                {
                    time.Hour = hourBefore;
                    look.Select(lookBefore);
                    if (worker != null) worker.gameObject.SetActive(true);
                }
                Debug.Log("WORKER_CONCEPTS_CAPTURE_OK " + folder);
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation = previous;
            }
        }

        // The concepts' model, its parts mapped onto painted materials (made once, then reused), and the outline.
        private static GameObject Import(out Material outline)
        {
            if (!File.Exists(ConceptPath)) throw new FileNotFoundException("Export the concepts from Art/Blender/Worker/worker_concepts.py first.", ConceptPath);
            Directory.CreateDirectory(MaterialPath);
            AssetDatabase.Refresh();
            var shader = Shader.Find("Wonder Gather/Painted") ?? throw new InvalidOperationException("The painted shader did not compile.");
            var importer = (ModelImporter)AssetImporter.GetAtPath(ConceptPath);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            foreach (var pair in Paint)
            {
                var material = MaterialAt(MaterialPath + "/" + pair.Key + ".mat", shader);
                var (colour, variation, brush, softness, translucency, gloss) = pair.Value;
                material.SetColor("_BaseColor", colour);
                material.SetFloat("_Variation", variation);
                material.SetFloat("_Brush", brush);
                material.SetFloat("_Softness", softness);
                material.SetFloat("_Translucency", translucency);
                material.SetFloat("_Gloss", gloss);
                material.SetFloat("_VertexColor", 0);
                // Drawn, not only modelled: the paint filter leaves beings clear, so their few marks hold.
                material.SetFloat("_Drawn", 1);
                // A face painted by hand: Face_<Concept>.png, written by the Blender script beside the model.
                if (pair.Key.StartsWith("Skin_")) material.SetTexture("_BaseMap", Face(pair.Key.Substring(5)));
                EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), material);
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
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<GameObject>(ConceptPath) ?? throw new InvalidOperationException("The concepts did not import.");
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

        private static Texture2D Face(string concept)
        {
            string path = ConceptFolder + "/Face_" + concept + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path) ?? throw new FileNotFoundException("The face was not painted.", path);
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.anisoLevel = 4;
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
