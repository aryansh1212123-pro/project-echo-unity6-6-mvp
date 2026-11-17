using NUnit.Framework;

namespace ProjectEcho.Tests
{
    public sealed class EchoDataModelTests
    {
        [Test]
        public void ProfileEnsureValid_RepairsDefaultsAndClampsValues()
        {
            var profile = new EchoProfile
            {
                id = " ",
                displayName = " ",
                sanctuaryName = null,
                traits = new EchoTraits { courage = -12f, curiosity = 125f },
                needs = new EchoNeeds { energy = -4f, hunger = 140f },
                playerRelationship = new EchoRelationship { trust = 130f, fear = -5f },
                memories = null,
                mutations = null,
                crystalShards = -2,
                bioSamples = -1,
                insight = -8,
                day = 0,
                expeditionsCompleted = -3
            };

            profile.EnsureValid();

            Assert.That(string.IsNullOrWhiteSpace(profile.id), Is.False);
            Assert.That(profile.displayName, Is.EqualTo("Luma"));
            Assert.That(profile.sanctuaryName, Is.EqualTo("The Quiet Between"));
            Assert.That(profile.traits.courage, Is.EqualTo(0f));
            Assert.That(profile.traits.curiosity, Is.EqualTo(100f));
            Assert.That(profile.needs.energy, Is.EqualTo(0f));
            Assert.That(profile.needs.hunger, Is.EqualTo(100f));
            Assert.That(profile.playerRelationship.trust, Is.EqualTo(100f));
            Assert.That(profile.playerRelationship.fear, Is.EqualTo(0f));
            Assert.That(profile.memories, Is.Not.Null);
            Assert.That(profile.mutations, Is.Not.Null);
            Assert.That(profile.crystalShards, Is.Zero);
            Assert.That(profile.bioSamples, Is.Zero);
            Assert.That(profile.insight, Is.Zero);
            Assert.That(profile.day, Is.EqualTo(1));
            Assert.That(profile.expeditionsCompleted, Is.Zero);
        }

        [Test]
        public void GameStateEnsureValid_RepairsWorldKnowledgeAndEntries()
        {
            var state = new EchoGameState
            {
                echo = null,
                worldKnowledge = null
            };

            state.EnsureValid();

            Assert.That(state.echo, Is.Not.Null);
            Assert.That(state.worldKnowledge, Is.Not.Null);

            state.worldKnowledge.Add(new WorldKnowledgeEntry
            {
                location = " ",
                knownTags = null
            });
            state.EnsureValid();

            Assert.That(state.worldKnowledge[0].location, Is.EqualTo("Unknown"));
            Assert.That(state.worldKnowledge[0].knownTags, Is.Not.Null);
        }
    }
}
