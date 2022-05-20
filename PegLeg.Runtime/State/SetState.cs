using PegLeg.Runtime.Hashing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PegLeg.Runtime.State
{
    public sealed class SetState : HashStateBase<SetState>
    {
        private readonly ImmutableSortedDictionary<int, StringMemorySet> _sets;

        public static SetState Empty { get; } = new SetState(Scope, ImmutableSortedDictionary<int, StringMemorySet>.Empty);

        private SetState(HashValue hash, ImmutableSortedDictionary<int, StringMemorySet> sets)
            : base(hash)
        {
            _sets = sets;
        }

        private static SetState Create(Hasher hasher, ImmutableSortedDictionary<int, StringMemorySet> sets)
        {
            if (sets.IsEmpty)
                return Empty;
            hasher.Append(Scope);
            foreach (var item in sets)
            {
                hasher.Append(item.Key);
                hasher.Append(item.Value.Hash);
            }

            return new SetState(hasher.GetAndReset(), sets);
        }

        public bool IsEmpty => _sets.IsEmpty;

        // TODO: We'll need to define the semantics for #~name?e - i.e., case sensitivity! It's currently ordinal.
        public bool Contains(int key, ReadOnlyMemory<char> value)
        {
            if (!_sets.TryGetValue(key, out var set))
                return false;
            return set.Contains(new ComparableMemoryString(value));
        }

        public SetState Add(Hasher hasher, int key, ReadOnlyMemory<char> value)
        {
            if (!_sets.ContainsKey(key))
                return Create(hasher, _sets.Add(key, StringMemorySet.Unit(hasher, new ComparableMemoryString(value))));

            var set = _sets.ValueRef(key);
            var newSet = set.Add(hasher, new ComparableMemoryString(value));
            if (newSet.Equals(set))
                return this;

            return Create(hasher, _sets.SetItem(key, newSet));
        }

        /// <summary>
        /// A <see cref="ReadOnlyMemory{char}"/> that is comparable, using ordinal comparison.
        /// </summary>
        private readonly struct ComparableMemoryString : IEquatable<ComparableMemoryString>, IComparable<ComparableMemoryString>
        {
            public readonly ReadOnlyMemory<char> Memory;

            public ComparableMemoryString(ReadOnlyMemory<char> memory) => Memory = memory;

            public int CompareTo(ComparableMemoryString other) => Memory.Span.CompareTo(other.Memory.Span, StringComparison.Ordinal);
            public readonly bool Equals(ComparableMemoryString other) => Memory.Span.Equals(other.Memory.Span, StringComparison.Ordinal);
            public override readonly bool Equals(object? obj) => obj is ComparableMemoryString x && Equals(x);
            public override int GetHashCode()
            {
                var hashCode = new HashCode();
#if NETSTANDARD
                foreach (var value in Memory.Span)
                    hashCode.Add(value);
#else
                hashCode.AddBytes(MemoryMarshal.AsBytes(Memory.Span));
#endif
                return hashCode.ToHashCode();
            }
        }

        private sealed class StringMemorySet : HashStateBase<StringMemorySet>
        {
            public bool Contains(ComparableMemoryString value) => _set.Contains(value);

            public StringMemorySet Add(Hasher hasher, ComparableMemoryString value)
            {
                var newSet = _set.Add(value);
                if (newSet == _set)
                    return this;
                return Create(hasher, newSet);
            }

            public static StringMemorySet Unit(Hasher hasher, ComparableMemoryString value) => Create(hasher, ImmutableSortedSet<ComparableMemoryString>.Empty.Add(value));

            private StringMemorySet(HashValue hash, ImmutableSortedSet<ComparableMemoryString> set)
                : base(hash)
            {
                _set = set;
            }

            private static StringMemorySet Create(Hasher hasher, ImmutableSortedSet<ComparableMemoryString> set)
            {
                // Note: Cannot handle empty sets!

                hasher.Append(Scope);
                foreach (var item in set)
                    hasher.Append(item.Memory.Span);

                return new StringMemorySet(hasher.GetAndReset(), set);
            }

            private readonly ImmutableSortedSet<ComparableMemoryString> _set;
        }
    }
}
