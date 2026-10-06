using System;
using UnityEngine;

namespace WonderGather.Tests
{
    // Calls back when a frame's bodies have all been posed (after every LateUpdate): for a picture or a measure that
    // must see the body and what the physics moved in the same frame. A batch run cannot wait for the end of a frame.
    [DefaultExecutionOrder(30000)]
    public sealed class AfterEverything : MonoBehaviour
    {
        public Action Then;
        private void LateUpdate() => Then?.Invoke();
    }
}
