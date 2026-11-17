using System.Collections.Generic;
using UnityEngine;

namespace ProjectEcho
{
    public sealed class EchoDecision
    {
        public EchoActionState state;
        public string explanation;
        public float utility;
        public Vector3 target;
    }

    public sealed class BehaviorPlanner
    {
        private readonly EchoProfile profile;
        private readonly MemoryManager memories;
        public BehaviorPlanner(EchoProfile profile, MemoryManager memories) { this.profile = profile; this.memories = memories; }

        public EchoDecision Decide(Vector3 currentPosition, bool expedition, int decisionIndex)
        {
            var options = new List<EchoDecision>();
            float energyNeed = 100 - profile.needs.energy;
            float hungerNeed = 100 - profile.needs.hunger;
            float socialNeed = 100 - profile.needs.social;
            float curiosityNeed = 100 - profile.needs.curiosity;
            float safetyNeed = 100 - profile.needs.safety;
            float predatorMemory = memories.WeightOf(MemoryTag.Predator);

            Add(options, EchoActionState.Sleep, "Energy is low; the Echo seeks a quiet place to rest.",
                energyNeed * .88f + (100 - profile.traits.courage) * .08f);
            Add(options, EchoActionState.Eat, "Hunger is shaping the next decision.", hungerNeed * .86f);
            Add(options, EchoActionState.Escape, predatorMemory > 0 ? "A remembered danger makes open space feel safer." : "Safety is the strongest unmet need.",
                safetyNeed * (.52f + profile.traits.selfPreservation / 200f) + predatorMemory * 8f);
            Add(options, EchoActionState.Socialize, "The Echo is seeking a familiar presence.", socialNeed * (.48f + profile.traits.loyalty / 170f));
            Add(options, EchoActionState.Investigate, "Curiosity is pulling attention toward an unfamiliar signal.",
                curiosityNeed * (.48f + profile.traits.curiosity / 150f) + profile.traits.creativity * .12f);
            Add(options, EchoActionState.Gather, "The Echo is looking for useful bioglass and samples.", 24f + profile.traits.creativity * .12f);
            Add(options, EchoActionState.Explore, "The Echo is mapping a new path through the sanctuary.",
                20f + curiosityNeed * .25f + profile.traits.curiosity * .20f - predatorMemory * 1.5f);
            Add(options, EchoActionState.Follow, "A trusted bond draws the Echo closer to its creator.",
                expedition ? 22f + profile.traits.loyalty * .20f : 8f + profile.playerRelationship.trust * .18f);
            Add(options, EchoActionState.Assist, "The Echo offers help after remembering care it received.",
                memories.WeightOf(MemoryTag.Rescue) > 0 ? 17f + profile.traits.loyalty * .18f : 2f);
            Add(options, EchoActionState.Idle, "Nothing urgent is asking for attention; the Echo watches and listens.", 14f);

            options.Sort((a, b) => b.utility.CompareTo(a.utility));
            EchoDecision chosen = options[0];
            chosen.target = TargetFor(chosen.state, currentPosition, decisionIndex, expedition);
            return chosen;
        }

        private static void Add(List<EchoDecision> list, EchoActionState state, string reason, float utility)
        {
            list.Add(new EchoDecision { state = state, explanation = reason, utility = Mathf.Max(0, utility) });
        }

        private static Vector3 TargetFor(EchoActionState state, Vector3 current, int index, bool expedition)
        {
            if (state == EchoActionState.Idle || state == EchoActionState.Sleep) return current;
            float radius = expedition ? 10f : 5.6f;
            float angle = (index * 137.50776f + (int)state * 43f) * Mathf.Deg2Rad;
            if (state == EchoActionState.Follow || state == EchoActionState.Assist) radius = 2.1f;
            if (state == EchoActionState.Escape) radius = 6.5f;
            return new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }
    }

    public sealed class EchoBrain
    {
        private readonly BehaviorPlanner planner;
        private readonly EchoTuning tuning;
        private float elapsed;
        private int decisionIndex;
        public EchoDecision Current { get; private set; }
        public EchoBrain(EchoProfile profile, MemoryManager memories, EchoTuning tuning)
        {
            planner = new BehaviorPlanner(profile, memories); this.tuning = tuning;
        }

        public bool Tick(float deltaTime, Vector3 position, bool expedition)
        {
            elapsed += deltaTime;
            float interval = tuning != null ? Mathf.Max(.2f, tuning.brainDecisionSeconds) : 2.8f;
            if (Current != null && elapsed < interval) return false;
            elapsed = 0; decisionIndex++;
            Current = planner.Decide(position, expedition, decisionIndex);
            EchoEvents.BehaviorChanged?.Invoke(Current.state, Current.explanation);
            return true;
        }
    }
}
