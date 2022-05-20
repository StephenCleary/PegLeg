using PegLeg.Runtime.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PegLeg.Runtime.State
{
    public sealed class VariableState : HashStateBase<VariableState>
    {
        public static VariableState Empty { get; } = new(Scope, BackrefState.Empty, SetState.Empty, BoolState.Empty);

        public InputRange? BackrefTryGet(int key) => _backrefs.TryGet(key);

        public VariableState BackrefAdd(Hasher hasher, int key, InputRange value)
        {
            var newBackrefs = _backrefs.Add(hasher, key, value);
            if (newBackrefs == _backrefs)
                return this;
            return Create(hasher, newBackrefs, _sets, _bools);
        }

        public bool SetContains(int key, ReadOnlyMemory<char> value) => _sets.Contains(key, value);

        public VariableState SetAdd(Hasher hasher, int key, ReadOnlyMemory<char> value)
        {
            var newSets = _sets.Add(hasher, key, value);
            if (newSets == _sets)
                return this;
            return Create(hasher, _backrefs, newSets, _bools);
        }

        public bool BoolContains(int value) => _bools.Contains(value);

        public VariableState BoolAdd(Hasher hasher, int value)
        {
            var newBools = _bools.Add(hasher, value);
            if (newBools == _bools)
                return this;
            return Create(hasher, _backrefs, _sets, newBools);
        }

        private VariableState(HashValue hash, BackrefState backrefs, SetState sets, BoolState bools)
            : base(hash)
        {
            _backrefs = backrefs;
            _sets = sets;
            _bools = bools;
        }

        private static VariableState Create(Hasher hasher, BackrefState backrefs, SetState sets, BoolState bools)
        {
            if (backrefs.IsEmpty && sets.IsEmpty && bools.IsEmpty)
                return Empty;
            hasher.Append(backrefs.Hash);
            hasher.Append(sets.Hash);
            hasher.Append(bools.Hash);
            return new(hasher.GetAndReset(), backrefs, sets, bools);
        }

        private readonly BackrefState _backrefs;
        private readonly SetState _sets;
        private readonly BoolState _bools;
    }
}
