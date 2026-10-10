using System.Collections.Generic;
using UnityEngine;

namespace WonderGather
{
    // A boulder of the place: a rock a miner can be sent to mine by a click (S3, step 9:
    // Docs/Design/ThePhysicalBody.md). Blows of a pick's head on it break pieces off: each blow's energy is taken
    // where it lands, and when the rock has taken what a piece costs, a piece comes off there, as a loose stone
    // with its own weight, that falls and lies where it falls. (Bringing stones home is S4.)
    //
    // Until the place is set up with them (step 11), the boulders are marked when first asked for: every solid rock
    // under an object called "Boulder".
    public sealed class Boulder : MonoBehaviour
    {
        // The energy of blows that breaks a piece off (joules), a first setting; how big a piece is across (metres,
        // the least and the most); and what stone weighs (kilograms a cubic metre).
        public const float Breaks = 90, Least = .09f, Most = .14f, Density = 2600;

        public Collider Rock { get; private set; }
        // Blows it has taken, the energy taken since the last piece came off, and the pieces struck off it.
        public int Blows { get; private set; }
        public float Taken { get; private set; }
        public readonly List<Rigidbody> Stones = new List<Rigidbody>();
        // Where each blow landed on it.
        public readonly List<Vector3> Struck = new List<Vector3>();

        private static readonly List<Boulder> all = new List<Boulder>();

        // The boulders of the place.
        public static IReadOnlyList<Boulder> All()
        {
            all.RemoveAll(b => b == null);
            if (all.Count > 0) return all;
            foreach (var solid in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None))
            {
                var parent = solid.transform.parent;
                if (parent == null || parent.name != "Boulder") continue;
                var boulder = parent.GetComponent<Boulder>();
                if (boulder == null) boulder = parent.gameObject.AddComponent<Boulder>();
                boulder.Rock = solid;
                if (!all.Contains(boulder)) all.Add(boulder);
            }
            // Always in the same order: from the west, then from the south.
            all.Sort((a, b) => a.transform.position.x != b.transform.position.x ? a.transform.position.x.CompareTo(b.transform.position.x)
                : a.transform.position.z.CompareTo(b.transform.position.z));
            return all;
        }

        public static Boulder Of(Collider solid)
        {
            if (solid == null) return null;
            foreach (var boulder in All()) if (boulder.Rock == solid) return boulder;
            return null;
        }

        // A blow of this energy landed at a point of its surface (whose way out is given). The rock takes it; when it
        // has taken what a piece costs, a piece comes off there. Returns the piece, or null.
        public Rigidbody Strike(Vector3 point, Vector3 outward, float energy)
        {
            Blows++;
            Struck.Add(point);
            Taken += Mathf.Max(0, energy);
            if (Taken < Breaks) return null;
            Taken -= Breaks;
            return Loosen(point, outward);
        }

        // A piece of the rock itself, small, where the blow landed: thrown a little out and up, as a struck piece is.
        private Rigidbody Loosen(Vector3 point, Vector3 outward)
        {
            var from = Rock.GetComponent<MeshFilter>();
            var paint = Rock.GetComponent<MeshRenderer>();
            // Each piece is its own: its size and its turn come from how many came before it.
            var chance = new System.Random(Mathf.RoundToInt(transform.position.x * 131 + transform.position.z * 977) * 31 + Stones.Count * 7919);
            float Between(float a, float b) => a + (float)chance.NextDouble() * (b - a);
            float across = Between(Least, Most);
            var stone = new GameObject("Loose stone");
            var shape = from != null ? from.sharedMesh : null;
            Vector3 size = Vector3.one * across;
            if (shape != null)
            {
                var show = new GameObject("Stone");
                show.transform.SetParent(stone.transform, false);
                Vector3 whole = shape.bounds.size;
                float scale = across / Mathf.Max(whole.x, whole.y, whole.z, 1e-3f);
                show.transform.localScale = Vector3.one * scale;
                show.transform.localPosition = -shape.bounds.center * scale;
                show.AddComponent<MeshFilter>().sharedMesh = shape;
                if (paint != null) show.AddComponent<MeshRenderer>().sharedMaterials = paint.sharedMaterials;
                size = whole * scale;
            }
            // A stone lies as a block of about its own size lies: it does not roll on.
            var solid = stone.AddComponent<BoxCollider>();
            solid.size = size * .8f;
            if (Rock.sharedMaterial != null) solid.sharedMaterial = Rock.sharedMaterial;
            outward = outward.sqrMagnitude > 1e-6f ? outward.normalized : Vector3.up;
            stone.transform.SetPositionAndRotation(point + outward * (across * .6f), Quaternion.Euler(Between(0, 360), Between(0, 360), Between(0, 360)));
            var body = stone.AddComponent<Rigidbody>();
            // A rounded piece fills about half the block around it.
            body.mass = Density * size.x * size.y * size.z * .5f;
            body.angularDamping = 1.5f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Vector3 aside = Vector3.Cross(Vector3.up, outward);
            body.linearVelocity = outward * Between(.7f, 1.2f) + Vector3.up * Between(.5f, 1.1f) + aside * Between(-.4f, .4f);
            body.angularVelocity = new Vector3(Between(-4, 4), Between(-4, 4), Between(-4, 4));
            // It does not push against the rock it came from while it is still inside its skin.
            Physics.IgnoreCollision(solid, Rock, true);
            stone.AddComponent<LooseStone>().Came(this, solid);
            Stones.Add(body);
            return body;
        }

        private void OnDestroy() => all.Remove(this);
    }
}
