using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PegLeg.Runtime.Hashing
{
    /// <summary>
    /// Provides a reusable incremental hash.
    /// </summary>
    public sealed class Hasher
    {
#if NETSTANDARD
        /// <summary>
        /// Gets the final hash value.
        /// </summary>
        public HashValue GetAndReset()
        {
            var result = new HashValue();
            var hashResult = _hasher.GetHashAndReset();
            hashResult.AsSpan().CopyTo(((IHashValue) result).Span);
            return result;
        }
#else
        /// <summary>
        /// Gets the final hash value.
        /// </summary>
        public HashValue GetAndReset()
        {
            var result = new HashValue();
            _hasher.GetHashAndReset(((IHashValue) result).Span);
            return result;
        }
#endif

        /// <summary>
        /// Appends an integer data value to the hash.
        /// </summary>
        public void Append(int data) => AppendValue(data);

        /// <summary>
        /// Appends a span of characters to the hash as binary data.
        /// </summary>
        public void Append(ReadOnlySpan<char> data) => AppendSpan(data);

        /// <summary>
        /// Appends a hash value to the hash.
        /// </summary>
        public void Append(HashValue hash) => AppendSpan(hash.ReadOnlySpan);

        private void AppendSpan<T>(ReadOnlySpan<T> data) where T : unmanaged => AppendSpan(MemoryMarshal.AsBytes(data));


#if NETSTANDARD

        private void AppendSpan(ReadOnlySpan<byte> data)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(data.Length);
            try
            {
                data.CopyTo(buffer);
                _hasher.AppendData(buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void AppendValue<T>(T data) where T : unmanaged
        {
            Span<T> span = stackalloc T[1];
            span[0] = data;
            AppendSpan<T>(span);
        }

#else

        private void AppendSpan(ReadOnlySpan<byte> data) => _hasher.AppendData(data);
        private void AppendValue<T>(T data) where T : unmanaged => AppendSpan(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in data), length: 1));

#endif

        private readonly IncrementalHash _hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
    }
}
