using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Nito.HashAlgorithms
{
    /// <summary>
    /// Provides a base type for hash algorithms with a simpler API than <see cref="HashAlgorithm"/>.
    /// </summary>
    public abstract class HashAlgorithmBase : HashAlgorithm
    {
        /// <summary>
        /// Creates a new instance of this type. Usually, derived constructors finish by calling <see cref="HashAlgorithm.Initialize"/>.
        /// </summary>
        /// <param name="hashSizeInBits">The size of the hash, in bits.</param>
        protected HashAlgorithmBase(int hashSizeInBits)
        {
#if NETSTANDARD1_3
            HashSize = hashSizeInBits;
#else
            HashSizeValue = hashSizeInBits;
#endif
        }

        /// <summary>
        /// Routes the specified data into the hash algorithm.
        /// </summary>
        /// <param name="source">The data to hash.</param>
        protected abstract void DoHashCore(ReadOnlySpan<byte> source);

        /// <summary>
        /// Finalizes the hash computation.
        /// </summary>
        /// <param name="destination">The buffer to receive the hash value.</param>
        protected abstract void DoHashFinal(Span<byte> destination);

#if NETSTANDARD1_3
        /// <inheritdoc />
        public override int HashSize { get; }
#endif

        /// <inheritdoc />
        protected override void HashCore(byte[] array, int offset, int count)
        {
            _ = array ?? throw new ArgumentNullException(nameof(array));
            if (offset < 0)
                throw new ArgumentException($"{nameof(offset)} must be greater than or equal to 0, but is equal to {offset}.");
            if (count < 0)
                throw new ArgumentException($"{nameof(count)} must be greater than or equal to 0, but is equal to {count}.");
            if (count > array.Length)
                throw new ArgumentException($"{nameof(count)} must be less than or equal to the length of {nameof(array)} ({array.Length}), but is equal to {count}.");
            if (offset > array.Length - count)
                throw new ArgumentException($"{nameof(offset)} must be less than or equal to the length of {nameof(array)} ({array.Length}) minus {nameof(count)} ({count}), but is equal to {offset}.");

            DoHashCore(array.AsSpan().Slice(offset, count));
        }

        /// <inheritdoc />
        protected override byte[] HashFinal()
        {
            var result = new byte[HashSize / 8];
            DoHashFinal(result);
            return result;
        }

#if !NETSTANDARD1_3 && !NETSTANDARD2_0 && !NET461
        /// <inheritdoc />
        protected override void HashCore(ReadOnlySpan<byte> source) => DoHashCore(source);

        /// <inheritdoc />
        protected override bool TryHashFinal(Span<byte> destination, out int bytesWritten)
        {
            DoHashFinal(destination);
            bytesWritten = HashSizeValue / 8;
            return true;
        }
#endif
    }
}
