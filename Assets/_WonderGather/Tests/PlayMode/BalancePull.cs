using System;
using UnityEngine;

namespace WonderGather.Tests
{
    // Something that pulls a body, for the bench and the tests: it gives its pull to the body's balance at every step
    // of the physics, before the balance reads it.
    [DefaultExecutionOrder(100)]
    internal sealed class BalancePull : MonoBehaviour
    {
        public PhysicalBalance balance;
        public Func<Vector3> force, at;
        private void FixedUpdate() { if (balance != null && force != null) balance.Push(force(), at()); }
    }
}
