using System;
using UnityEngine;

namespace WonderGather
{
    [CreateAssetMenu(menuName = "Wonder Gather/Tool")]
    public sealed class ToolDefinition : ScriptableObject
    {
        [SerializeField] private string id, displayName;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 primaryGrip, secondaryGrip, head;
        [SerializeField] private float headRadius = .055f;
        // The handle's radius where each hand grips it (x: the primary grip, y: the secondary). A hand closes
        // round a handle of that thickness. The first pickaxe's shaft is 65 mm across.
        [SerializeField] private Vector2 gripRadii = new Vector2(.0325f, .0325f);
        public string Id => id;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public Vector3 PrimaryGrip => primaryGrip;
        public Vector3 SecondaryGrip => secondaryGrip;
        public Vector3 Head => head;
        public float HeadRadius => headRadius;
        // Hand 0 is the left, on the secondary grip; hand 1 the right, on the primary.
        public float GripRadius(int hand) => hand == 0 ? gripRadii.y : gripRadii.x;
        public bool IsValid => ValidText(id) && ValidText(displayName)
            && prefab != null && UnitScale(prefab.transform.localScale)
            && Finite(primaryGrip) && Finite(secondaryGrip) && Finite(head)
            && (primaryGrip-secondaryGrip).sqrMagnitude > .0001f
            && float.IsFinite(headRadius) && headRadius > 0 && headRadius <= .2f;
        private static bool Finite(Vector3 p) => float.IsFinite(p.x) && float.IsFinite(p.y) && float.IsFinite(p.z);
        private static bool UnitScale(Vector3 p) => Finite(p) && (p-Vector3.one).sqrMagnitude<.000001f;
        private static bool ValidText(string value)
        {
            if(string.IsNullOrWhiteSpace(value) || value.Length>64) return false;
            foreach(char c in value) if(char.IsControl(c)) return false;
            return true;
        }
        public void Configure(string key, string label, GameObject model, Vector3 primary, Vector3 secondary, Vector3 strikingHead, float radius, Vector2? handle = null)
        {
            if (handle.HasValue && !(handle.Value.x > .002f && handle.Value.y > .002f && handle.Value.x < .1f && handle.Value.y < .1f))
                throw new ArgumentException("A tool's handle needs a thickness a hand can close on.");
            if (!ValidText(key) || !ValidText(label) || model == null || !UnitScale(model.transform.localScale)
                || !Finite(primary) || !Finite(secondary) || !Finite(strikingHead)
                || (primary-secondary).sqrMagnitude <= .0001f || !float.IsFinite(radius) || radius <= 0 || radius > .2f)
                throw new ArgumentException("A tool needs an identity, model, distinct grips and a finite striking head.");
            id=key; displayName=label; prefab=model; primaryGrip=primary; secondaryGrip=secondary; head=strikingHead; headRadius=radius;
            if (handle.HasValue) gripRadii=handle.Value;
        }
    }
}
