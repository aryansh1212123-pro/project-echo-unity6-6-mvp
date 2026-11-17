using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEcho
{
    [Serializable]
    public sealed class EncounterChoiceData
    {
        public string label;
        [TextArea] public string consequence;
        public TraitKey favoredTrait;
        [Range(0, 100)] public int difficulty = 48;
        public MemoryTag memoryTag;
        public string emotion = "Wonder";
        [Range(0, 1)] public float importance = 0.55f;
        public int shards = 1;
        public int samples = 0;
        public int insight = 1;
    }

    [Serializable]
    public sealed class EncounterData
    {
        public string id;
        public string title;
        public string location;
        [TextArea] public string description;
        public MemoryKind memoryKind = MemoryKind.Environmental;
        public EncounterChoiceData first = new EncounterChoiceData();
        public EncounterChoiceData second = new EncounterChoiceData();
    }

    [Serializable]
    public sealed class MutationData
    {
        public MutationId id;
        public string title;
        [TextArea] public string description;
        public int shardCost = 4;
        public int sampleCost = 1;
    }

    [CreateAssetMenu(fileName = "EchoTuning", menuName = "Project Echo/Echo Tuning")]
    public sealed class EchoTuning : ScriptableObject
    {
        [Header("Simulation")]
        [Min(0.2f)] public float brainDecisionSeconds = 2.8f;
        [Min(0.2f)] public float needsTickSeconds = 5f;
        [Min(8)] public int memoryLimit = 80;
        [Min(0.01f)] public float movementSpeed = 1.55f;
        public float hungerDecayPerTick = 0.8f;
        public float energyDecayPerTick = 0.55f;
        public float socialDecayPerTick = 0.4f;
        public float curiosityDecayPerTick = 0.3f;
        [Header("Playable expedition content")]
        public List<EncounterData> encounters = new List<EncounterData>();
        public List<MutationData> mutations = new List<MutationData>();

        public void EnsureDefaults()
        {
            if (encounters == null) encounters = new List<EncounterData>();
            if (mutations == null) mutations = new List<MutationData>();
            if (encounters.Count == 0) CreateDefaultEncounters();
            if (mutations.Count == 0) CreateDefaultMutations();
        }

        private void CreateDefaultEncounters()
        {
            encounters.Add(new EncounterData
            {
                id = "signal_under_roots", title = "A Signal Under the Roots", location = "Mossglass Ruins",
                description = "A low note trembles through the crystal floor. The signal answers when your Echo steps closer.",
                memoryKind = MemoryKind.Environmental,
                first = new EncounterChoiceData { label = "Follow the signal", consequence = "Curiosity guides the Echo into the old archive.", favoredTrait = TraitKey.Curiosity, difficulty = 50, memoryTag = MemoryTag.Ruin, emotion = "Wonder", importance = .72f, shards = 1, samples = 0, insight = 2 },
                second = new EncounterChoiceData { label = "Listen from safety", consequence = "The Echo studies the pattern before approaching.", favoredTrait = TraitKey.SelfPreservation, difficulty = 42, memoryTag = MemoryTag.Ruin, emotion = "Cautious wonder", importance = .52f, shards = 2, samples = 0, insight = 1 }
            });
            encounters.Add(new EncounterData
            {
                id = "quiet_between_trees", title = "The Quiet Between Trees", location = "Glassroot Canopy",
                description = "Pale leaves open in the dark. Something small is caught beneath a fallen crystal branch.",
                memoryKind = MemoryKind.Social,
                first = new EncounterChoiceData { label = "Stay and help", consequence = "The Echo frees the trembling creature and waits until it is safe.", favoredTrait = TraitKey.Loyalty, difficulty = 46, memoryTag = MemoryTag.Rescue, emotion = "Tenderness", importance = .82f, shards = 0, samples = 1, insight = 2 },
                second = new EncounterChoiceData { label = "Call softly, then move on", consequence = "The Echo offers a gentle signal without risking the unstable branch.", favoredTrait = TraitKey.Creativity, difficulty = 40, memoryTag = MemoryTag.Social, emotion = "Hope", importance = .58f, shards = 1, samples = 1, insight = 1 }
            });
            encounters.Add(new EncounterData
            {
                id = "the_glass_crossing", title = "The Glass Crossing", location = "Singing Ravine",
                description = "A narrow bridge of translucent roots spans a blue-lit drop. Wind carries a familiar scent from the other side.",
                memoryKind = MemoryKind.Personal,
                first = new EncounterChoiceData { label = "Cross together", consequence = "Your presence steadies the Echo as it crosses the singing roots.", favoredTrait = TraitKey.Courage, difficulty = 48, memoryTag = MemoryTag.Rescue, emotion = "Trust", importance = .78f, shards = 1, samples = 0, insight = 2 },
                second = new EncounterChoiceData { label = "Find another path", consequence = "The Echo maps a slower route and remembers the safer approach.", favoredTrait = TraitKey.SelfPreservation, difficulty = 38, memoryTag = MemoryTag.Discovery, emotion = "Relief", importance = .62f, shards = 2, samples = 0, insight = 1 }
            });
            encounters.Add(new EncounterData
            {
                id = "shadow_in_the_canopy", title = "A Shadow Keeps Pace", location = "Blueglass Thicket",
                description = "A predator moves between distant trunks. It does not attack, but its patient shadow follows the Echo's path.",
                memoryKind = MemoryKind.Personal,
                first = new EncounterChoiceData { label = "Hold the light together", consequence = "The Echo stays near you until the shadow turns away.", favoredTrait = TraitKey.Courage, difficulty = 52, memoryTag = MemoryTag.Predator, emotion = "Trust", importance = .84f, shards = 1, samples = 0, insight = 2 },
                second = new EncounterChoiceData { label = "Leave quietly by the roots", consequence = "The Echo finds a sheltered route and notes where the shadow appeared.", favoredTrait = TraitKey.SelfPreservation, difficulty = 43, memoryTag = MemoryTag.Predator, emotion = "Caution", importance = .76f, shards = 2, samples = 0, insight = 1 }
            });
        }

        private void CreateDefaultMutations()
        {
            mutations.Add(new MutationData { id = MutationId.ThermalVision, title = "Thermal Vision", description = "Warm shapes resolve through jungle shadow. The eye glow turns amber.", shardCost = 3, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.Regeneration, title = "Regeneration", description = "A slow restorative pulse helps the Echo recover between journeys.", shardCost = 4, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.EchoSense, title = "Echo Sense", description = "The Echo feels hollow spaces in the ruins before seeing them.", shardCost = 3, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.AdaptiveSkin, title = "Adaptive Skin", description = "Pearlescent skin shifts toward the colors of its surroundings.", shardCost = 3, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.NeuralLink, title = "Neural Link", description = "A quiet signal makes player interventions feel more familiar.", shardCost = 5, sampleCost = 2 });
            mutations.Add(new MutationData { id = MutationId.DeepMemory, title = "Deep Memory", description = "Important experiences fade more slowly and hold their emotional weight.", shardCost = 4, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.EnhancedHearing, title = "Enhanced Hearing", description = "The Echo's crest catches distant calls in the wind.", shardCost = 3, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.Bioluminescence, title = "Bioluminescence", description = "Living light blooms under the skin, brightest when curious.", shardCost = 4, sampleCost = 1 });
            mutations.Add(new MutationData { id = MutationId.DefensiveShell, title = "Defensive Shell", description = "A translucent back-plate forms without changing the Echo's gentle nature.", shardCost = 5, sampleCost = 2 });
            mutations.Add(new MutationData { id = MutationId.NeuralCreativity, title = "Neural Creativity", description = "New pathways help the Echo solve unfamiliar problems.", shardCost = 4, sampleCost = 1 });
        }
    }
}
