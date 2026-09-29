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
        public string Id => id;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public Vector3 PrimaryGrip => primaryGrip;
        public Vector3 SecondaryGrip => secondaryGrip;
        public Vector3 Head => head;
        public float HeadRadius => headRadius;
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
        public void Configure(string key, string label, GameObject model, Vector3 primary, Vector3 secondary, Vector3 strikingHead, float radius)
        {
            if (!ValidText(key) || !ValidText(label) || model == null || !UnitScale(model.transform.localScale)
                || !Finite(primary) || !Finite(secondary) || !Finite(strikingHead)
                || (primary-secondary).sqrMagnitude <= .0001f || !float.IsFinite(radius) || radius <= 0 || radius > .2f)
                throw new ArgumentException("A tool needs an identity, model, distinct grips and a finite striking head.");
            id=key; displayName=label; prefab=model; primaryGrip=primary; secondaryGrip=secondary; head=strikingHead; headRadius=radius;
        }
    }
}
