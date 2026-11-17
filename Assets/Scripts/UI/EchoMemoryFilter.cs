using System.Collections.Generic;
using System.Linq;

namespace ProjectEcho
{
    /// <summary>Applies the Memory Archive's category filter and newest-first ordering.</summary>
    public static class EchoMemoryFilter
    {
        public static List<EchoMemory> Apply(IEnumerable<EchoMemory> memories, MemoryKind? kind)
        {
            if (memories == null) return new List<EchoMemory>();
            return memories
                .Where(memory => memory != null && (!kind.HasValue || memory.kind == kind.Value))
                .OrderByDescending(memory => memory.timestampUtc)
                .ToList();
        }
    }
}
