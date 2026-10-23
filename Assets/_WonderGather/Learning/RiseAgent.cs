using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace WonderGather
{
    // The third path (Luis, October 22, 2026: "I approve we try the third path"; Docs/Research/reports, "Getting up by
    // its own joints"): what learns the moment from hands and feet to the squat. In the stages it learns at it is told
    // what the body senses (OwnRiser.Sense) and says six corrections (OwnRiser.Learned): the trunk's and the head's bow
    // and the twist, how high the hips are held, where the weight is meant to be. Everything it says is done by the
    // body's own joints. Help that exists only while it learns is allowed (Luis, October 22); in the game it has none.
    public sealed class RiseAgent : Agent
    {
        public const string Behaviour = "SmallRise";
        public const int Says = OwnRiser.Learns, Senses = OwnRiser.Senses + Says;
        // How far each correction goes at most: degrees for the bows and the twist, a share of the hips' standing
        // height, metres for the weight's place.
        public static readonly float[] By = { 20, 30, 15, .12f, .08f, .05f };
        // How much a change in what it says costs, a decision (it learns not to jerk); and how far each decision moves
        // what it says towards what it asks for (half way: a body does not change its mind all at once; at first, asking
        // at random, it cost more than standing was worth, October 23).
        public static float Jerk = .01f, Follows = .5f;

        public OwnBody Body;
        private readonly float[] said = new float[Says];
        private readonly List<float> sensed = new List<float>(Senses);

        // An agent on this body: its behaviour's numbers set before it wakes (a model to run, or none: it is taught).
        public static RiseAgent On(GameObject on, OwnBody body, Unity.InferenceEngine.ModelAsset model = null)
        {
            var behaviour = on.GetComponent<BehaviorParameters>();
            if (behaviour == null) behaviour = on.AddComponent<BehaviorParameters>();
            behaviour.BehaviorName = Behaviour;
            behaviour.BrainParameters.VectorObservationSize = Senses;
            behaviour.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(Says);
            if (model != null) behaviour.Model = model;
            var agent = on.GetComponent<RiseAgent>();
            if (agent == null) agent = on.AddComponent<RiseAgent>();
            agent.Body = body;
            agent.MaxStep = 0;
            return agent;
        }

        // Nothing said yet: as searched.
        public void Afresh()
        {
            for (int i = 0; i < Says; i++) said[i] = 0;
            if (Body != null && Body.Riser != null) Body.Riser.Learned = null;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensed.Clear();
            if (Body != null && Body.Riser != null) Body.Riser.Sense(sensed);
            while (sensed.Count < OwnRiser.Senses) sensed.Add(0);
            // (A number that is none, of a body gone wild, is told as nothing: the trainer takes no such number.)
            for (int i = 0; i < OwnRiser.Senses; i++) sensor.AddObservation(float.IsNaN(sensed[i]) || float.IsInfinity(sensed[i]) ? 0 : sensed[i]);
            for (int i = 0; i < Says; i++) sensor.AddObservation(said[i]);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var a = actions.ContinuousActions;
            float changed = 0;
            for (int i = 0; i < Says; i++)
            {
                float now = Mathf.Lerp(said[i], Mathf.Clamp(a[i], -1, 1), Follows);
                changed += (now - said[i]) * (now - said[i]);
                said[i] = now;
            }
            AddReward(-Jerk * changed / Says);
            var riser = Body != null ? Body.Riser : null;
            if (riser == null) return;
            if (riser.Learned == null || riser.Learned.Length != Says) riser.Learned = new float[Says];
            for (int i = 0; i < Says; i++) riser.Learned[i] = said[i] * By[i];
        }

        // Asked without a teacher or a model: it says nothing, and the body does as searched.
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var a = actionsOut.ContinuousActions;
            for (int i = 0; i < a.Length; i++) a[i] = 0;
        }
    }
}
