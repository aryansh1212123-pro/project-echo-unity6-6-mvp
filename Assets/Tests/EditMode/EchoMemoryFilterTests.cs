using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectEcho.Tests
{
    public sealed class EchoMemoryFilterTests
    {
        private static EchoMemory Memory(MemoryKind kind, long timestamp)
        {
            return new EchoMemory { kind = kind, timestampUtc = timestamp };
        }

        [Test]
        public void Apply_AllCategories_ExcludesNullsAndSortsNewestFirst()
        {
            var oldest = Memory(MemoryKind.Personal, 10);
            var newest = Memory(MemoryKind.Social, 30);
            var middle = Memory(MemoryKind.Environmental, 20);
            var source = new List<EchoMemory> { middle, null, oldest, newest };

            List<EchoMemory> result = EchoMemoryFilter.Apply(source, null);

            CollectionAssert.AreEqual(new[] { newest, middle, oldest }, result);
        }

        [TestCase(MemoryKind.Personal)]
        [TestCase(MemoryKind.Environmental)]
        [TestCase(MemoryKind.Social)]
        public void Apply_Category_ReturnsOnlyMatchingMemoriesInNewestFirstOrder(MemoryKind kind)
        {
            var olderMatch = Memory(kind, 5);
            var other = Memory(kind == MemoryKind.Personal ? MemoryKind.Social : MemoryKind.Personal, 100);
            var newerMatch = Memory(kind, 15);

            List<EchoMemory> result = EchoMemoryFilter.Apply(
                new[] { olderMatch, other, newerMatch }, kind);

            CollectionAssert.AreEqual(new[] { newerMatch, olderMatch }, result);
        }

        [Test]
        public void Apply_NullOrEmptySource_ReturnsEmptyList()
        {
            Assert.That(EchoMemoryFilter.Apply(null, null), Is.Empty);
            Assert.That(EchoMemoryFilter.Apply(new List<EchoMemory>(), MemoryKind.Social), Is.Empty);
        }
    }
}
