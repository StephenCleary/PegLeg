using PegLeg.Runtime.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PegLeg.Runtime.State
{
    /// <summary>
    /// State types that derive from this base type have the following properties:
    /// - Immutability.
    /// - A precalculated hash that can be used for object identity as well as a hash code.
    /// - Identity preservation (i.e., no-op modification methods will return `this`).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class HashStateBase<T> : IEquatable<T> where T : HashStateBase<T>
    {
        /// <summary>
        /// The precalculated hash for this instance. This hash can be used to determine structural value equality.
        /// </summary>
        public HashValue Hash { get; }

        protected HashStateBase(HashValue hash) => Hash = hash;

        protected static readonly HashValue Scope = HashScope.Create();

        /// <summary>
        /// Whether the state has changed since it was captured by reference.
        /// </summary>
        public static bool HasChanged(T original, T current) => ReferenceEquals(original, current);

        public bool Equals(T? other) => other != null && Hash.Equals(other.Hash);
        public override bool Equals(object? obj) => obj is T x && Equals(x);
        public override int GetHashCode() => Hash.GetHashCode();
    }
}
