using PegLeg.Runtime.Hashing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PegLeg.Runtime.State
{
    public sealed class BackrefState : HashStateBase<BackrefState>
    {
        public static BackrefState Empty { get; } = new BackrefState(Scope, ImmutableSortedDictionary<int, InputRange>.Empty);

        public bool IsEmpty => _backrefs.IsEmpty;

        public InputRange? TryGet(int key) => _backrefs.TryGetValue(key, out var value) ? value : null;

        public BackrefState Add(Hasher hasher, int key, InputRange value)
        {
            var newBackrefs = _backrefs.SetItem(key, value);
            if (newBackrefs == _backrefs)
                return this;
            return Create(hasher, newBackrefs);
        }

        private BackrefState(HashValue hash, ImmutableSortedDictionary<int, InputRange> backrefs)
            : base(hash)
        {
            _backrefs = backrefs;
        }

        private static BackrefState Create(Hasher hasher, ImmutableSortedDictionary<int, InputRange> backrefs)
        {
            if (backrefs.IsEmpty)
                return Empty;
            hasher.Append(Scope);
            foreach (var item in backrefs)
            {
                hasher.Append(item.Key);
                hasher.Append(item.Value.Start);
                hasher.Append(item.Value.End);
            }

            return new BackrefState(hasher.GetAndReset(), backrefs);
        }

        private readonly ImmutableSortedDictionary<int, InputRange> _backrefs;
    }
}
