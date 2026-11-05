using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The stances of the getting up for any body (layer 1; Luis, November 3, 2026) on the drawn miners: each stance fitted
    // to the miner as the game makes it (BodyFrame), its free limbs resting (Luis, November 5), and the miner drawn in it,
    // in its own place and light, from its four sides, on a level stage (the fits are for level ground). The posed body's
    // segments are written from the fit each frame (AtlasPoser) and MinerBody draws the bones from them, as it draws a
    // physical body (Docs/Technical/AnyBody/INTEGRATION.md 3.3, step 5). Nothing of the game is changed.
    // Run on its own: -testFilter WonderGather.Tests.StanceAtlasDrawnCapture [-atlasOut <folder>] [-atlasBodies Small,Long]
    // [-atlasStances stand,quad]; then Docs/Technical/AnyBody/sheets/atlas_sheets.py <folder>.
    [Explicit]
    public sealed class StanceAtlasDrawnCapture
    {
        private const int Tile = 512;
        private const float FieldOfView = 20, Elevation = 15;

        private static readonly (string Key, string Family, object[] Args)[] Stances =
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

        // The side of the body that faces the camera: its front, its right, its back, its left (in the body's own frame).
        private static readonly (string Name, Vector3 From)[] Views =
        {
            ("front", Vector3.forward), ("right", Vector3.right), ("back", Vector3.back), ("left", Vector3.left),
        };

        private readonly List<GameObject> made = new List<GameObject>();

        [TearDown]
        public void End()
        {
            foreach (var g in made) if (g != null) UnityEngine.Object.Destroy(g);
            made.Clear();
            Time.captureFramerate = 0;
            GrassField.NotFor = null;
        }

        [UnityTest, Timeout(3600000)]
        public IEnumerator Capture()
        {
            string folder = CaptureTools.Argument("-atlasOut") ?? Path.Combine("Captures", "Atlas", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "_drawn");
            string tiles = Path.Combine(folder, "tiles");
            Directory.CreateDirectory(tiles);
            var only = CaptureTools.Argument("-atlasStances");
            var bodies = new HashSet<string>((CaptureTools.Argument("-atlasBodies") ?? "Small,Long,Round").Split(','));
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 30;

            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            camera.fieldOfView = FieldOfView;
            camera.nearClipPlane = .02f;
            // (The meadow's grass would stand between a low camera and the feet.)
            GrassField.NotFor = camera;
            // The place's own light at midday, held still so the sun can be turned with each view (TimeOfDay re-aims it
            // every frame: the design's check, C6).
            time.Hour = 13;
            Light sun = RenderSettings.sun;
            if (sun == null) foreach (var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional) { sun = l; break; }
            time.enabled = false;
            // A stage on the dirt path, along one of its long straight stretches, level, its top just above the ground.
            Vector2 from = ground.Path[4], to = ground.Path[5];
            Vector3 along = new Vector3(to.x - from.x, 0, to.y - from.y).normalized;
            Vector3 spot = new Vector3(from.x, 0, from.y);
            float top = -1e9f;
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI / 12;
                for (float r = 0; r <= 1.6f; r += .4f) top = Mathf.Max(top, ground.Height(spot.x + r * Mathf.Cos(a), spot.z + r * Mathf.Sin(a)));
            }
            spot.y = top + .002f;
            var stage = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            UnityEngine.Object.Destroy(stage.GetComponent<Collider>());
            stage.name = "Atlas stage";
            stage.transform.localScale = new Vector3(3.2f, .05f, 3.2f);
            stage.transform.position = spot - new Vector3(0, .05f, 0);
            stage.GetComponent<Renderer>().sharedMaterial = Clay(new Color(.55f, .46f, .36f));
            made.Add(stage);
            Quaternion facing = Quaternion.LookRotation(along);
            // (The pickaxe is not part of a stance, in the hands or slung on the back.)
            foreach (var held in UnityEngine.Object.FindObjectsByType<HeldThing>(FindObjectsSortMode.None))
                foreach (var r in held.GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name.StartsWith("Pickaxe")) foreach (var r in t.GetComponentsInChildren<Renderer>(true)) r.enabled = false;

            var csv = new StringBuilder("body,stance,look,name,cost,status,S,held,worst,base_mm,contact_err_mm,penetration_mm,fit_ms,starts,lowest_hand_mm\n");
            var log = new StringBuilder();
            var solver = new AnyBody.ReserveSolver();
            int shots = 0;
            for (int index = 0; index < choice.Count; index++)
            {
                string who = choice.NameOf(index);
                if (!bodies.Contains(who)) continue;
                choice.Choose(index);
                yield return null;
                var unit = choice.Current;
                unit.SetSelected(false);
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                var agent = unit.GetComponent<NavMeshAgent>();
                unit.Motor.Stop();
                if (agent != null && agent.isOnNavMesh) agent.Warp(new Vector3(spot.x, ground.Height(spot.x, spot.z), spot.z));
                if (agent != null) agent.enabled = false;
                unit.transform.SetPositionAndRotation(spot, facing);
                // Held still in the stance: the posed body lets its segments be, its balance and its breath are off, and
                // it is drawn at its finest (the level of detail is measured from its root: the check, C7).
                biped.LetGo = true;
                if (unit.TryGetComponent(out PhysicalBalance balance)) balance.Acts = false;
                if (unit.TryGetComponent(out MinerBreath breath)) breath.enabled = false;
                miner.Swell = 0; miner.Shrug = 0;
                foreach (var lod in unit.GetComponentsInChildren<LODGroup>()) lod.ForceLOD(0);
                var poser = unit.gameObject.AddComponent<AtlasPoser>();
                poser.Miner = miner;
                poser.Origin = spot;
                poser.Facing = facing;
                var hands = unit.gameObject.AddComponent<AtlasHands>();
                hands.Miner = miner;
                hands.Ground = spot.y;
                hands.Forward = facing * Vector3.forward;

                var b = BodyFrame.FromUnit(unit.gameObject);
                var standing = AnyBody.StanceFitter.Fit(b, AnyBody.StanceLibrary.Make(b, "stand"));
                float height = (float)Highest(b, standing.Q);
                float distance = .5f * 1.25f * height / Mathf.Tan(.5f * FieldOfView * Mathf.Deg2Rad);
                foreach (var s in Stances)
                {
                    if (only != null && Array.IndexOf(only.Split(','), s.Key) < 0) continue;
                    var st = AnyBody.StanceLibrary.Make(b, s.Family, s.Args);
                    var fit = AnyBody.StanceFitter.Fit(b, st);
                    var report = AnyBody.Statics.Report(b, st, fit.Q);
                    var problem = AnyBody.Statics.Build(b, fit.Q, st.Supports);
                    var reserve = solver.Solve(problem);
                    var worst = new List<string>();
                    if (reserve.Held && reserve.PerJointLp != null)
                        for (int j = 0; j < problem.Joints.Length; j++) if (reserve.PerJointLp[j] >= reserve.S - .005) worst.Add(problem.Joints[j].Name);
                    poser.Pose(b, fit.Q);
                    for (int side = 0; side < 2; side++)
                    {
                        hands.Bears[side] = false;
                        foreach (var sp in st.Supports)
                            if (sp.Side == side && (sp.Kind == AnyBody.SupportKind.Hand || sp.Kind == AnyBody.SupportKind.Forearm)) hands.Bears[side] = true;
                    }
                    // Held 40 frames, so the hung things settle, then seen from its four sides, the sun turned with the
                    // camera so that every side is lit alike.
                    for (int k = 0; k < 40; k++) yield return null;
                    Vector3 centre = poser.Centre();
                    // How low the drawn hands come, against the stage (the drawn wrists are straight: the check, C2).
                    float lowest = Mathf.Min(Lowest(miner.Rig.hands != null && miner.Rig.hands.Length > 0 ? miner.Rig.hands[0] : null), Lowest(miner.Rig.hands != null && miner.Rig.hands.Length > 1 ? miner.Rig.hands[1] : null)) - spot.y;
                    csv.Append(string.Join(",", who, s.Key, "drawn", Quote(st.Name), F(fit.Cost), fit.Status, reserve.Held ? F(reserve.S) : "inf", reserve.Held ? "1" : "0",
                        Quote(string.Join(" ", worst)), F(1000 * report.BaseMargin), F(report.ContactErrMm), F(report.PenetrationMm), F(fit.Millis),
                        fit.Starts.ToString(CultureInfo.InvariantCulture), F(1000 * lowest))).Append('\n');
                    log.AppendLine($"ATLAS_DRAWN {who} {s.Key}: {st.Name}; cost {fit.Cost:F4}; S {(reserve.Held ? reserve.S.ToString("F3", CultureInfo.InvariantCulture) : "not held")}; the drawn hands' lowest point {1000 * lowest:F0} mm over the stage");
                    foreach (var view in Views)
                    {
                        Vector3 side = facing * view.From;
                        Vector3 eye = Quaternion.AngleAxis(-Elevation, Vector3.Cross(Vector3.up, side)) * side;
                        camera.transform.position = centre + eye * distance;
                        camera.transform.rotation = Quaternion.LookRotation(-eye, Vector3.up);
                        Vector3 looks = -eye;
                        if (sun != null) sun.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(40, Vector3.up) * Quaternion.AngleAxis(50, Vector3.Cross(Vector3.up, looks)) * looks, Vector3.up);
                        yield return null;
                        CaptureTools.Render(camera, Path.Combine(tiles, $"{who}_{s.Key}_drawn_{view.Name}"), Tile, Tile);
                        shots++;
                    }
                }
                UnityEngine.Object.Destroy(poser);
                UnityEngine.Object.Destroy(hands);
                biped.LetGo = false;
                if (balance != null) balance.Acts = true;
                if (breath != null) breath.enabled = true;
                foreach (var lod in unit.GetComponentsInChildren<LODGroup>()) lod.ForceLOD(-1);
                if (agent != null) agent.enabled = true;
                yield return null;
            }
            File.WriteAllText(Path.Combine(folder, "figures.csv"), csv.ToString());
            File.WriteAllText(Path.Combine(folder, "log.txt"), log.ToString());
            Debug.Log(log.ToString());
            Assert.That(shots, Is.GreaterThan(0), "Nothing was drawn.");
        }

        private static float Lowest(Transform bone)
        {
            if (bone == null) return float.PositiveInfinity;
            float low = bone.position.y;
            foreach (var t in bone.GetComponentsInChildren<Transform>()) low = Mathf.Min(low, t.position.y);
            return low;
        }

        internal static double Highest(AnyBody.BodyModel b, double[] q)
        {
            var k = AtlasPoser.Kin(b, q);
            double topZ = k[AnyBody.Pt.HeadC].Z.V + b.HeadRadius;
            for (int c = 0; c < 10; c++) topZ = Math.Max(topZ, Math.Max(k.CapA[c].Z.V, k.CapC[c].Z.V) + k.CapR[c]);
            return topZ;
        }

        private static Material Clay(Color colour)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = colour };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .1f);
            return material;
        }

        private static string F(double v) => double.IsInfinity(v) || double.IsNaN(v) ? "" : v.ToString("0.####", CultureInfo.InvariantCulture);
        private static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "'") + "\"";
    }

    // A hand that bears the body's weight laid on the ground, its fingers forward, after MinerBody has drawn the arm (drawing
    // only: the drawn wrist is otherwise kept straight, and such a hand went into the ground, 4 to 13 cm: the design's
    // check, C2). The fingers go the forearm's way along the ground, or the body's way where the forearm stands upright.
    [DefaultExecutionOrder(60)]
    internal sealed class AtlasHands : MonoBehaviour
    {
        public MinerBody Miner;
        public readonly bool[] Bears = new bool[2];
        public float Ground;
        public Vector3 Forward = Vector3.forward;

        private void LateUpdate()
        {
            if (Miner == null || Miner.Rig.hands == null) return;
            for (int s = 0; s < 2 && s < Miner.Rig.hands.Length; s++)
            {
                var hand = Miner.Rig.hands[s];
                if (!Bears[s] || hand == null) continue;
                var tip = Farthest(hand);
                Vector3 wrist = hand.position, now = tip.position - wrist;
                float length = now.magnitude;
                if (length < 1e-3f) continue;
                var forearm = Miner.Rig.forearms != null && Miner.Rig.forearms.Length > s ? Miner.Rig.forearms[s] : null;
                Vector3 fore = forearm != null ? wrist - forearm.position : Vector3.down;
                Vector3 flat = Vector3.ProjectOnPlane(fore, Vector3.up);
                if (flat.magnitude < .3f * fore.magnitude) flat = Vector3.ProjectOnPlane(Forward, Vector3.up);
                flat.Normalize();
                // (The fingertips down to the ground, less a palm's thickness.)
                float drop = Mathf.Clamp(wrist.y - Ground - .012f, 0, .9f * length);
                Vector3 want = flat * Mathf.Sqrt(Mathf.Max(0, length * length - drop * drop)) - Vector3.up * drop;
                hand.rotation = Quaternion.FromToRotation(now, want) * hand.rotation;
            }
        }

        private static Transform Farthest(Transform root)
        {
            Transform far = root;
            float most = 0;
            foreach (var t in root.GetComponentsInChildren<Transform>())
            {
                float d = (t.position - root.position).sqrMagnitude;
                if (d > most) { most = d; far = t; }
            }
            return far;
        }
    }

    // Writes the posed body's segments from a fitted stance each frame, before MinerBody draws its bones from them, and
    // says that the limbs are turned as their segments are (INTEGRATION.md 2.4: the frame map; 3.3).
    [DefaultExecutionOrder(45)]
    internal sealed class AtlasPoser : MonoBehaviour
    {
        public MinerBody Miner;
        public Vector3 Origin;
        public Quaternion Facing = Quaternion.identity;
        private AnyBody.BodyModel body;
        private AnyBody.Kin kin;

        public void Pose(AnyBody.BodyModel b, double[] q) { body = b; kin = Kin(b, q); }

        internal static AnyBody.Kin Kin(AnyBody.BodyModel b, double[] q)
        {
            var dq = new AnyBody.D[q.Length];
            for (int i = 0; i < q.Length; i++) dq[i] = q[i];
            var k = new AnyBody.Kin();
            AnyBody.Kinematics.Forward(b, dq, k);
            return k;
        }

        // The middle of the posed body's points, in the world.
        public Vector3 Centre()
        {
            if (kin == null) return Origin;
            var bounds = new Bounds(W(kin.Pts[0]), Vector3.zero);
            foreach (var p in kin.Pts) bounds.Encapsulate(W(p));
            bounds.Encapsulate(W(kin[AnyBody.Pt.HeadC]) + Vector3.up * (float)body.HeadRadius);
            return bounds.center;
        }

        // The solver's frame (x forward, y left, z up) in the world, about the stage's origin and the body's facing.
        private Vector3 W(AnyBody.DV3 p) => Origin + Facing * new Vector3((float)-p.Y.V, (float)p.Z.V, (float)p.X.V);
        private Vector3 Dir(AnyBody.DM3 r, double x, double y, double z)
        {
            double ax = r.M00.V * x + r.M01.V * y + r.M02.V * z, ay = r.M10.V * x + r.M11.V * y + r.M12.V * z, az = r.M20.V * x + r.M21.V * y + r.M22.V * z;
            return Facing * new Vector3((float)-ay, (float)az, (float)ax);
        }

        // OwnBody's frame of a link (y along it away from its joint, z the side its elbow or knee sticks out), from the
        // link's axes: the trunk and head, the boots and toes (y its up, z its forward); the arms (y down it, z back); the
        // legs (y down it, z forward).
        private Quaternion Turn(int link, int kind)
        {
            var r = kin.R[link];
            Vector3 y = kind == 0 ? Dir(r, 0, 0, 1) : Dir(r, 0, 0, -1);
            Vector3 z = kind == 1 ? Dir(r, -1, 0, 0) : Dir(r, 1, 0, 0);
            return Quaternion.LookRotation(z, y);
        }

        private static void Put(Transform segment, Vector3 at, Quaternion turned)
        {
            if (segment == null) return;
            segment.SetPositionAndRotation(at, turned);
        }

        private void LateUpdate()
        {
            if (Miner == null || kin == null) return;
            var s = Miner.Solved;
            var k = kin;
            var b = body;
            Put(s.pelvis, W(k[AnyBody.Pt.P]), Turn(AnyBody.BodyLinks.Hips, 0));
            Put(s.torso, W(k[AnyBody.Pt.TrunkC]), Turn(AnyBody.BodyLinks.Trunk, 0));
            Put(s.head, W(k[AnyBody.Pt.HeadC]), Turn(AnyBody.BodyLinks.Head, 0));
            for (int i = 0; i < 2; i++)
            {
                int ua = i == 0 ? AnyBody.BodyLinks.UaL : AnyBody.BodyLinks.UaR, fa = ua + 1;
                int th = i == 0 ? AnyBody.BodyLinks.ThL : AnyBody.BodyLinks.ThR, sh = th + 1, ft = th + 2, to = th + 3;
                var shp = k[i == 0 ? AnyBody.Pt.ShL : AnyBody.Pt.ShR];
                var el = k[i == 0 ? AnyBody.Pt.ElL : AnyBody.Pt.ElR];
                var wr = k[i == 0 ? AnyBody.Pt.WrL : AnyBody.Pt.WrR];
                var hj = k[i == 0 ? AnyBody.Pt.HjL : AnyBody.Pt.HjR];
                var kn = k[i == 0 ? AnyBody.Pt.KnL : AnyBody.Pt.KnR];
                var an = k[i == 0 ? AnyBody.Pt.AnL : AnyBody.Pt.AnR];
                var ball = k[i == 0 ? AnyBody.Pt.BallL : AnyBody.Pt.BallR];
                if (s.upperArms != null && s.upperArms.Length == 2) Put(s.upperArms[i], .5f * (W(shp) + W(el)), Turn(ua, 1));
                if (s.forearms != null && s.forearms.Length == 2) Put(s.forearms[i], .5f * (W(el) + W(wr)), Turn(fa, 1));
                if (s.thighs != null && s.thighs.Length == 2) Put(s.thighs[i], .5f * (W(hj) + W(kn)), Turn(th, 2));
                if (s.shins != null && s.shins.Length == 2) Put(s.shins[i], .5f * (W(kn) + W(an)), Turn(sh, 2));
                // (The boot's box from its ankle, the toes' from the ball of the foot: their sizes are the posed body's, not written.)
                if (s.feet != null && s.feet.Length == 2) Put(s.feet[i], W(an) + Dir(k.R[ft], .5 * (b.BallLength - b.HeelLength), 0, -b.AnkleHeight + .5 * b.FootHeight), Turn(ft, 0));
                if (s.toes != null && s.toes.Length == 2) Put(s.toes[i], W(ball) + Dir(k.R[to], .5 * b.ToeLength, 0, .5 * b.ToeHeight), Turn(to, 0));
            }
            Miner.TurnsAsSolved();
        }
    }
}
