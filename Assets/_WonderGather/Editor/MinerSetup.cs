using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // S1d: the three miners as units for the RTS. Each rigged model from Art/Blender/Worker/workers.py --rigged
    // (Miner_<Name>.fbx, its atlas and miners.json) becomes a prefab: a navigating unit whose procedural body
    // solves onto invisible segments sized from the model's skeleton, and whose bones MinerBody turns to match.
    // Three levels of detail, one painted material and the drawn outline; a lamp's light where it has one.
    // AddToOrdinaryPlace puts all three in the Ordinary Place, one standing at a time, with the choice (M).
    public static class MinerSetup
    {
        public const string Folder = "Assets/_WonderGather/Art/Worker/Miners";
        private const string MaterialFolder = "Assets/_WonderGather/Materials/Miners";
        public const string PrefabFolder = "Assets/_WonderGather/Prefabs/Miners";
        private const string BodyPrefab = "Assets/_WonderGather/Prefabs/LivingBodyBiped.prefab";
        public static readonly string[] Names = { "Small", "Long", "Round" };
        private static readonly string[] Notes =
        {
            "young and curious, a lantern at the hip",
            "tall and unhurried, a pickaxe on the back",
            "sturdy and laughing, a lamp on the cap",
        };

        [Serializable] private class Manifest { public Entry[] miners; }

        [Serializable]
        private class Entry
        {
            public string name;
            public float height, hipHeight, hipWidth, leg, ankleHeight, heelLength, ballLength, toeLength, waistRise, headRise;
            public float upperArm, forearm, armOut, armDrop, armForward;
            public float[] shoulder;
            // The walk natural to this body: the Froude number of its comfortable walk, and the walk's character.
            public float froude, bounce, sway, arm_swing;
            // Each arm's own carriage (left, right): further out, and how much of the swing it keeps.
            public float[] armCarry, armSwingSide;
            // What hangs and swings: a lantern or mug in a hand, a satchel on its strap.
            public Hung[] hanging;
            // How far a thigh swings, in degrees, before it reaches the skirt's front and back flaps.
            public float[] skirtSlack;
            // The body's shape, for a tool swung in front of it: how far its front and its face stand ahead of the
            // hips, and its head's half-width.
            public float bodyFront, faceFront, headHalf;
            // The hands that close round a handle (hands.py).
            public Gripped[] grips;
            // What each part of the body weighs (weights.py).
            public Weighed weights;
        }

        [Serializable]
        private class Weighed
        {
            public float mass, body, worn, armRadius, legRadius;
            // Four joints as modelled (the hips' middle, the collar, the left and the right shoulder), in the being's
            // own space: how the model sits in its prefab is found from them.
            public float[] frame;
            public WeighedPart[] parts;
        }

        [Serializable]
        private class WeighedPart
        {
            public string bone;
            public float mass;
            // Its centre, in the being's own space as modelled.
            public float[] at;
        }

        [Serializable]
        private class Gripped
        {
            // The hand's bone, and its fingers' bones: the four fingers, then the thumb, three each from the knuckle.
            public string hand;
            public string[] bones;
            // Each digit's four joints (knuckle to tip), in the being's own space (x to its right, y up, z forward):
            // as modelled, with the hand open, and closed round a handle of each radius (one after another).
            public float[] rest, open, radii, closed;
            // A point on each handle's axis, and the handles' direction through the closed fingers.
            public float[] centres, axis;
            // The way the fingers leave the wrist.
            public float[] along;
        }

        // A tool made for a miner (tools.json, written by Art/Blender/Worker/tools.py), in the tool's own space:
        // y up the handle towards the head, z the way it strikes.
        [Serializable] private class ToolFile { public Tooled[] tools; }

        [Serializable]
        private class Tooled
        {
            public string name;
            public float size, length, headRadius, foot, top;
            // Where the hands grip (the right hand's, then the left's), the handle's radius at each, the ball
            // the strike sweeps, and the point itself.
            public float[] primaryGrip, secondaryGrip, gripRadii, head, tip;
            // What it weighs, where, and how hard it is to turn (weights.py).
            public float mass;
            public float[] centre, inertia, inertiaTurn;
        }

        [Serializable]
        private class Hung
        {
            public string bone, hand;
            public float length, damping, limit, stopDistance;
            // In the being's own space (x to its right, y up, z forward).
            public float[] aim, stopNormal, handle;
            // The limb under the cloth it rests on: its bone, how far down that bone, and the cloth's share of its movement.
            public string pusher;
            public float pushShare;
            // Where it rests on that cloth, in the being's space, from the ground under its middle.
            public float[] pushPoint;
            // It hangs from something sewn to cloth (a loop on an apron): the place it hangs from moves with that cloth.
            public bool rides;
            // It hangs in a loop with its head across the loop, and swings only square to that head (its handle).
            public bool hinged;
        }

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        [MenuItem("Wonder Gather/Create The Miners")]
        public static void CreatePrefabs()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play mode first.");
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Folder + "/miners.json"));
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory(PrefabFolder);
            AssetDatabase.Refresh();
            foreach (string name in Names)
            {
                var entry = manifest.miners.FirstOrDefault(x => x.name == name) ?? throw new InvalidOperationException("miners.json has no " + name);
                Create(name, entry);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("MINERS_PREFABS_OK");
        }

        public static string PrefabPath(string name) => PrefabFolder + "/Miner_" + name + ".prefab";

        // A thing that hangs by its handle from a hook or a loop sewn to the cloth (not in a hand, and not hinged in a
        // loop by its head, as a hammer is) can be taken off it by a hand.
        private static bool Taken(Hung h) => h.rides && !h.hinged && string.IsNullOrEmpty(h.hand) && h.handle != null && h.handle.Length == 3;

        // What a hand needs to know to take a hanging thing, read from the thing's own skin (which is wholly its
        // bone's): its handle's radius (the bar the fingers close round, at the place it hangs from), how far it
        // reaches below that place along the way it hangs, and how far to either side of that line, square to its bar.
        private static void Measure(SkinnedMeshRenderer renderer, Transform thing, Vector3 aim, Vector3 bar, out float grip, out float deep, out float wide)
        {
            grip = deep = wide = 0;
            var mesh = renderer.sharedMesh;
            var vertices = mesh.vertices;
            var weights = mesh.boneWeights;
            int own = Array.IndexOf(renderer.bones, thing);
            float scale = renderer.transform.lossyScale.x;
            Vector3 place = renderer.transform.InverseTransformPoint(thing.position);
            Vector3 down = renderer.transform.InverseTransformDirection(thing.rotation * aim).normalized;
            Vector3 along = renderer.transform.InverseTransformDirection(thing.rotation * bar).normalized;
            Vector3 square = Vector3.Cross(down, along).normalized;
            bool Mine(int v)
            {
                var w = weights[v];
                return (w.boneIndex0 == own ? w.weight0 : 0) + (w.boneIndex1 == own ? w.weight1 : 0)
                       + (w.boneIndex2 == own ? w.weight2 : 0) + (w.boneIndex3 == own ? w.weight3 : 0) >= .5f;
            }
            Vector3 From(int v) => (vertices[v] - place) * scale;
            for (int v = 0; v < vertices.Length; v++)
            {
                if (!Mine(v)) continue;
                Vector3 p = From(v);
                float below = Vector3.Dot(p, down);
                deep = Mathf.Max(deep, below);
                // Below the handle and what carries it, the thing itself.
                if (below > .04f) wide = Mathf.Max(wide, Mathf.Abs(Vector3.Dot(p, square)));
            }
            // The bar, cut across at the place the thing hangs from: its skin crosses that cut at the bar's radius.
            // (A bar's own points are at its ends; near the cut there is nothing else of the thing.)
            var corners = mesh.triangles;
            for (int c = 0; c + 2 < corners.Length; c += 3)
            {
                if (!Mine(corners[c]) || !Mine(corners[c + 1]) || !Mine(corners[c + 2])) continue;
                for (int side = 0; side < 3; side++)
                {
                    Vector3 a = From(corners[c + side]), b = From(corners[c + (side + 1) % 3]);
                    float fromA = Vector3.Dot(a, along), fromB = Vector3.Dot(b, along);
                    if (fromA * fromB > 0 || Mathf.Approximately(fromA, fromB)) continue;
                    float round = Vector3.Lerp(a, b, fromA / (fromA - fromB)).magnitude;
                    if (round < .02f) grip = Mathf.Max(grip, round);
                }
            }
            Debug.Log($"MINER_TAKEN {thing.name}: its handle {grip * 1000:F1} mm round, {deep * 1000:F0} mm deep, {wide * 1000:F0} mm to each side");
        }

        // How far the body's clothes reach to one side (from the pelvis), over the heights at which a thing so deep
        // hangs from the hand of a relaxed arm: where the body's side would stop it. Read from the body's own skin,
        // leaving out the arms and whatever hangs on it.
        private static float Beside(SkinnedMeshRenderer renderer, Transform root, Transform pelvis, int side, float top, float deep, HashSet<Transform> notBody)
        {
            var mesh = renderer.sharedMesh;
            var vertices = mesh.vertices;
            var weights = mesh.boneWeights;
            var bones = renderer.bones;
            Vector3 right = root.right, forward = root.forward;
            float reach = 0;
            for (int v = 0; v < vertices.Length; v++)
            {
                var w = weights[v];
                int most = w.boneIndex0;
                float share = w.weight0;
                if (w.weight1 > share) { most = w.boneIndex1; share = w.weight1; }
                if (w.weight2 > share) { most = w.boneIndex2; share = w.weight2; }
                if (w.weight3 > share) { most = w.boneIndex3; }
                if (notBody.Contains(bones[most])) continue;
                Vector3 p = renderer.transform.TransformPoint(vertices[v]);
                if (p.y > top + .03f || p.y < top - deep) continue;
                float ahead = Vector3.Dot(p - pelvis.position, forward);
                if (ahead < -.09f || ahead > .14f) continue;
                reach = Mathf.Max(reach, Vector3.Dot(p - pelvis.position, right) * side);
            }
            return reach;
        }

        // What a hand needs to know of each thing that hangs on a miner by a handle, measured on the finished miner.
        private static void MeasureThings(GameObject root, Entry e)
        {
            var miner = root.GetComponent<MinerBody>();
            var biped = root.GetComponent<ProceduralBiped>();
            var renderer = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(r => r.name).First();
            var things = miner.Things.ToArray();
            var rig = miner.Rig;
            var notBody = new HashSet<Transform>(rig.upperArms.SelectMany(a => a.GetComponentsInChildren<Transform>(true)));
            foreach (var t in things) if (t.bone != null) notBody.Add(t.bone);
            var P = biped.SavedProportions;
            foreach (var h in e.hanging ?? new Hung[0])
            {
                int k = Array.FindIndex(things, t => t.bone != null && t.bone.name == h.bone);
                if (k < 0) continue;
                things[k].bar = Vector3.zero; things[k].grip = things[k].deep = things[k].wide = things[k].clear = 0;
                if (!Taken(h)) continue;
                var bone = things[k].bone;
                Vector3 bar = Quaternion.Inverse(bone.rotation) * (root.transform.rotation * new Vector3(h.handle[0], h.handle[1], h.handle[2]).normalized);
                Measure(renderer, bone, things[k].aim, bar, out float grip, out float deep, out float wide);
                // The hand on its own side carries it. Its handle hangs where a handle lies in the hand of that arm,
                // relaxed: below the wrist by the hand's own measure.
                int side = Vector3.Dot(bone.position - rig.pelvis.position, root.transform.right) < 0 ? -1 : 1, hand = side < 0 ? 0 : 1;
                float below = miner.CanHold(hand) ? Vector3.Scale(miner.Fingers(hand).centres[0], rig.hands[hand].lossyScale).magnitude : 0;
                float top = root.transform.position.y + P.hipHeight + P.waistRise + P.shoulder.y - P.armHang.y - below;
                float reach = Beside(renderer, root.transform, rig.pelvis, side, top, deep, notBody);
                things[k].bar = bar; things[k].grip = grip; things[k].deep = deep; things[k].wide = wide;
                things[k].clear = reach + wide + .012f;
                Debug.Log($"MINER_CARRIED {root.name} {bone.name}: its handle hangs {top:F3} m up in the {(hand == 0 ? "left" : "right")} hand; the body reaches {reach:F3} m to that side there; it is stopped at {things[k].clear:F3} m");
            }
            miner.ConfigureThings(things);
        }

        // The miners' prefabs as they are, with what a hand needs to take the things that hang on them measured from
        // their models. The rest of each prefab is left as it is. (Creating the prefabs measures the same.)
        [MenuItem("Wonder Gather/Measure What Hangs On The Miners")]
        public static void MeasureHanging()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play mode first.");
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Folder + "/miners.json"));
            foreach (string name in Names)
            {
                var e = manifest.miners.FirstOrDefault(x => x.name == name) ?? throw new InvalidOperationException("miners.json has no " + name);
                var root = PrefabUtility.LoadPrefabContents(PrefabPath(name));
                try
                {
                    MeasureThings(root, e);
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabPath(name));
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("MINERS_HANGING_MEASURED");
        }

        private static void Create(string name, Entry e)
        {
            var body = Material(name);
            var glow = MaterialAt(MaterialFolder + "/Lamp glow.mat", Painted());
            glow.SetColor("_BaseColor", new Color(.35f, .22f, .1f));
            // A flame's warm amber, not white: bright enough to glow, not so bright that it bleaches.
            glow.SetColor("_EmissionColor", new Color(1f, .6f, .24f) * 1.9f);
            glow.SetFloat("_Drawn", 1);
            var outline = MaterialAt(MaterialFolder + "/Outline.mat", Shader.Find("Wonder Gather/Outline") ?? throw new InvalidOperationException("The outline shader did not compile."));
            outline.SetFloat("_MaxWidth", .012f);
            outline.SetFloat("_Behind", .035f);
            EditorUtility.SetDirty(glow);
            EditorUtility.SetDirty(outline);

            string modelPath = Folder + "/Miner_" + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath) ?? throw new FileNotFoundException("Export the rigged miners first.", modelPath);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
            importer.importAnimation = false;
            importer.optimizeGameObjects = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.bakeAxisConversion = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Miner_" + name), body);
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Glow"), glow);
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) ?? throw new InvalidOperationException("The miner did not import: " + name);

            float scale = e.hipHeight / 1.43f;
            var root = new GameObject("Miner " + name);
            var collider = root.AddComponent<CapsuleCollider>();
            collider.height = e.height;
            collider.center = new Vector3(0, e.height * .5f, 0);
            collider.radius = Mathf.Clamp(e.height * .15f, .2f, .3f);
            var agent = root.AddComponent<NavMeshAgent>();
            // Each walks at its own comfortable pace: the speed at which its legs give its walk's Froude number
            // (v² / g·h; about .25 is a comfortable walk for any size). Small steps quickly, Long strides slowly.
            float standing = e.ankleHeight + .956f * 2 * e.leg;
            agent.speed = Mathf.Sqrt(Mathf.Clamp(e.froude > 0 ? e.froude : .25f, .15f, .35f) * 9.81f * standing);
            Debug.Log($"MINER_PACE {name} {agent.speed:F2} m/s");
            agent.acceleration = 4;
            agent.angularSpeed = 180;
            agent.radius = .3f;
            agent.height = e.height;
            agent.stoppingDistance = .12f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 40;
            root.AddComponent<UnitMotor>();
            var selectable = root.AddComponent<SelectableUnit>();
            var ring = Ring(root.transform, Mathf.Clamp(e.height * .3f, .38f, .55f));
            selectable.Configure(ring);

            // The procedural body's solution: invisible segments it places every frame.
            var solution = new GameObject("Body solution").transform;
            solution.SetParent(root.transform, false);
            Transform Part(string label, Vector3 size)
            {
                var part = new GameObject(label).transform;
                part.SetParent(solution, false);
                part.localScale = size;
                return part;
            }
            var pelvis = Part("Pelvis", Vector3.one);
            var torso = Part("Torso", Vector3.one);
            var head = Part("Head", Vector3.one);
            var thighs = new Transform[2]; var shins = new Transform[2]; var feet = new Transform[2]; var toes = new Transform[2];
            var upperArms = new Transform[2]; var forearms = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                string side = i == 0 ? "Left " : "Right ";
                thighs[i] = Part(side + "thigh", Vector3.one);
                shins[i] = Part(side + "shin", Vector3.one);
                feet[i] = Part(side + "foot", new Vector3(.1f, .06f, e.heelLength + e.ballLength));
                toes[i] = Part(side + "toe", new Vector3(.1f, .05f, e.toeLength));
                upperArms[i] = Part(side + "upper arm", Vector3.one);
                forearms[i] = Part(side + "forearm", Vector3.one);
            }
            var biped = root.AddComponent<ProceduralBiped>();
            biped.Configure(pelvis, torso, head, thighs, shins, feet, upperArms, forearms);
            biped.ConfigureToes(toes);
            biped.ConfigureRing(ring.transform);
            biped.SetProportions(new ProceduralBiped.Proportions
            {
                // Standing, the knees keep the same slight bend as the original body's (hip to ankle 95.6% of the leg).
                hipHeight = standing,
                hipWidth = e.hipWidth, legSegment = e.leg,
                ankleHeight = e.ankleHeight, heelLength = e.heelLength, ballLength = e.ballLength, toeLength = e.toeLength,
                waistRise = e.waistRise, torsoRise = (e.shoulder[1] * .5f), headRise = e.headRise,
                shoulder = new Vector3(e.shoulder[0], e.shoulder[1], e.shoulder[2]),
                upperArm = e.upperArm, forearm = e.forearm,
                armHang = new Vector3(e.armOut, e.armDrop, e.armForward),
                scale = scale,
                bounce = e.bounce > 0 ? e.bounce : 1, sway = e.sway > 0 ? e.sway : 1, armSwing = e.arm_swing > 0 ? e.arm_swing : 1,
                armCarry = e.armCarry != null && e.armCarry.Length == 2 ? new Vector2(e.armCarry[0], e.armCarry[1]) : Vector2.zero,
                armSwingSide = e.armSwingSide != null && e.armSwingSide.Length == 2 ? new Vector2(e.armSwingSide[0], e.armSwingSide[1]) : Vector2.one,
                bodyFront = e.bodyFront, faceFront = e.faceFront, headHalf = e.headHalf,
            });
            biped.SetTuning(.65f, Mathf.Clamp(.16f * scale, .05f, .3f), .34f);

            // The model, turned to face the unit's forward, with its levels of detail and outline.
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = "Model";
            instance.transform.SetParent(root.transform, false);
            var bones = instance.GetComponentsInChildren<Transform>(true);
            Transform Bone(string bone) => bones.FirstOrDefault(x => x.name == bone) ?? throw new InvalidOperationException($"{name} has no bone {bone}.");
            Vector3 up = Bone("Head").position - Bone("Pelvis").position;
            // -minerStance built: stand the model on its own vertical, as it was built, not with its head straight
            // above its hips. For Luis's choice P1 (Long was built with the head carried forward, and so leans back
            // 11 degrees in the game). The default is what has been played since October 3.
            if (Argument("-minerStance") == "built") up = instance.transform.up;
            Vector3 forward = Vector3.ProjectOnPlane(Bone("Toe.L").position - Bone("Foot.L").position + Bone("Toe.R").position - Bone("Foot.R").position, up);
            instance.transform.localRotation = Quaternion.Inverse(Quaternion.LookRotation(forward, up)) * instance.transform.localRotation;
            float height = instance.GetComponentsInChildren<Renderer>().Select(r => r.bounds.max.y).Max();
            if (Mathf.Abs(height - e.height) > e.height * .15f)
                throw new InvalidOperationException($"{name} imported {height:F2} m tall, expected {e.height:F2} m: check the export's units.");

            var renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(r => r.name).ToArray();
            if (renderers.Length != 3) throw new InvalidOperationException($"{name} should have three levels of detail, has {renderers.Length}.");
            var lods = instance.GetComponent<LODGroup>() ?? instance.AddComponent<LODGroup>();
            // Close views: the full model. The Strategy camera's usual height: the middle one. Far: the lightest.
            lods.SetLODs(new[]
            {
                new LOD(.18f, new Renderer[] { renderers[0] }),
                new LOD(.035f, new Renderer[] { renderers[1] }),
                new LOD(.004f, new Renderer[] { renderers[2] }),
            });
            lods.RecalculateBounds();
            for (int i = 0; i < 3; i++)
            {
                var smr = renderers[i];
                smr.updateWhenOffscreen = false;
                smr.skinnedMotionVectors = false;
                // The drawn outline on the nearer two (it follows the mesh's last part, the body).
                if (i < 2) smr.sharedMaterials = smr.sharedMaterials.Append(outline).ToArray();
            }
            Lamp(renderers[0], name);

            var miner = root.AddComponent<MinerBody>();
            // What hangs and swings, from the model's data: each on its own bone.
            var hanging = (e.hanging ?? new Hung[0]).Select(h =>
            {
                Vector3 In(float[] v) => v != null && v.Length == 3 ? new Vector3(v[0], v[1], v[2]) : Vector3.zero;
                var hand = string.IsNullOrEmpty(h.hand) ? null : Bone(h.hand);
                var bone = Bone(h.bone);
                // Directions come in the being's space; each goes into the frame it turns with.
                Vector3 Into(Transform frame, float[] v) => Quaternion.Inverse(frame.rotation) * (root.transform.rotation * In(v).normalized);
                var pusher = string.IsNullOrEmpty(h.pusher) ? null : bones.FirstOrDefault(x => x.name == h.pusher);
                // Where the thing rests on the cloth that bone carries.
                Vector3 pushed = pusher != null ? root.transform.TransformPoint(In(h.pushPoint)) : Vector3.zero;
                var riders = new List<Transform>();
                var shares = new List<float>();
                if (h.rides) Riders(renderers[0], bone, riders, shares);
                return new MinerBody.Hanging
                {
                    riders = riders.ToArray(), rideShares = shares.ToArray(),
                    ridePoints = riders.Select(r => r.InverseTransformPoint(bone.position)).ToArray(),
                    pusher = pusher, pushShare = h.pushShare,
                    pushPoint = pusher != null ? pusher.InverseTransformPoint(pushed) : Vector3.zero,
                    pushRest = pusher != null ? Bone("Pelvis").InverseTransformPoint(pushed) : Vector3.zero,
                    bone = bone, length = h.length, damping = h.damping, limit = h.limit,
                    aim = Into(bone, h.aim),
                    stopNormal = h.stopDistance > 0 ? Into(Bone("Pelvis"), h.stopNormal) : Vector3.zero, stopDistance = h.stopDistance,
                    hand = hand,
                    handle = hand != null ? Into(hand, h.handle) : h.hinged && bone.parent != null ? Into(bone.parent, h.handle) : Vector3.zero,
                    hinged = h.hinged,
                };
            }).ToArray();
            miner.Configure(new MinerBody.Bones
            {
                pelvis = Bone("Pelvis"), spine = Bone("Spine"), chest = Bone("Chest"), neck = Bone("Neck"), head = Bone("Head"),
                upperArms = new[] { Bone("UpperArm.L"), Bone("UpperArm.R") }, forearms = new[] { Bone("Forearm.L"), Bone("Forearm.R") },
                hands = new[] { Bone("Hand.L"), Bone("Hand.R") }, thighs = new[] { Bone("Thigh.L"), Bone("Thigh.R") },
                shins = new[] { Bone("Shin.L"), Bone("Shin.R") }, feet = new[] { Bone("Foot.L"), Bone("Foot.R") }, toes = new[] { Bone("Toe.L"), Bone("Toe.R") },
            }, new MinerBody.Solution
            {
                pelvis = pelvis, torso = torso, head = head, upperArms = upperArms, forearms = forearms, thighs = thighs, shins = shins, feet = feet, toes = toes,
            }, hanging, new[] { ("SkirtFront.L", 0, true), ("SkirtBack.L", 0, false), ("SkirtFront.R", 1, true), ("SkirtBack.R", 1, false) }
                .Select(f => new MinerBody.Flap { bone = bones.FirstOrDefault(x => x.name == f.Item1), leg = f.Item2, front = f.Item3 })
                .Where(f => f.bone != null).ToArray(),
                e.skirtSlack != null && e.skirtSlack.Length == 2 ? new Vector2(e.skirtSlack[0], e.skirtSlack[1]) : Vector2.zero,
                Grips(e, root.transform, Bone));
            miner.ConfigureTool(Pickaxe(name, outline));
            MeasureThings(root, e);
            Weigh(root, name, e, Bone);
            Debug.Log($"MINER_HANGING {name} " + string.Join(", ", hanging.Select(h => $"{h.bone.name} {h.length:F3} m stop {h.stopDistance:F3}")));
            Debug.Log($"MINER_GRIPS {name} " + string.Join(", ", (e.grips ?? new Gripped[0]).Select(g => $"{g.hand} {g.bones.Length} joints, {g.radii.Length} handles")));
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath(name));
            Object.DestroyImmediate(root);
        }

        // What the body weighs: each part's mass at its place in its own bone. The model's data is in the being's space
        // as modelled; the prefab stands the model squared to its own axes, so where the model sits is found from
        // four of its joints (the hips, the collar, the shoulders) and the bones that are those joints.
        private static void Weigh(GameObject root, string name, Entry e, Func<string, Transform> bone)
        {
            var w = e.weights;
            if (w == null || w.parts == null || w.parts.Length == 0 || w.frame == null || w.frame.Length != 12)
                throw new InvalidOperationException($"{name} has not been weighed (workers.py --weigh).");
            Vector3 At(float[] v, int k = 0) => new Vector3(v[k], v[k + 1], v[k + 2]);
            Vector3 hipsM = At(w.frame), collarM = At(w.frame, 3), leftM = At(w.frame, 6), rightM = At(w.frame, 9);
            Vector3 hips = bone("Pelvis").position, collar = bone("Neck").position, left = bone("UpperArm.L").position, right = bone("UpperArm.R").position;
            Quaternion Frame(Vector3 up, Vector3 across) => Quaternion.LookRotation(Vector3.Cross(across, up).normalized, up.normalized);
            Quaternion sits = Frame(collar - hips, right - left) * Quaternion.Inverse(Frame(collarM - hipsM, rightM - leftM));
            float scale = (collar - hips).magnitude / (collarM - hipsM).magnitude;
            if (Mathf.Abs(scale - 1) > .02f) throw new InvalidOperationException($"{name}'s model is not the size its weights were measured at ({scale:F3}).");
            var parts = w.parts.Select(p =>
            {
                var b = bone(p.bone);
                return new PhysicalBody.Part { bone = b, mass = p.mass, centre = b.InverseTransformPoint(hips + sits * (At(p.at) - hipsM)) };
            }).ToArray();
            int Index(string n) => Array.FindIndex(w.parts, p => p.bone == n);
            // The back carries everything above the hips.
            var above = Enumerable.Range(0, w.parts.Length).Where(i => w.parts[i].bone != "Pelvis" && !w.parts[i].bone.StartsWith("Thigh")
                && !w.parts[i].bone.StartsWith("Shin") && !w.parts[i].bone.StartsWith("Foot") && !w.parts[i].bone.StartsWith("Toe")).ToArray();
            var physical = root.AddComponent<PhysicalBody>();
            physical.Configure(parts, new[] { Index("UpperArm.L"), Index("Forearm.L"), Index("Hand.L"), Index("UpperArm.R"), Index("Forearm.R"), Index("Hand.R") },
                above, hips, w.armRadius, w.legRadius);
            Debug.Log($"MINER_BACK {name}: its back gives {physical.BackCapacity:F0} N m at most");
            Debug.Log($"MINER_WEIGHT {name}: {physical.Mass:F1} kg in {parts.Length} parts; its centre {physical.CentreOfMass().y - root.transform.position.y:F3} m up; "
                + $"the model sits {Quaternion.Angle(sits, root.transform.rotation):F2} degrees off its prefab's axes");
        }

        public const string ToolFolder = "Assets/_WonderGather/Data/Miners";
        public static string PickaxePath(string name) => ToolFolder + "/Pickaxe_" + name + ".asset";

        // The pickaxe made for one miner: its model as a prefab in the tool's own space, painted and outlined as
        // the miners are, and its data (grips, the handle's thickness, the striking head) as the model's build
        // measured them. The mineral asks for a "pickaxe"; every size of it answers to that.
        private static ToolDefinition Pickaxe(string name, Material outline)
        {
            var file = JsonUtility.FromJson<ToolFile>(File.ReadAllText(Folder + "/tools.json"));
            var told = file.tools.FirstOrDefault(x => x.name == name) ?? throw new InvalidOperationException("tools.json has no pickaxe for " + name);
            string modelPath = Folder + "/Pickaxe_" + name + ".fbx", atlasPath = Folder + "/Pickaxe_" + name + "_Atlas.png";
            var texture = (TextureImporter)AssetImporter.GetAtPath(atlasPath) ?? throw new FileNotFoundException("The pickaxe's atlas is missing.", atlasPath);
            texture.sRGBTexture = true;
            texture.mipmapEnabled = true;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 4;
            texture.maxTextureSize = 512;
            texture.textureCompression = TextureImporterCompression.CompressedHQ;
            texture.SaveAndReimport();
            var paint = MaterialAt(MaterialFolder + "/Pickaxe " + name + ".mat", Painted());
            paint.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath));
            paint.SetColor("_BaseColor", Color.white);
            paint.SetFloat("_Variation", .12f);
            paint.SetFloat("_Brush", .3f);
            paint.SetFloat("_BrushScale", 14);
            paint.SetFloat("_Softness", .38f);
            paint.SetFloat("_Translucency", 0);
            paint.SetFloat("_Gloss", .12f);
            paint.SetFloat("_VertexColor", 0);
            paint.SetColor("_EmissionColor", Color.black);
            paint.SetFloat("_Drawn", 1);
            EditorUtility.SetDirty(paint);

            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath) ?? throw new FileNotFoundException("Build the miners' tools first.", modelPath);
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.bakeAxisConversion = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Pickaxe_" + name), paint);
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) ?? throw new InvalidOperationException("The pickaxe did not import: " + name);

            var root = new GameObject("Pickaxe (" + name + ")");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = "Model";
            instance.transform.SetParent(root.transform, false);
            var renderers = instance.GetComponentsInChildren<MeshRenderer>(true).OrderBy(r => r.name).ToArray();
            if (renderers.Length != 2) throw new InvalidOperationException($"{name}'s pickaxe should have two levels of detail, has {renderers.Length}.");
            // The data is in the tool's own space: the model must have come in the same way up, at the same size.
            // (The model comes in with its point towards its bearer; it is turned to strike forwards.)
            if (renderers[0].bounds.max.z < -renderers[0].bounds.min.z) instance.transform.localRotation = Quaternion.Euler(0, 180, 0) * instance.transform.localRotation;
            Bounds seen = renderers[0].bounds;
            Vector3 tip = new Vector3(told.tip[0], told.tip[1], told.tip[2]);
            if (Mathf.Abs(seen.min.y - told.foot) > .01f || Mathf.Abs(seen.max.y - told.top) > .01f || Mathf.Abs(seen.max.z - tip.z) > .01f || seen.max.z < -seen.min.z)
                throw new InvalidOperationException($"{name}'s pickaxe came in the wrong way round: it spans {seen.min} to {seen.max}; its foot should be at y {told.foot:F3}, its top at {told.top:F3}, its point at z {tip.z:F3}.");
            renderers[0].sharedMaterials = new[] { paint, outline };
            renderers[1].sharedMaterials = new[] { paint };
            // A tool is about a third of its bearer's height: it changes level, and is left out, when its bearer is.
            var lods = instance.GetComponent<LODGroup>() ?? instance.AddComponent<LODGroup>();
            lods.SetLODs(new[] { new LOD(.06f, new Renderer[] { renderers[0] }), new LOD(.0013f, new Renderer[] { renderers[1] }) });
            lods.RecalculateBounds();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/Pickaxe_" + name + ".prefab");
            Object.DestroyImmediate(root);

            Directory.CreateDirectory(ToolFolder);
            AssetDatabase.Refresh();
            var tool = AssetDatabase.LoadAssetAtPath<ToolDefinition>(PickaxePath(name));
            if (tool == null)
            {
                tool = ScriptableObject.CreateInstance<ToolDefinition>();
                AssetDatabase.CreateAsset(tool, PickaxePath(name));
            }
            Vector3 In(float[] v) => new Vector3(v[0], v[1], v[2]);
            tool.Configure("pickaxe", "Pickaxe", prefab, In(told.primaryGrip), In(told.secondaryGrip), In(told.head), told.headRadius,
                new Vector2(told.gripRadii[0], told.gripRadii[1]));
            if (!(told.mass > 0) || told.centre == null || told.inertia == null || told.inertiaTurn == null)
                throw new InvalidOperationException($"{name}'s pickaxe has not been weighed (tools.py --weigh).");
            tool.ConfigureWeight(told.mass, In(told.centre), In(told.inertia), new Quaternion(told.inertiaTurn[0], told.inertiaTurn[1], told.inertiaTurn[2], told.inertiaTurn[3]),
                told.foot, told.top, In(told.tip));
            EditorUtility.SetDirty(tool);
            Debug.Log($"MINER_TOOL {name}: a pickaxe {told.length:F2} m long, gripped at {told.primaryGrip[1]:F3} and {told.secondaryGrip[1]:F3}, "
                + $"its handle {told.gripRadii[0] * 2000:F0} and {told.gripRadii[1] * 2000:F0} mm thick there");
            return tool;
        }

        // The hands that close: for each, every finger joint's turn (in the joint's own frame) from its rest to the
        // open hand and to the hand closed round each handle. The model gives where the joints go; each bone takes
        // the smallest turn that aims it there, its parents' turns first, exactly as the model's build did when it
        // measured the closed hand on its handle (hands.turns). Index 0 is the left hand, 1 the right.
        private static MinerBody.Grip[] Grips(Entry e, Transform root, Func<string, Transform> bone)
        {
            var grips = new MinerBody.Grip[2];
            foreach (var g in e.grips ?? new Gripped[0])
            {
                var hand = bone(g.hand);
                var joints = g.bones.Select(bone).ToArray();
                int digits = joints.Length / 3, rows = g.radii.Length;
                if (joints.Length != digits * 3 || g.rest.Length != digits * 12 || g.open.Length != digits * 12 || g.closed.Length != rows * digits * 12 || g.centres.Length != rows * 3)
                    throw new InvalidOperationException($"{e.name}: the closing of {g.hand} does not match its bones.");
                Vector3 Told(float[] v, int point) => new Vector3(v[point * 3], v[point * 3 + 1], v[point * 3 + 2]);
                // The model's joints at rest are these bones: that fixes exactly how the model's space sits on this
                // hand (from the first finger's, the little finger's and the thumb's knuckles). The prefab turns the
                // model to face forward by its feet, which is near enough for a walk and not for a fingertip.
                Quaternion Frame(Vector3 p0, Vector3 p1, Vector3 p2) => Quaternion.LookRotation(p1 - p0, Vector3.Cross(p1 - p0, p2 - p0));
                int little = (digits - 2) * 4, thumb = (digits - 1) * 4;
                Quaternion sits = Frame(joints[0].position, joints[(digits - 2) * 3].position, joints[(digits - 1) * 3].position)
                                  * Quaternion.Inverse(Frame(Told(g.rest, 0), Told(g.rest, little), Told(g.rest, thumb)));
                Vector3 from = joints[0].position - sits * Told(g.rest, 0);
                Vector3 At(float[] v, int point) => sits * Told(v, point) + from;
                float fit = 0;
                for (int j = 0; j < joints.Length; j++) fit = Mathf.Max(fit, Vector3.Distance(joints[j].position, At(g.rest, j / 3 * 4 + j % 3)));
                if (fit > .001f) throw new InvalidOperationException($"{e.name}: the bones of {g.hand} are {fit * 1000:F1} mm from the model's joints.");
                Debug.Log($"MINER_GRIP_FIT {e.name} {g.hand}: bones on the model's joints within {fit * 1000:F2} mm; the model's space sits {Quaternion.Angle(sits, root.rotation):F2} degrees off the prefab's");
                Quaternion[] Turns(float[] goal, int first)
                {
                    var turns = new Quaternion[joints.Length];
                    for (int d = 0; d < digits; d++)
                    {
                        Quaternion carried = Quaternion.identity;
                        for (int j = 0; j < 3; j++)
                        {
                            int p = d * 4 + j;
                            Vector3 rest = At(g.rest, p + 1) - At(g.rest, p);
                            Vector3 want = Quaternion.Inverse(carried) * (At(goal, first + p + 1) - At(goal, first + p));
                            Quaternion turn = Quaternion.FromToRotation(rest, want);
                            carried *= turn;
                            var joint = joints[d * 3 + j];
                            turns[d * 3 + j] = Quaternion.Inverse(joint.rotation) * turn * joint.rotation;
                        }
                    }
                    return turns;
                }
                var closed = new List<Quaternion>();
                for (int r = 0; r < rows; r++) closed.AddRange(Turns(g.closed, r * digits * 4));
                grips[g.hand.EndsWith(".L") ? 0 : 1] = new MinerBody.Grip
                {
                    joints = joints, open = Turns(g.open, 0), radii = g.radii, closed = closed.ToArray(),
                    centres = Enumerable.Range(0, rows).Select(r => hand.InverseTransformPoint(At(g.centres, r))).ToArray(),
                    axis = Quaternion.Inverse(hand.rotation) * (sits * new Vector3(g.axis[0], g.axis[1], g.axis[2]).normalized),
                    along = Quaternion.Inverse(hand.rotation) * (sits * new Vector3(g.along[0], g.along[1], g.along[2]).normalized),
                };
            }
            return grips;
        }

        // A lamp's warm light where the model has glowing glass: on the bone that carries the glass.
        // The cloth a hanging thing is sewn to: the bones of the nearest skin that is not the thing's own, and
        // their shares. (Its loop is skinned like the cloth it is sewn on; the thing itself is wholly its bone's.)
        private static void Riders(SkinnedMeshRenderer renderer, Transform thing, List<Transform> riders, List<float> shares)
        {
            var mesh = renderer.sharedMesh;
            var vertices = mesh.vertices;
            var weights = mesh.boneWeights;
            int own = Array.IndexOf(renderer.bones, thing);
            Vector3 place = renderer.transform.InverseTransformPoint(thing.position);
            int nearest = -1;
            float best = float.MaxValue;
            for (int v = 0; v < vertices.Length; v++)
            {
                var w = weights[v];
                if ((w.boneIndex0 == own && w.weight0 > 0) || (w.boneIndex1 == own && w.weight1 > 0)
                    || (w.boneIndex2 == own && w.weight2 > 0) || (w.boneIndex3 == own && w.weight3 > 0)) continue;
                float d = (vertices[v] - place).sqrMagnitude;
                if (d < best) { best = d; nearest = v; }
            }
            if (nearest < 0) return;
            var found = weights[nearest];
            void Add(int index, float share) { if (share > .01f) { riders.Add(renderer.bones[index]); shares.Add(share); } }
            Add(found.boneIndex0, found.weight0); Add(found.boneIndex1, found.weight1);
            Add(found.boneIndex2, found.weight2); Add(found.boneIndex3, found.weight3);
            Debug.Log($"MINER_RIDES {thing.name} rides " + string.Join(", ", riders.Select((r, i) => $"{r.name} {shares[i]:F2}"))
                      + $" ({Mathf.Sqrt(best) * renderer.transform.lossyScale.x * 1000:F0} mm from its cloth)");
        }

        private static void Lamp(SkinnedMeshRenderer renderer, string name)
        {
            var mesh = renderer.sharedMesh;
            if (mesh.subMeshCount < 2) return;
            var indices = mesh.GetIndices(0);
            if (indices.Length == 0) return;
            var vertices = mesh.vertices;
            var weights = mesh.boneWeights;
            Vector3 centre = Vector3.zero;
            var votes = new float[renderer.bones.Length];
            foreach (int index in indices.Distinct())
            {
                centre += vertices[index];
                var w = weights[index];
                votes[w.boneIndex0] += w.weight0;
                votes[w.boneIndex1] += w.weight1;
            }
            centre /= indices.Distinct().Count();
            int bone = Array.IndexOf(votes, votes.Max());
            Debug.Log($"MINER_LAMP_VOTES {name} submeshes {mesh.subMeshCount} glass vertices {indices.Distinct().Count()} centre {renderer.transform.TransformPoint(centre)} " +
                      string.Join(" ", votes.Select((v, k) => v > 0 ? $"{renderer.bones[k].name}:{v:F0}" : null).Where(x => x != null)));
            var carrier = renderer.bones[bone];
            var light = new GameObject("Lamp light").AddComponent<Light>();
            light.transform.SetParent(carrier, false);
            light.transform.position = renderer.transform.TransformPoint(centre);
            light.type = LightType.Spot;
            light.color = new Color(1f, .72f, .42f);
            light.shadows = LightShadows.None;
            var forward = renderer.transform.root.forward;
            if (carrier.name == "Head")
            {
                // A cap lamp: a beam ahead and down onto the path.
                light.transform.rotation = Quaternion.LookRotation(Vector3.Lerp(forward, Vector3.down, .45f).normalized);
                light.spotAngle = 75;
                light.innerSpotAngle = 30;
                light.range = 5f;
                light.intensity = 1.6f;
            }
            else
            {
                // A carried lantern: a warm pool on the ground and the legs, never lighting the face from below.
                light.transform.rotation = Quaternion.LookRotation(Vector3.down, forward);
                light.spotAngle = 150;
                light.innerSpotAngle = 60;
                light.range = 2.2f;
                light.intensity = 1.1f;
            }
            Debug.Log($"MINER_LAMP {name} on {renderer.bones[bone].name}");
        }

        private static Shader Painted() => Shader.Find("Wonder Gather/Painted") ?? throw new InvalidOperationException("The painted shader did not compile.");

        private static Material Material(string name)
        {
            string atlasPath = Folder + "/Miner_" + name + "_Atlas.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(atlasPath) ?? throw new FileNotFoundException("The miner's atlas is missing.", atlasPath);
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.anisoLevel = 4;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            // The face's thin strokes live in this atlas: a sharper mip keeps them at a distance.
            importer.mipMapBias = -.6f;
            importer.SaveAndReimport();
            var material = MaterialAt(MaterialFolder + "/Miner " + name + ".mat", Painted());
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Variation", .12f);
            material.SetFloat("_Brush", .3f);
            material.SetFloat("_BrushScale", 14);
            material.SetFloat("_Softness", .38f);
            material.SetFloat("_Translucency", .12f);
            material.SetFloat("_Gloss", .08f);
            material.SetFloat("_VertexColor", 0);
            material.SetColor("_EmissionColor", Color.black);
            // Drawn, not only modelled: kept clear of the paint filter, so the face's few marks hold.
            material.SetFloat("_Drawn", 1);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material MaterialAt(string path, Shader shader)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader) material.shader = shader;
            return material;
        }

        // The selection ring, as the other units have it (the living body's ring, resized).
        private static GameObject Ring(Transform parent, float radius)
        {
            var template = AssetDatabase.LoadAssetAtPath<GameObject>(BodyPrefab) ?? throw new FileNotFoundException(BodyPrefab);
            var source = template.transform.Find("Selection ring") ?? throw new InvalidOperationException("The living body has no selection ring.");
            var ring = Object.Instantiate(source.gameObject, parent, false);
            ring.name = "Selection ring";
            var line = ring.GetComponent<LineRenderer>();
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2 / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius);
            }
            return ring;
        }

        // All three in the Ordinary Place where its worker stood; the remembered or first choice stands, the others wait.
        [MenuItem("Wonder Gather/Add The Miners To The Ordinary Place")]
        public static void AddToOrdinaryPlace()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play mode first.");
            var scene = EditorSceneManager.OpenScene(OrdinaryPlaceSetup.ScenePath, OpenSceneMode.Single);
            var selection = Object.FindAnyObjectByType<SelectionController>() ?? throw new InvalidOperationException("The scene has no selection.");
            // The worker the place was built with: its one unit that is not a miner.
            var old = Object.FindObjectsByType<SelectableUnit>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(x => x.GetComponent<MinerBody>() == null)?.gameObject;
            var choice = Object.FindAnyObjectByType<MinerChoice>(FindObjectsInactive.Include);
            Vector3 position;
            Quaternion rotation;
            if (old != null)
            {
                position = old.transform.position;
                rotation = old.transform.rotation;
                Object.DestroyImmediate(old);
            }
            else if (choice != null && choice.Current != null)
            {
                position = choice.Current.transform.position;
                rotation = choice.Current.transform.rotation;
            }
            else throw new InvalidOperationException("Neither the worker nor the miners were found in the Ordinary Place.");
            if (choice != null) Object.DestroyImmediate(choice.gameObject);
            foreach (var stale in Object.FindObjectsByType<MinerBody>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(stale.gameObject);

            var group = new GameObject("The miners");
            var units = new SelectableUnit[Names.Length];
            var prefabs = new GameObject[Names.Length];
            for (int i = 0; i < Names.Length; i++)
            {
                var prefab = prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(Names[i])) ?? throw new FileNotFoundException(PrefabPath(Names[i]));
                var unit = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                unit.transform.SetParent(group.transform, false);
                unit.transform.SetPositionAndRotation(position, rotation);
                units[i] = unit.GetComponent<SelectableUnit>();
            }
            choice = group.AddComponent<MinerChoice>();
            choice.Configure(units, Names, Notes, selection, 0);
            // A crowd of miners for the benchmark (-wgcrowd); idle otherwise.
            group.AddComponent<MinerCrowdBenchmark>().Configure(prefabs);
            // A first look at them at work, with K (until where and how they mine is decided).
            group.AddComponent<MinerWorkPreview>();
            EditorUtility.SetDirty(selection);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save " + OrdinaryPlaceSetup.ScenePath);
            Debug.Log("ORDINARY_PLACE_MINERS_OK");
        }

        // Batch: prefabs, then the Ordinary Place.
        public static void CreateAll()
        {
            CreatePrefabs();
            AddToOrdinaryPlace();
        }
    }
}
