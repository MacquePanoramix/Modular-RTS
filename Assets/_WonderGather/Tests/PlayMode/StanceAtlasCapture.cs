using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The stances of the getting up for any body (layer 1; Luis, November 3, 2026), fitted to a body by the stance solver
    // (Scripts/Units/Rising/AnyBody) and seen from its four sides as the body's own solids: the capsules, the head's sphere
    // and the boxes of its boots and toes that the fit was made with, its supports marked, on level ground with a 10 cm
    // grid. A first look (Docs/Technical/AnyBody/INTEGRATION.md, its check C4): the three miners as the solver has them
    // (BodyFactory), before the bridge from a made miner and before the drawn miner. Nothing of the game is used or changed.
    // Run on its own: -testFilter WonderGather.Tests.StanceAtlasCapture [-atlasOut <folder>] [-atlasBodies Small,Long]
    // [-atlasStances stand,quad]; then Docs/Technical/AnyBody/sheets/atlas_sheets.py <folder>.
    [Explicit]
    public sealed class StanceAtlasCapture
    {
        private const int Tile = 512;
        private const float FieldOfView = 20, Elevation = 15;

        // (key, family, arguments): the first look's stances, from the prototype's batch.
        private static readonly (string Key, string Family, object[] Args)[] FirstLook =
        {
            ("stand", "stand", new object[0]),
            ("quad", "quad", new object[0]),
            ("bear_0.5", "bear", new object[] { 0.5 }),
            ("squat_0.35", "squat", new object[] { 0.35 }),
            ("squat_0.4_toes", "squat", new object[] { 0.4, true }),
            ("tallkneel", "tallkneel", new object[0]),
            ("halfkneel_L", "halfkneel", new object[] { "L", "none" }),
            ("halfkneel_up_0.7_L", "halfkneel_up", new object[] { 0.7, "L", "none" }),
            ("situp_25_hands", "situp", new object[] { 25.0, true }),
        };

        // Which side of the body faces the camera, and where the camera then is (Unity; the fits keep the body's yaw at
        // 0, so it faces +z and its right is +x).
        private static readonly (string Name, Vector3 From)[] Views =
        {
            ("front", Vector3.forward), ("right", Vector3.right), ("back", Vector3.back), ("left", Vector3.left),
        };

        // The fit as the prototype makes it, and with the free limbs relaxed.
        private static readonly string[] Looks = { "fitted", "relaxed" };

        private readonly List<GameObject> made = new List<GameObject>();

        [TearDown]
        public void End()
        {
            foreach (var g in made) if (g != null) UnityEngine.Object.Destroy(g);
            made.Clear();
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator Capture()
        {
            string folder = CaptureTools.Argument("-atlasOut") ?? Path.Combine("Captures", "Atlas", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            string tiles = Path.Combine(folder, "tiles");
            Directory.CreateDirectory(tiles);
            // The bodies: the three miners by name, "made" for the ten made bodies (BodyFrame.MadeBodies), or those by name.
            var madeBodies = BodyFrame.MadeBodies(AnyBody.BodyFactory.Miner);
            var bodies = new List<(string Key, AnyBody.BodyModel Body)>();
            foreach (string name in (CaptureTools.Argument("-atlasBodies") ?? "Small,Long,Round").Split(','))
            {
                string n = name.Trim();
                if (n == "made") foreach (var m in madeBodies) bodies.Add((Key(m.Name), m.Body));
                else
                {
                    var found = madeBodies.Find(m => Key(m.Name) == Key(n));
                    bodies.Add((Key(n), found.Body ?? AnyBody.BodyFactory.Make(n)));
                }
            }
            var lookNames = (CaptureTools.Argument("-atlasLooks") ?? string.Join(",", Looks)).Split(',');
            var only = CaptureTools.Argument("-atlasStances");
            var stances = new List<(string Key, string Family, object[] Args)>();
            foreach (var s in FirstLook) if (only == null || Array.IndexOf(only.Split(','), s.Key) >= 0) stances.Add(s);

            var camera = Studio(out var light);
            var csv = new StringBuilder("body,stance,look,name,cost,status,S,held,worst,base_mm,contact_err_mm,penetration_mm,fit_ms,starts\n");
            var log = new StringBuilder();
            var solver = new AnyBody.ReserveSolver();
            foreach (var (body, b) in bodies)
            {
                // One scale for all of a body's stances: its standing height, times 1.25, fills the frame.
                var standing = AnyBody.StanceFitter.Fit(b, AnyBody.StanceLibrary.Make(b, "stand"));
                float height = (float)Highest(b, standing.Q);
                float distance = .5f * 1.25f * height / Mathf.Tan(.5f * FieldOfView * Mathf.Deg2Rad);
                foreach (var s in stances)
                foreach (var look in lookNames)
                {
                    var st = AnyBody.StanceLibrary.Make(b, s.Family, s.Args);
                    // As fitted; and the free limbs relaxed (FitOptions.RelaxFreeLimbs), side by side for the judges and Luis.
                    var fit = AnyBody.StanceFitter.Fit(b, st, new AnyBody.FitOptions { RelaxFreeLimbs = look == "relaxed" });
                    var report = AnyBody.Statics.Report(b, st, fit.Q);
                    var problem = AnyBody.Statics.Build(b, fit.Q, st.Supports);
                    var reserve = solver.Solve(problem);
                    var worst = new List<string>();
                    if (reserve.Held && reserve.PerJointLp != null)
                        for (int j = 0; j < problem.Joints.Length; j++) if (reserve.PerJointLp[j] >= reserve.S - .005) worst.Add(problem.Joints[j].Name);
                    csv.Append(string.Join(",", body, s.Key, look, Quote(st.Name), F(fit.Cost), fit.Status, reserve.Held ? F(reserve.S) : "inf", reserve.Held ? "1" : "0",
                        Quote(string.Join(" ", worst)), F(1000 * report.BaseMargin), F(report.ContactErrMm), F(report.PenetrationMm), F(fit.Millis), fit.Starts.ToString(CultureInfo.InvariantCulture))).Append('\n');
                    log.AppendLine($"ATLAS {body} {s.Key} {look}: {st.Name}; cost {fit.Cost:F4} ({fit.Status}, {fit.Starts} starts, {fit.Millis:F0} ms); S {(reserve.Held ? reserve.S.ToString("F3", CultureInfo.InvariantCulture) : "not held")} ({string.Join(" ", worst)}); base {1000 * report.BaseMargin:F0} mm; contact {report.ContactErrMm:F1} mm; into the ground {report.PenetrationMm:F1} mm");

                    var solids = Solids(b, fit.Q, problem);
                    Vector3 centre = Centre(solids);
                    foreach (var view in Views)
                    {
                        Vector3 from = Quaternion.AngleAxis(-Elevation, Vector3.Cross(Vector3.up, view.From)) * view.From;
                        camera.transform.position = centre + from * distance;
                        camera.transform.rotation = Quaternion.LookRotation(-from, Vector3.up);
                        // The sun 40 degrees to the camera's left and 50 above the ground, the same on every side: its
                        // light goes the camera's way, turned down 50 degrees and round 40 towards the camera's right.
                        Vector3 looks = -from;
                        light.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(40, Vector3.up) * Quaternion.AngleAxis(50, Vector3.Cross(Vector3.up, looks)) * looks, Vector3.up);
                        yield return null;
                        CaptureTools.Render(camera, Path.Combine(tiles, $"{body}_{s.Key}_{look}_{view.Name}"), Tile, Tile);
                    }
                    UnityEngine.Object.Destroy(solids);
                    yield return null;
                }
            }
            // The line-up (-atlasLineup 1): every body standing, side by side at one scale, from its front and its left.
            int lineup = 0;
            if (CaptureTools.Argument("-atlasLineup") == "1")
            {
                var stood = new List<GameObject>();
                float x = 0, tallest = 0;
                foreach (var (body, b) in bodies)
                {
                    var st = AnyBody.StanceLibrary.Make(b, "stand");
                    var fit = AnyBody.StanceFitter.Fit(b, st);
                    var solids = Solids(b, fit.Q, AnyBody.Statics.Build(b, fit.Q, st.Supports));
                    float width = Mathf.Max(.5f, 2.2f * (float)(b.HipsRadius + b.HipHalfWidth));
                    solids.transform.position = new Vector3(x + .5f * width, 0, 0);
                    x += width + .15f;
                    tallest = Mathf.Max(tallest, (float)Highest(b, fit.Q));
                    stood.Add(solids);
                }
                var centre = new Vector3(.5f * (x - .15f), .5f * tallest, 0);
                // (The ground under the whole row.)
                var ground = GameObject.Find("Atlas ground");
                if (ground != null)
                {
                    ground.transform.position = new Vector3(centre.x, -.001f, 0);
                    ground.transform.localScale = new Vector3(x + 4, .001f, x + 4);
                }
                // (Framed so that the row and its tallest body fit a picture three times as wide as it is high.)
                float half = Mathf.Tan(.5f * FieldOfView * Mathf.Deg2Rad);
                float distance = Mathf.Max(.5f * 1.25f * tallest / half, .5f * 1.05f * x / (3 * half));
                foreach (var (name, turn) in new[] { ("front", 0f), ("three_quarter", -35f) })
                {
                    Vector3 side = Quaternion.AngleAxis(turn, Vector3.up) * Vector3.forward;
                    Vector3 from = Quaternion.AngleAxis(-8, Vector3.Cross(Vector3.up, side)) * side;
                    camera.transform.position = centre + from * distance;
                    camera.transform.rotation = Quaternion.LookRotation(-from, Vector3.up);
                    Vector3 looking = -from;
                    light.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(40, Vector3.up) * Quaternion.AngleAxis(50, Vector3.Cross(Vector3.up, looking)) * looking, Vector3.up);
                    yield return null;
                    CaptureTools.Render(camera, Path.Combine(folder, "lineup_" + name), 3 * Tile, Tile);
                    // (Where each body stands in the picture, for its name under it: the share of the width from the left.)
                    camera.aspect = 3;
                    var places = new StringBuilder("body,u\n");
                    for (int i = 0; i < stood.Count; i++)
                        places.Append(bodies[i].Key).Append(',').Append(F(camera.WorldToViewportPoint(stood[i].transform.position).x)).Append('\n');
                    camera.ResetAspect();
                    File.WriteAllText(Path.Combine(folder, "lineup_" + name + ".csv"), places.ToString());
                    lineup++;
                }
                foreach (var g in stood) UnityEngine.Object.Destroy(g);
            }
            File.WriteAllText(Path.Combine(folder, "figures.csv"), csv.ToString());
            File.WriteAllText(Path.Combine(folder, "log.txt"), log.ToString());
            Debug.Log(log.ToString());
            Assert.That(Directory.GetFiles(tiles, "*.ppm").Length, Is.EqualTo(bodies.Count * stances.Count * lookNames.Length * Views.Length), "A tile was not written.");
        }

        // A body's name as a file's (no spaces or commas).
        private static string Key(string name) => name.Replace(", ", "_").Replace(" ", "_").Replace(",", "_");

        // The stage: a level ground with a faint 10 cm grid, a sun, a soft sky and a camera of little perspective.
        private Camera Studio(out Light light)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            UnityEngine.Object.Destroy(ground.GetComponent<Collider>());
            ground.name = "Atlas ground";
            ground.transform.localScale = new Vector3(6, .001f, 6);
            ground.transform.position = new Vector3(0, -.001f, 0);
            var earth = Clay(new Color(.62f, .55f, .45f));
            earth.mainTexture = Grid();
            earth.mainTextureScale = new Vector2(60, 60);
            if (earth.HasProperty("_BaseMap")) { earth.SetTexture("_BaseMap", earth.mainTexture); earth.SetTextureScale("_BaseMap", new Vector2(60, 60)); }
            ground.GetComponent<Renderer>().sharedMaterial = earth;
            made.Add(ground);

            var sun = new GameObject("Atlas sun");
            light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.color = new Color(1, .96f, .9f);
            light.shadows = LightShadows.Soft;
            made.Add(sun);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.45f, .47f, .52f);

            var eye = new GameObject("Atlas camera");
            var camera = eye.AddComponent<Camera>();
            camera.fieldOfView = FieldOfView;
            camera.nearClipPlane = .05f; camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.80f, .84f, .88f);
            made.Add(eye);
            return camera;
        }

        // The body's solids at the fitted pose, as the fit has them: 10 capsules (a cylinder and two spheres each, so the
        // ends keep their roundness), the head's sphere, and the boxes of the boots and toes; each contact a small dot, by
        // kind; the centre of mass a dark dot with its plumb line.
        private GameObject Solids(AnyBody.BodyModel b, double[] q, AnyBody.StaticsProblem problem)
        {
            var root = new GameObject("Atlas body");
            made.Add(root);
            var k = Kin(b, q);
            var skin = Clay(new Color(.86f, .78f, .68f));
            var bearing = Clay(new Color(.78f, .62f, .48f));
            int bears = 0;
            foreach (var c in problem.Contacts) if (c.OnGround) bears |= 1 << c.Link;
            for (int c = 0; c < 10; c++)
                Capsule(root.transform, U(k.CapA[c]), U(k.CapC[c]), (float)k.CapR[c], (bears & (1 << k.CapLink[c])) != 0 ? bearing : skin);
            Sphere(root.transform, U(k[AnyBody.Pt.HeadC]), (float)b.HeadRadius, (bears & (1 << AnyBody.BodyLinks.Head)) != 0 ? bearing : skin);
            // (A mark on the head's front, so that the side it faces can be read: not a solid of the fit.)
            Sphere(root.transform, U(k[AnyBody.Pt.HeadC]) + Axis(k.R[AnyBody.BodyLinks.Head], 0) * (float)(.9 * b.HeadRadius), (float)(.2 * b.HeadRadius), Clay(new Color(.35f, .27f, .22f)));
            for (int s = 0; s < 2; s++)
            {
                int ft = s == 0 ? AnyBody.BodyLinks.FtL : AnyBody.BodyLinks.FtR, to = ft + 1;
                var an = k[s == 0 ? AnyBody.Pt.AnL : AnyBody.Pt.AnR];
                var ball = k[s == 0 ? AnyBody.Pt.BallL : AnyBody.Pt.BallR];
                Box(root.transform, an, k.R[ft], -b.HeelLength, b.BallLength, b.FootHalfWidth, -b.AnkleHeight, -b.AnkleHeight + b.FootHeight, (bears & (1 << ft)) != 0 ? bearing : skin);
                Box(root.transform, ball, k.R[to], 0, b.ToeLength, b.ToeHalfWidth, 0, b.ToeHeight, (bears & (1 << to)) != 0 ? bearing : skin);
            }
            foreach (var c in problem.Contacts)
            {
                var dot = Sphere(root.transform, U(c.Point), .012f, Clay(KindColour(c.Kind)));
                dot.name = "Contact " + c.Kind;
            }
            Vector3 com = U(k.Com);
            Sphere(root.transform, com, .02f, Clay(new Color(.15f, .15f, .18f)));
            Capsule(root.transform, com, new Vector3(com.x, 0, com.z), .003f, Clay(new Color(.15f, .15f, .18f)));
            return root;
        }

        private static AnyBody.Kin Kin(AnyBody.BodyModel b, double[] q)
        {
            var dq = new AnyBody.D[q.Length];
            for (int i = 0; i < q.Length; i++) dq[i] = q[i];
            var k = new AnyBody.Kin();
            AnyBody.Kinematics.Forward(b, dq, k);
            return k;
        }

        // The highest point of the body's solids at a pose (its standing height, for the standing fit).
        private static double Highest(AnyBody.BodyModel b, double[] q)
        {
            var k = Kin(b, q);
            double top = k[AnyBody.Pt.HeadC].Z.V + b.HeadRadius;
            for (int c = 0; c < 10; c++) top = Math.Max(top, Math.Max(k.CapA[c].Z.V, k.CapC[c].Z.V) + k.CapR[c]);
            return top;
        }

        private static Vector3 Centre(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.center;
        }

        // The solver's frame (x forward, y left, z up; right-handed) in Unity's (x right, y up, z forward).
        private static Vector3 U(AnyBody.DV3 p) => new Vector3((float)-p.Y.V, (float)p.Z.V, (float)p.X.V);
        private static Vector3 U(AnyBody.Vec3 p) => new Vector3((float)-p.Y, (float)p.Z, (float)p.X);
        private static Vector3 Axis(AnyBody.DM3 r, int column) =>
            column == 0 ? U(new AnyBody.DV3(r.M00, r.M10, r.M20)) : column == 1 ? U(new AnyBody.DV3(r.M01, r.M11, r.M21)) : U(new AnyBody.DV3(r.M02, r.M12, r.M22));

        private void Capsule(Transform parent, Vector3 a, Vector3 c, float radius, Material material)
        {
            float length = Vector3.Distance(a, c);
            if (length > 1e-4f)
            {
                var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                UnityEngine.Object.Destroy(rod.GetComponent<Collider>());
                rod.transform.SetParent(parent, false);
                rod.transform.position = .5f * (a + c);
                rod.transform.rotation = Quaternion.FromToRotation(Vector3.up, c - a);
                rod.transform.localScale = new Vector3(2 * radius, .5f * length, 2 * radius);
                rod.GetComponent<Renderer>().sharedMaterial = material;
            }
            Sphere(parent, a, radius, material);
            Sphere(parent, c, radius, material);
        }

        private GameObject Sphere(Transform parent, Vector3 at, float radius, Material material)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UnityEngine.Object.Destroy(ball.GetComponent<Collider>());
            ball.transform.SetParent(parent, false);
            ball.transform.position = at;
            ball.transform.localScale = Vector3.one * 2 * radius;
            ball.GetComponent<Renderer>().sharedMaterial = material;
            return ball;
        }

        // A box in a link's frame from its origin: x from x0 to x1 (its length), y within half a width, z from z0 to z1.
        private void Box(Transform parent, AnyBody.DV3 origin, AnyBody.DM3 r, double x0, double x1, double halfWidth, double z0, double z1, Material material)
        {
            Vector3 along = Axis(r, 0), up = Axis(r, 2);
            Vector3 centre = U(origin) + along * (float)(.5 * (x0 + x1)) + up * (float)(.5 * (z0 + z1));
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(box.GetComponent<Collider>());
            box.transform.SetParent(parent, false);
            box.transform.position = centre;
            // (A box is the same mirrored, so the change of handedness between the frames does not show in it.)
            box.transform.rotation = Quaternion.LookRotation(along, up);
            box.transform.localScale = new Vector3((float)(2 * halfWidth), (float)(z1 - z0), (float)(x1 - x0));
            box.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material Clay(Color colour)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = colour };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .15f);
            return material;
        }

        // A faint grid, one line a tile.
        private static Texture2D Grid()
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGB24, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 8 };
            var pixels = new Color32[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    pixels[y * 64 + x] = x == 0 || y == 0 ? new Color32(205, 205, 205, 255) : new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply(true);
            return texture;
        }

        private static Color KindColour(AnyBody.SupportKind kind)
        {
            switch (kind)
            {
                case AnyBody.SupportKind.Hand: case AnyBody.SupportKind.Forearm: return new Color(.85f, .25f, .2f);
                case AnyBody.SupportKind.Knee: case AnyBody.SupportKind.Shin: return new Color(.2f, .45f, .85f);
                case AnyBody.SupportKind.Sole: case AnyBody.SupportKind.Heel: case AnyBody.SupportKind.Ball: case AnyBody.SupportKind.Toes: return new Color(.2f, .65f, .3f);
                case AnyBody.SupportKind.HandOnThigh: return new Color(.9f, .6f, .1f);
                default: return new Color(.6f, .3f, .7f);
            }
        }

        private static string F(double v) => double.IsInfinity(v) || double.IsNaN(v) ? "" : v.ToString("0.####", CultureInfo.InvariantCulture);
        private static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "'") + "\"";
    }
}
