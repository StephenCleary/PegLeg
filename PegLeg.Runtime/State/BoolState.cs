using PegLeg.Runtime.Hashing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PegLeg.Runtime.State
{
    public sealed class BoolState : HashStateBase<BoolState>
    {
        public static BoolState Empty { get; } = new BoolState(Scope, ImmutableSortedSet<int>.Empty);

        public bool IsEmpty => _set.IsEmpty;

        public bool Contains(int value) => _set.Contains(value);

        public BoolState Add(Hasher hasher, int value)
        {
            var newSet = _set.Add(value);
            if (newSet == _set)
                return this;
            return Create(hasher, newSet);
        }

        private BoolState(HashValue hash, ImmutableSortedSet<int> set)
            : base(hash)
        {
            _set = set;
        }

        private static BoolState Create(Hasher hasher, ImmutableSortedSet<int> set)
        {
            if (set.IsEmpty)
                return Empty;
            hasher.Append(Scope);
            foreach (var item in set)
                hasher.Append(item);
            return new BoolState(hasher.GetAndReset(), set);
        }

        private readonly ImmutableSortedSet<int> _set;
    }
}
