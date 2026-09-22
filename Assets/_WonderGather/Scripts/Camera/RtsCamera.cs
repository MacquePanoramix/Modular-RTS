using UnityEngine;

namespace WonderGather
{
    public sealed class RtsCamera : MonoBehaviour
    {
        [SerializeField] private RtsInput input;
        [SerializeField] private SelectionController selection;
        [SerializeField] private float panSpeed = 15, rotationSpeed = 80, zoomSensitivity = .002f, smoothing = 10;
        [SerializeField] private float minDistance = 5, maxDistance = 42, boundary = 25;
        [SerializeField] private bool terrainAware;
        private Vector3 target, current;
        private float yaw, currentYaw, distance = 23, currentDistance = 23;
        public void Configure(RtsInput source, SelectionController selected) { input = source; selection = selected; }

        private void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            yaw += input.Rotate * rotationSpeed * dt;
            var rotation = Quaternion.Euler(0, yaw, 0);
            var direction = new Vector3(input.Pan.x, 0, input.Pan.y);
            target += rotation * direction * (panSpeed * Mathf.Lerp(.35f, 1.6f, distance / maxDistance) * dt);
            if (input.FocusPressed && selection.HasSelection) target = selection.Center;
            target = new Vector3(Mathf.Clamp(target.x, -boundary, boundary), 0, Mathf.Clamp(target.z, -boundary, boundary));
            if(terrainAware && Physics.Raycast(new Vector3(target.x,40,target.z),Vector3.down,out var ground,80,1<<6,QueryTriggerInteraction.Ignore))
                target.y=ground.point.y+1.1f;
            distance = Mathf.Clamp(distance * Mathf.Exp(-input.Zoom * zoomSensitivity), minDistance, maxDistance);
            float blend = 1 - Mathf.Exp(-smoothing * dt);
            current = Vector3.Lerp(current, target, blend);
            currentYaw = Mathf.LerpAngle(currentYaw, yaw, blend);
            currentDistance = Mathf.Lerp(currentDistance, distance, blend);
            float pitch = Mathf.Lerp(32, 62, Mathf.InverseLerp(minDistance, maxDistance, currentDistance));
            var viewRotation = Quaternion.Euler(pitch, currentYaw, 0);
            Vector3 position=current-viewRotation*Vector3.forward*currentDistance;
            if(terrainAware && Physics.Raycast(new Vector3(position.x,40,position.z),Vector3.down,out var below,80,1<<6,QueryTriggerInteraction.Ignore))
                position.y=Mathf.Max(position.y,below.point.y+.6f);
            transform.SetPositionAndRotation(position,terrainAware?Quaternion.LookRotation(current-position):viewRotation);
        }
    }
}
