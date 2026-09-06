using UnityEngine;

namespace WonderGather
{
    public sealed class WandererHud : MonoBehaviour
    {
        [SerializeField] private SelectionController selection;
        public void Configure(SelectionController value) => selection = value;
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(18, 18, 430, 138), GUI.skin.box);
            GUILayout.Label("WONDER GATHER  /  THE WANDERER");
            GUILayout.Label("WASD / Arrows: pan     Q / E: rotate     Wheel: zoom");
            GUILayout.Label("Left click: select     Right click: move");
            GUILayout.Label("Esc: deselect     F: focus selected unit");
            GUILayout.Space(8);
            GUILayout.Label(selection.Status);
            GUILayout.EndArea();
        }
    }
}
