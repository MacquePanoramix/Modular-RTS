using UnityEngine;

namespace WonderGather
{
    // What the player asks of the camera this frame. RtsInput fills it from devices;
    // tests construct it directly. Pointer values are screen pixels, bottom-left origin.
    public struct CameraIntent
    {
        public Vector2 Pan;            // WASD/arrows: x right, y forward.
        public float Rotate;           // Q/E: Strategy rotation, Explore down/up.
        public float Zoom;             // Wheel delta (positive = in/forward).
        public Vector2 Pointer;
        public Vector2 PointerDelta;
        public bool PointerInWorld;    // The pointer may aim world actions (not over interface).
        public bool PointerInWindow;   // The pointer is inside a focused game window (edge scrolling).
        public bool Look;              // Right mouse held: Explore looks around.
        public bool LookDragging;      // The right-button press has moved past the click threshold.
        public bool Middle;            // Middle mouse held.
        public bool AltOrbit;          // Alt + left mouse held: Explore orbits.
        public bool Alt, Fast, Slow;
        public bool Toggle, Focus;
    }
}
