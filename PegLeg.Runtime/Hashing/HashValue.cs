using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PegLeg.Runtime.Hashing
{
    public interface IHashValue
    {
        Span<byte> Span { get; }
    }

    /// <summary>
    /// An immutable hash.
    /// </summary>
    public sealed class HashValue : IEquatable<HashValue>, IHashValue
    {
        private readonly int[] _data = new int[5];

        /// <summary>
        /// Retrieves the underlying span of bytes for the hash value. This member should only be accessed during construction.
        /// </summary>
        Span<byte> IHashValue.Span => MemoryMarshal.AsBytes(_data.AsSpan());

        /// <summary>
        /// Retrieves the underlying span of bytes for the hash value.
        /// </summary>
        public ReadOnlySpan<byte> ReadOnlySpan => ((IHashValue) this).Span;

        // Benchmarks indicate manual comparison is faster than `_data.AsSpan().SequenceEqual(other._data.AsSpan())`, likely because the hashes are highly random so the first comparison finds any differences.
        public bool Equals(HashValue? other) => other != null && _data[0] == other._data[0] && _data[1] == other._data[1] && _data[2] == other._data[2] && _data[3] == other._data[3] && _data[4] == other._data[4];
        public override bool Equals(object? obj) => obj is HashValue other && Equals(other);
        public override int GetHashCode() => _data[0] ^ _data[1] ^ _data[2] ^ _data[3] ^ _data[4];
    }
}
