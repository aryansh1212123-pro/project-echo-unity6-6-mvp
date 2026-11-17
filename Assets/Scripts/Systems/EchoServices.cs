using System;
using System.Linq;
using UnityEngine;

namespace ProjectEcho
{
    public static class EchoEvents
    {
        public static Action<EchoMemory> MemoryCreated;
        public static Action<TraitKey, float, string> TraitChanged;
        public static Action<EchoActionState, string> BehaviorChanged;
        public static Action ProfileChanged;
    }

    public sealed class MemoryManager
    {
        private readonly EchoProfile profile;
        private readonly EchoTuning tuning;
        public MemoryManager(EchoProfile profile, EchoTuning tuning) { this.profile = profile; this.tuning = tuning; }

        public EchoMemory Remember(MemoryKind kind, MemoryTag tag, string eventType, string emotion, string location, string summary, float importance, params string[] participants)
        {
            var memory = new EchoMemory
            {
                id = Guid.NewGuid().ToString("N"), kind = kind, tag = tag, eventType = eventType,
                emotionalValue = emotion, location = location, summary = summary,
                timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), participants = participants ?? Array.Empty<string>(),
                importance = Mathf.Clamp01(importance), strength = profile.mutations.Contains(MutationId.DeepMemory) ? 1.15f : 1f
            };
            profile.memories.Add(memory);
            int cap = tuning != null ? Mathf.Max(8, tuning.memoryLimit) : 80;
            while (profile.memories.Count > cap)
            {
                int weakest = profile.memories.FindIndex(m => m != null && m.strength < .15f);
                if (weakest < 0) weakest = 0;
                profile.memories.RemoveAt(weakest);
            }
            EchoEvents.MemoryCreated?.Invoke(memory);
            EchoEvents.ProfileChanged?.Invoke();
            return memory;
        }

        public float WeightOf(MemoryTag tag)
        {
            return profile.memories.Where(m => m != null && m.tag == tag)
                .Sum(m => Mathf.Clamp01(m.importance * m.strength));
        }

        public bool HasTag(MemoryTag tag) => profile.memories.Any(m => m != null && m.tag == tag);
    }

    public sealed class WorldKnowledgeDatabase
    {
        private readonly EchoGameState state;
        public WorldKnowledgeDatabase(EchoGameState state) { this.state = state; state.EnsureValid(); }

        public void RecordVisit(string location, MemoryTag tag)
        {
            WorldKnowledgeEntry entry = GetOrCreate(location);
            entry.visits++;
            entry.lastVisitedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!entry.knownTags.Contains(tag)) entry.knownTags.Add(tag);
            EchoEvents.ProfileChanged?.Invoke();
        }

        public bool Knows(string location, MemoryTag tag)
        {
            WorldKnowledgeEntry entry = Find(location);
            return entry != null && entry.knownTags.Contains(tag);
        }

        public int VisitCount(string location)
        {
            WorldKnowledgeEntry entry = Find(location);
            return entry == null ? 0 : entry.visits;
        }

        private WorldKnowledgeEntry GetOrCreate(string location)
        {
            location = string.IsNullOrWhiteSpace(location) ? "Unknown" : location;
            WorldKnowledgeEntry entry = Find(location);
            if (entry != null) return entry;
            entry = new WorldKnowledgeEntry { location = location, firstKnownUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            state.worldKnowledge.Add(entry);
            return entry;
        }

        private WorldKnowledgeEntry Find(string location)
        {
            if (state.worldKnowledge == null) state.worldKnowledge = new System.Collections.Generic.List<WorldKnowledgeEntry>();
            return state.worldKnowledge.FirstOrDefault(e => e != null && string.Equals(e.location, location, StringComparison.OrdinalIgnoreCase));
        }
    }

    public sealed class TraitManager
    {
        private readonly EchoProfile profile;
        public TraitManager(EchoProfile profile) { this.profile = profile; }
        public void Experience(TraitKey trait, float delta, string reason)
        {
            if (Mathf.Abs(delta) < .01f) return;
            profile.traits.Set(trait, profile.traits.Get(trait) + delta);
            EchoEvents.TraitChanged?.Invoke(trait, delta, reason);
            EchoEvents.ProfileChanged?.Invoke();
        }
    }

    public sealed class NeedManager
    {
        private readonly EchoProfile profile;
        private readonly EchoTuning tuning;
        private float elapsed;
        public NeedManager(EchoProfile profile, EchoTuning tuning) { this.profile = profile; this.tuning = tuning; }

        public void Tick(float deltaTime, bool onExpedition, EchoActionState currentAction)
        {
            elapsed += deltaTime;
            float interval = tuning != null ? Mathf.Max(.2f, tuning.needsTickSeconds) : 5f;
            while (elapsed >= interval)
            {
                elapsed -= interval;
                profile.needs.hunger -= tuning != null ? tuning.hungerDecayPerTick : .8f;
                profile.needs.energy -= tuning != null ? tuning.energyDecayPerTick : .55f;
                profile.needs.social -= tuning != null ? tuning.socialDecayPerTick : .4f;
                profile.needs.curiosity -= tuning != null ? tuning.curiosityDecayPerTick : .3f;
                profile.needs.safety += onExpedition ? -.15f : .15f;
                if (currentAction == EchoActionState.Sleep) profile.needs.energy += 2.5f;
                if (currentAction == EchoActionState.Eat) profile.needs.hunger += 4f;
                if (currentAction == EchoActionState.Socialize) profile.needs.social += 2f;
                if (currentAction == EchoActionState.Explore || currentAction == EchoActionState.Investigate) profile.needs.curiosity += 1.2f;
                if (profile.mutations.Contains(MutationId.Regeneration)) profile.needs.energy += .45f;
                profile.needs.EnsureValid();
            }
        }

        public void Soothe(float safety, float social)
        {
            profile.needs.safety += safety; profile.needs.social += social; profile.needs.EnsureValid();
            EchoEvents.ProfileChanged?.Invoke();
        }
        public void Feed(float amount) { profile.needs.hunger += amount; profile.needs.EnsureValid(); EchoEvents.ProfileChanged?.Invoke(); }
        public void Rest(float amount) { profile.needs.energy += amount; profile.needs.safety += amount * .15f; profile.needs.EnsureValid(); EchoEvents.ProfileChanged?.Invoke(); }
        public void Play(float amount) { profile.needs.social += amount; profile.needs.curiosity += amount * .6f; profile.needs.EnsureValid(); EchoEvents.ProfileChanged?.Invoke(); }
    }

    public sealed class RelationshipManager
    {
        private readonly EchoProfile profile;
        private readonly EchoRelationship relationship;
        public RelationshipManager(EchoProfile profile) { this.profile = profile; relationship = profile.playerRelationship; }
        public void SharedCare(float trust, float friendship, float respect = 0)
        {
            if (profile.mutations.Contains(MutationId.NeuralLink)) { trust *= 1.4f; friendship *= 1.2f; }
            relationship.trust = Mathf.Clamp(relationship.trust + trust, 0, 100);
            relationship.friendship = Mathf.Clamp(relationship.friendship + friendship, 0, 100);
            relationship.respect = Mathf.Clamp(relationship.respect + respect, 0, 100);
            relationship.EnsureValid(); EchoEvents.ProfileChanged?.Invoke();
        }
        public void SetFear(float delta) { relationship.fear = Mathf.Clamp(relationship.fear + delta, 0, 100); relationship.EnsureValid(); }
    }

    public sealed class MutationManager
    {
        private readonly EchoProfile profile;
        public MutationManager(EchoProfile profile) { this.profile = profile; }
        public bool Has(MutationId id) => profile.mutations.Contains(id);
        public bool Unlock(MutationData data, out string result)
        {
            if (Has(data.id)) { result = "Already expressed"; return false; }
            if (profile.crystalShards < data.shardCost || profile.bioSamples < data.sampleCost) { result = "Not enough materials"; return false; }
            profile.crystalShards -= data.shardCost; profile.bioSamples -= data.sampleCost;
            profile.mutations.Add(data.id);
            if (data.id == MutationId.NeuralCreativity) profile.traits.creativity = Mathf.Clamp(profile.traits.creativity + 2, 0, 100);
            if (data.id == MutationId.DeepMemory)
                foreach (var memory in profile.memories) if (memory != null) memory.strength = Mathf.Clamp01(memory.strength + .2f);
            EchoEvents.ProfileChanged?.Invoke(); result = data.title + " expressed"; return true;
        }
    }
}
