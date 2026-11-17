using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEcho
{
    public enum TraitKey { Courage, Curiosity, Loyalty, Creativity, SelfPreservation }
    public enum NeedKey { Energy, Hunger, Safety, Social, Curiosity }
    public enum MemoryKind { Personal, Environmental, Social }
    public enum MemoryTag { Rescue, Predator, Ruin, Discovery, Care, Social, Shelter, Gathering }
    public enum EchoActionState { Idle, Explore, Gather, Eat, Sleep, Follow, Investigate, Assist, Escape, Socialize }
    public enum MutationId
    {
        ThermalVision, Regeneration, EchoSense, AdaptiveSkin, NeuralLink, DeepMemory,
        EnhancedHearing, Bioluminescence, DefensiveShell, NeuralCreativity
    }

    [Serializable]
    public sealed class EchoTraits
    {
        [Range(0, 100)] public float courage = 48;
        [Range(0, 100)] public float curiosity = 62;
        [Range(0, 100)] public float loyalty = 52;
        [Range(0, 100)] public float creativity = 45;
        [Range(0, 100)] public float selfPreservation = 55;

        public float Get(TraitKey key)
        {
            switch (key)
            {
                case TraitKey.Courage: return courage;
                case TraitKey.Curiosity: return curiosity;
                case TraitKey.Loyalty: return loyalty;
                case TraitKey.Creativity: return creativity;
                default: return selfPreservation;
            }
        }

        public void Set(TraitKey key, float value)
        {
            value = Mathf.Clamp(value, 0f, 100f);
            switch (key)
            {
                case TraitKey.Courage: courage = value; break;
                case TraitKey.Curiosity: curiosity = value; break;
                case TraitKey.Loyalty: loyalty = value; break;
                case TraitKey.Creativity: creativity = value; break;
                default: selfPreservation = value; break;
            }
        }

        public void EnsureValid()
        {
            courage = Mathf.Clamp(courage, 0, 100);
            curiosity = Mathf.Clamp(curiosity, 0, 100);
            loyalty = Mathf.Clamp(loyalty, 0, 100);
            creativity = Mathf.Clamp(creativity, 0, 100);
            selfPreservation = Mathf.Clamp(selfPreservation, 0, 100);
        }
    }

    [Serializable]
    public sealed class EchoNeeds
    {
        [Range(0, 100)] public float energy = 82;
        [Range(0, 100)] public float hunger = 76;
        [Range(0, 100)] public float safety = 88;
        [Range(0, 100)] public float social = 65;
        [Range(0, 100)] public float curiosity = 74;

        public float Get(NeedKey key)
        {
            switch (key)
            {
                case NeedKey.Energy: return energy;
                case NeedKey.Hunger: return hunger;
                case NeedKey.Safety: return safety;
                case NeedKey.Social: return social;
                default: return curiosity;
            }
        }

        public void Set(NeedKey key, float value)
        {
            value = Mathf.Clamp(value, 0f, 100f);
            switch (key)
            {
                case NeedKey.Energy: energy = value; break;
                case NeedKey.Hunger: hunger = value; break;
                case NeedKey.Safety: safety = value; break;
                case NeedKey.Social: social = value; break;
                default: curiosity = value; break;
            }
        }

        public void EnsureValid()
        {
            energy = Mathf.Clamp(energy, 0, 100);
            hunger = Mathf.Clamp(hunger, 0, 100);
            safety = Mathf.Clamp(safety, 0, 100);
            social = Mathf.Clamp(social, 0, 100);
            curiosity = Mathf.Clamp(curiosity, 0, 100);
        }
    }

    [Serializable]
    public sealed class EchoRelationship
    {
        public string subjectId = "player";
        public string subjectName = "Creator";
        [Range(0, 100)] public float trust = 24;
        [Range(0, 100)] public float friendship = 18;
        [Range(0, 100)] public float fear = 0;
        [Range(0, 100)] public float respect = 10;

        public void EnsureValid()
        {
            trust = Mathf.Clamp(trust, 0, 100);
            friendship = Mathf.Clamp(friendship, 0, 100);
            fear = Mathf.Clamp(fear, 0, 100);
            respect = Mathf.Clamp(respect, 0, 100);
        }
    }

    [Serializable]
    public sealed class EchoMemory
    {
        public string id;
        public MemoryKind kind;
        public MemoryTag tag;
        public string eventType;
        public string emotionalValue;
        public string location;
        public long timestampUtc;
        public string[] participants;
        [Range(0, 1)] public float importance = 0.5f;
        [Range(0, 1)] public float strength = 1f;
        public string summary;
    }

    [Serializable]
    public sealed class EchoProfile
    {
        public string id;
        public string displayName = "Luma";
        public string species = "Starter Echo";
        public EchoTraits traits = new EchoTraits();
        public EchoNeeds needs = new EchoNeeds();
        public EchoRelationship playerRelationship = new EchoRelationship();
        public List<EchoMemory> memories = new List<EchoMemory>();
        public List<MutationId> mutations = new List<MutationId>();
        public int crystalShards = 8;
        public int bioSamples = 2;
        public int insight = 0;
        public int day = 1;
        public int expeditionsCompleted = 0;
        public string sanctuaryName = "The Quiet Between";

        public void EnsureValid()
        {
            if (string.IsNullOrWhiteSpace(id)) id = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "Luma";
            if (traits == null) traits = new EchoTraits();
            if (needs == null) needs = new EchoNeeds();
            if (playerRelationship == null) playerRelationship = new EchoRelationship();
            if (memories == null) memories = new List<EchoMemory>();
            if (mutations == null) mutations = new List<MutationId>();
            traits.EnsureValid();
            needs.EnsureValid();
            playerRelationship.EnsureValid();
            crystalShards = Mathf.Max(0, crystalShards);
            bioSamples = Mathf.Max(0, bioSamples);
            insight = Mathf.Max(0, insight);
            day = Mathf.Max(1, day);
            expeditionsCompleted = Mathf.Max(0, expeditionsCompleted);
        }
    }

    [Serializable]
    public sealed class WorldKnowledgeEntry
    {
        public string location;
        public int visits;
        public long firstKnownUtc;
        public long lastVisitedUtc;
        public List<MemoryTag> knownTags = new List<MemoryTag>();
    }

    [Serializable]
    public sealed class EchoGameState
    {
        public int schemaVersion = 2;
        public EchoProfile echo = new EchoProfile();
        public List<WorldKnowledgeEntry> worldKnowledge = new List<WorldKnowledgeEntry>();
        public long lastSavedUtc;

        public void EnsureValid()
        {
            if (echo == null) echo = new EchoProfile();
            if (worldKnowledge == null) worldKnowledge = new List<WorldKnowledgeEntry>();
            echo.EnsureValid();
            foreach (var entry in worldKnowledge)
            {
                if (entry == null) continue;
                if (entry.knownTags == null) entry.knownTags = new List<MemoryTag>();
                if (string.IsNullOrWhiteSpace(entry.location)) entry.location = "Unknown";
            }
        }
    }
}
