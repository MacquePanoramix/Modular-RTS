using System;
using System.Collections;
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
    // Not a test: the game's own movement, recorded for the model audit's pose sweep (Docs/ArtDirection/ModelQualityMethod.md,
    // pass 4). Each miner stands, starts walking, walks straight, turns and stops, and then mines at a rock face with the
    // pickaxe made for it (two swings; those frames also carry the pickaxe's own matrix, "t", from its space to the mesh's).
    // 30 frames a second; every frame writes, for
    // each bone of the skinned mesh, the matrix that carries the mesh's rest pose to that frame's pose (the bone's world matrix
    // times its bind pose, in the mesh's own space), as skinning applies it. With the bones' rest positions, Blender poses the
    // separate parts exactly as the game does and checks them on every frame.
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerPoseRecord -poseOut <folder> [-captureMiner Small]
    [Explicit("A recording for the model audit, not a test.")]
    public sealed class MinerPoseRecord
    {
        private static void Matrix(StringBuilder json, Matrix4x4 m)
        {
            json.Append('[');
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 4; c++)
                {
                    if (r + c > 0) json.Append(',');
                    json.Append(m[r, c].ToString("0.######", CultureInfo.InvariantCulture));
                }
            json.Append(']');
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator Record()
        {
            string folder = CaptureTools.Argument("-poseOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "MinerPoses");
            string only = CaptureTools.Argument("-captureMiner");
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 30;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                // On the dirt path, along one of its long straight stretches.
                var from = ground.Path[4];
                var to = ground.Path[5];
                var spot = OnGround(new Vector3(from.x, 0, from.y));
                var away = OnGround(new Vector3(to.x, 0, to.y)) - spot;
                away.y = 0;
                away.Normalize();
                for (int index = 0; index < choice.Count; index++)
                {
                    string name = choice.NameOf(index);
                    if (only != null && !string.Equals(only, name, StringComparison.OrdinalIgnoreCase)) continue;
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    unit.GetComponent<NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    unit.GetComponent<ProceduralBiped>().ResetPose();
                    for (int k = 0; k < 40; k++) yield return null;

                    SkinnedMeshRenderer skin = null;
                    foreach (var r in unit.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        if (r.name.EndsWith("LOD0")) skin = r;
                    Assert.IsNotNull(skin, $"{name} has no LOD0 skinned mesh");
                    var bones = skin.bones;
                    var bind = skin.sharedMesh.bindposes;
                    var json = new StringBuilder();
                    json.Append("{\"name\":\"").Append(name).Append("\",\"fps\":30,\"bones\":[");
                    for (int b = 0; b < bones.Length; b++) json.Append(b > 0 ? "," : "").Append('"').Append(bones[b].name).Append('"');
                    // Each bone's rest matrix in the mesh's space (the inverse of its bind pose).
                    json.Append("],\"rest\":[");
                    for (int b = 0; b < bones.Length; b++) { if (b > 0) json.Append(','); Matrix(json, bind[b].inverse); }
                    json.Append("],\"frames\":[");
                    int frames = 0;
                    void Frame(string label, Transform held = null)
                    {
                        var toMesh = skin.transform.worldToLocalMatrix;
                        json.Append(frames++ > 0 ? "," : "").Append("{\"label\":\"").Append(label).Append("\",\"m\":[");
                        for (int b = 0; b < bones.Length; b++) { if (b > 0) json.Append(','); Matrix(json, toMesh * bones[b].localToWorldMatrix * bind[b]); }
                        json.Append("]");
                        if (held != null) { json.Append(",\"t\":"); Matrix(json, toMesh * held.localToWorldMatrix); }
                        json.Append("}");
                    }
                    for (int k = 0; k < 30; k++) { if (k % 3 == 0) Frame("stand"); yield return null; }
                    unit.Motor.TryMove(OnGround(spot + away * 10f));
                    for (int k = 0; k < 120; k++) { Frame("walk"); yield return null; }
                    unit.Motor.TryMove(OnGround(unit.transform.position + Quaternion.AngleAxis(-100, Vector3.up) * away * 5f));
                    for (int k = 0; k < 60; k++) { Frame("turn"); yield return null; }
                    unit.Motor.Stop();
                    for (int k = 0; k < 45; k++) { Frame("stop"); yield return null; }
                    // At work: two swings at a rock face, after the first has struck.
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    if (miner != null && miner.Pickaxe != null)
                    {
                        unit.GetComponent<NavMeshAgent>().Warp(spot);
                        unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                        biped.ResetPose();
                        var gatherer = unit.gameObject.AddComponent<Gatherer>();
                        var tool = unit.gameObject.AddComponent<EquippedTool>();
                        tool.SetDefinition(miner.Pickaxe);
                        biped.ConfigureWork(gatherer, null);
                        for (int k = 0; k < 20; k++) yield return null;
                        float swing = (miner.Pickaxe.Head - miner.Pickaxe.PrimaryGrip).magnitude;
                        var (node, depot, rock) = CaptureTools.RockFace(unit.transform.position, away, (biped.ToolHand.z + swing) * .78f, OnGround(spot - away * 3));
                        gatherer.Configure(depot, Vector3.zero);
                        gatherer.Gather(node);
                        float began = Time.time;
                        while (tool.AcceptedStrikes < 1 && Time.time - began < 30) yield return null;
                        while (tool.Progress > .05f && Time.time - began < 40) yield return null;
                        int mined = 0;
                        for (int k = 0; k < 56; k++)
                        {
                            if (tool.HandsOnTool && tool.Model != null) { Frame("mine", tool.Model); mined++; }
                            yield return null;
                        }
                        Debug.Log($"MINER_POSES {name} mining: {mined} frames, {tool.AcceptedStrikes} strikes ({tool.Status})");
                        gatherer.CancelOrder();
                        biped.ConfigureWork(null, null);
                        UnityEngine.Object.Destroy(tool);
                        UnityEngine.Object.Destroy(gatherer);
                        UnityEngine.Object.Destroy(rock);
                        UnityEngine.Object.Destroy(depot.gameObject);
                        yield return null;
                    }
                    json.Append("]}");
                    File.WriteAllText(Path.Combine(folder, $"Miner_{name}_poses.json"), json.ToString());
                    Debug.Log($"MINER_POSES {name} {frames} frames, {bones.Length} bones");
                }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
            Debug.Log("MINER_POSE_RECORD_OK " + folder);
        }
    }
}
