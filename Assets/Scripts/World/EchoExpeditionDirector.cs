using System;
using UnityEngine;

namespace ProjectEcho
{
    public sealed class EchoExpeditionDirector
    {
        private readonly EchoSceneBootstrap game;
        private readonly EchoProfile profile;
        private readonly EchoTuning tuning;
        private int encounterIndex;
        public bool IsActive { get; private set; }
        public bool AwaitingReturn { get; private set; }
        public int CompletedEncounters { get; private set; }
        public EncounterData CurrentEncounter { get; private set; }
        public string LastOutcome { get; private set; } = "The jungle is quiet for now.";
        public Action Changed;

        public EchoExpeditionDirector(EchoSceneBootstrap game, EchoProfile profile, EchoTuning tuning)
        { this.game = game; this.profile = profile; this.tuning = tuning; }

        public void Begin()
        {
            if (IsActive || tuning.encounters.Count == 0) return;
            IsActive = true; AwaitingReturn = false; CompletedEncounters = 0; encounterIndex = 0;
            profile.needs.energy = Mathf.Max(0, profile.needs.energy - 8);
            profile.needs.safety = Mathf.Max(0, profile.needs.safety - 4);
            LastOutcome = profile.displayName + " steps beneath the glass canopy.";
            SelectEncounter(); EchoEvents.ProfileChanged?.Invoke(); Changed?.Invoke(); game.SaveNow();
        }

        public void Choose(int option)
        {
            if (!IsActive || AwaitingReturn || CurrentEncounter == null) return;
            EncounterChoiceData choice = option == 0 ? CurrentEncounter.first : CurrentEncounter.second;
            float trait = profile.traits.Get(choice.favoredTrait);
            float relationshipSupport = profile.playerRelationship.trust * (CurrentEncounter.memoryKind == MemoryKind.Personal ? .08f : .02f);
            float safetySupport = profile.needs.safety * .12f;
            float memorySupport = RelevantMemorySupport(choice.memoryTag);
            if (choice.memoryTag == MemoryTag.Ruin && profile.mutations.Contains(MutationId.EchoSense)) memorySupport += 4f;
            if (choice.memoryTag == MemoryTag.Predator && profile.mutations.Contains(MutationId.DefensiveShell)) safetySupport += 4f;
            float score = trait * .62f + safetySupport + relationshipSupport + memorySupport;
            bool success = score >= choice.difficulty;
            float scale = success ? 1f : .5f;
            string resultWord = success ? "It works." : "It is harder than it looked.";
            string summary = success ? choice.consequence : profile.displayName + " pauses, changes course, and remembers the risk.";
            LastOutcome = resultWord + " " + summary;

            profile.crystalShards += Mathf.Max(0, Mathf.RoundToInt(choice.shards * scale));
            profile.bioSamples += Mathf.Max(0, Mathf.RoundToInt(choice.samples * scale));
            profile.insight += Mathf.Max(0, Mathf.RoundToInt(choice.insight * scale));
            profile.needs.energy = Mathf.Max(0, profile.needs.energy - (success ? 4 : 7));
            profile.needs.curiosity = Mathf.Clamp(profile.needs.curiosity + (success ? 5 : 2), 0, 100);
            profile.needs.safety = Mathf.Clamp(profile.needs.safety + (success ? 2 : -5), 0, 100);
            game.Traits.Experience(choice.favoredTrait, success ? 1.7f : .6f, CurrentEncounter.title);

            if (choice.memoryTag == MemoryTag.Rescue)
            {
                game.Relationships.SharedCare(success ? 4 : 1, success ? 3 : 1, success ? 2 : 0);
                game.Traits.Experience(TraitKey.Loyalty, success ? 1f : .4f, "A shared moment of care");
            }
            if (choice.memoryTag == MemoryTag.Predator && !success)
            {
                game.Relationships.SetFear(2);
                game.Traits.Experience(TraitKey.SelfPreservation, 1.2f, "A close encounter");
            }

            game.Memories.Remember(CurrentEncounter.memoryKind, choice.memoryTag, CurrentEncounter.id,
                success ? choice.emotion : "Unease", CurrentEncounter.location, summary,
                choice.importance, profile.displayName, "Creator");
            game.WorldKnowledge.RecordVisit(CurrentEncounter.location, choice.memoryTag);
            CompletedEncounters++;
            encounterIndex++;
            if (CompletedEncounters >= 3)
            {
                AwaitingReturn = true;
                CurrentEncounter = null;
            }
            else SelectEncounter();
            EchoEvents.ProfileChanged?.Invoke(); Changed?.Invoke(); game.SaveNow();
        }

        public void ReturnToSanctuary()
        {
            if (!IsActive || !AwaitingReturn) return;
            IsActive = false; AwaitingReturn = false; CurrentEncounter = null;
            profile.expeditionsCompleted++;
            profile.day++;
            profile.needs.safety = Mathf.Clamp(profile.needs.safety + 15, 0, 100);
            profile.needs.social = Mathf.Clamp(profile.needs.social + 10, 0, 100);
            profile.needs.energy = Mathf.Clamp(profile.needs.energy + 8, 0, 100);
            LastOutcome = profile.displayName + " returns with new memories. The Sanctuary feels familiar again.";
            game.Memories.Remember(MemoryKind.Personal, MemoryTag.Shelter, "returned_home", "Relief",
                profile.sanctuaryName, LastOutcome, .62f, profile.displayName, "Creator");
            game.WorldKnowledge.RecordVisit(profile.sanctuaryName, MemoryTag.Shelter);
            game.Relationships.SharedCare(2, 2, 1);
            EchoEvents.ProfileChanged?.Invoke(); Changed?.Invoke(); game.SaveNow();
        }

        private float RelevantMemorySupport(MemoryTag tag)
        {
            float weight = game.Memories.WeightOf(tag);
            return Mathf.Min(8f, weight * 3f);
        }

        private void SelectEncounter()
        {
            if (tuning.encounters.Count == 0) { CurrentEncounter = null; return; }
            int start = Mathf.Max(0, profile.expeditionsCompleted % tuning.encounters.Count);
            CurrentEncounter = tuning.encounters[(start + encounterIndex) % tuning.encounters.Count];
        }
    }
}
