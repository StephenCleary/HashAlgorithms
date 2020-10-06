using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Globalization;

namespace Nito.CRCs
{
    /// <summary>
    /// A generalized CRC-16 algorithm.
    /// </summary>
    public sealed class CRC16 : HashAlgorithm
    {
        /// <summary>
        /// The lookup tables for non-reversed polynomials.
        /// </summary>
        private static readonly ConcurrentDictionary<ushort, ushort[]> NormalLookupTables = new ConcurrentDictionary<ushort, ushort[]>();
        
        /// <summary>
        /// The lookup tables for reversed polynomials.
        /// </summary>
        private static readonly ConcurrentDictionary<ushort, ushort[]> ReversedLookupTables = new ConcurrentDictionary<ushort, ushort[]>();

        /// <summary>
        /// A reference to the lookup table.
        /// </summary>
        private readonly ushort[] lookupTable;

        /// <summary>
        /// The CRC-32 algorithm definition.
        /// </summary>
        private readonly Definition definition;

        /// <summary>
        /// The current value of the remainder.
        /// </summary>
        private ushort remainder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC16"/> class with the specified definition and lookup table.
        /// </summary>
        /// <param name="definition">The CRC-16 algorithm definition. May not be <c>null</c>.</param>
        /// <param name="lookupTable">The lookup table. May not be <c>null</c>.</param>
        public CRC16(Definition definition, ushort[] lookupTable)
        {
            _ = definition ?? throw new ArgumentNullException(nameof(definition));
            _ = lookupTable ?? throw new ArgumentNullException(nameof(lookupTable));
            if (lookupTable.Length != 256)
                throw new ArgumentException($"{nameof(lookupTable)} must have 256 entries, but it has {lookupTable.Length} entries.", nameof(lookupTable));

#if !NETSTANDARD1_3
            this.HashSizeValue = 16;
#endif
            this.definition = definition;
            this.lookupTable = lookupTable;
            this.Initialize();
        }

#if NETSTANDARD1_3
        /// <inheritdoc/>
        public override int HashSize => 16;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC16"/> class with the specified definition.
        /// </summary>
        /// <param name="definition">The CRC-16 algorithm definition.</param>
        public CRC16(Definition definition)
            : this(definition, FindOrGenerateLookupTable(definition))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CRC16"/> class with the default definition.
        /// </summary>
        public CRC16()
            : this(Definition.Default)
        {
        }

        /// <summary>
        /// Gets the result of the CRC-16 algorithm.
        /// </summary>
        public ushort Result
        {
            get
            {
                if (this.definition.ReverseResultBeforeFinalXor != this.definition.ReverseDataBytes)
                {
                    return (ushort)(HackersDelight.Reverse(this.remainder) ^ this.definition.FinalXorValue);
                }
                else
                {
                    return (ushort)(this.remainder ^ this.definition.FinalXorValue);
                }
            }
        }

        /// <summary>
        /// Searches the known lookup tables for one matching the given CRC-16 definition; if none is found, a new lookup table is generated and added to the known lookup tables.
        /// </summary>
        /// <param name="definition">The CRC-16 definition. May not be <c>null</c>.</param>
        /// <returns>The lookup table for the given CRC-16 definition.</returns>
        public static ushort[] FindOrGenerateLookupTable(Definition definition)
        {
            _ = definition ?? throw new ArgumentNullException(nameof(definition));

            ConcurrentDictionary<ushort, ushort[]> tables;
            if (definition.ReverseDataBytes)
            {
                tables = ReversedLookupTables;
            }
            else
            {
                tables = NormalLookupTables;
            }

            var ret = tables.GetOrAdd(definition.TruncatedPolynomial, _ => GenerateLookupTable(definition));
            return ret;
        }

        /// <summary>
        /// Generates a lookup table for a CRC-16 algorithm definition. Both <see cref="Definition.TruncatedPolynomial"/> and <see cref="Definition.ReverseDataBytes"/> are used in the calculations.
        /// </summary>
        /// <param name="definition">The CRC-16 algorithm definition. May not be <c>null</c>.</param>
        /// <returns>The lookup table.</returns>
        public static ushort[] GenerateLookupTable(Definition definition)
        {
            _ = definition ?? throw new ArgumentNullException(nameof(definition));

            unchecked
            {
                ushort[] ret = new ushort[256];

                byte dividend = 0;
                do
                {
                    ushort remainder = 0;

                    for (byte mask = 0x80; mask != 0; mask >>= 1)
                    {
                        if ((dividend & mask) != 0)
                        {
                            remainder ^= 0x8000;
                        }

                        if ((remainder & 0x8000) != 0)
                        {
                            remainder <<= 1;
                            remainder ^= definition.TruncatedPolynomial;
                        }
                        else
                        {
                            remainder <<= 1;
                        }
                    }

                    if (definition.ReverseDataBytes)
                    {
                        var index = HackersDelight.Reverse(dividend);
                        ret[index] = HackersDelight.Reverse(remainder);
                    }
                    else
                    {
                        ret[dividend] = remainder;
                    }

                    ++dividend;
                }
                while (dividend != 0);

                return ret;
            }
        }

        /// <summary>
        /// Initializes the CRC-16 calculations.
        /// </summary>
        public override void Initialize()
        {
            if (this.definition.ReverseDataBytes)
            {
                this.remainder = HackersDelight.Reverse(this.definition.Initializer);
            }
            else
            {
                this.remainder = this.definition.Initializer;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return "0x" + this.Result.ToString("X4", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
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

        private void DoHashCore(ReadOnlySpan<byte> source)
        {
            unchecked
            {
                ushort remainder = this.remainder;
                for (int i = 0; i != source.Length; ++i)
                {
                    byte index = this.ReflectedIndex(remainder, source[i]);
                    remainder = this.ReflectedShift(remainder);
                    remainder ^= this.lookupTable[index];
                }

                this.remainder = remainder;
            }
        }

#if !NETSTANDARD1_3 && !NETSTANDARD2_0
        /// <inheritdoc/>
        protected override void HashCore(ReadOnlySpan<byte> source) => DoHashCore(source);
#endif

        /// <inheritdoc/>
        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(this.Result);
        }

#if !NETSTANDARD1_3 && !NETSTANDARD2_0
        /// <inheritdoc/>
        protected override bool TryHashFinal(Span<byte> destination, out int bytesWritten)
        {
            bytesWritten = 2;
            return BitConverter.TryWriteBytes(destination, Result);
        }
#endif

        /// <summary>
        /// Gets the index into the lookup array for a given remainder and data byte. Data byte reversal is taken into account.
        /// </summary>
        /// <param name="remainder">The current remainder.</param>
        /// <param name="data">The data byte.</param>
        /// <returns>The index into the lookup array.</returns>
        private byte ReflectedIndex(ushort remainder, byte data)
        {
            unchecked
            {
                if (this.definition.ReverseDataBytes)
                {
                    return (byte)(remainder ^ data);
                }
                else
                {
                    return (byte)((remainder >> 8) ^ data);
                }
            }
        }

        /// <summary>
        /// Shifts a byte out of the remainder. This is the high byte or low byte, depending on whether the data bytes are reversed.
        /// </summary>
        /// <param name="remainder">The remainder value.</param>
        /// <returns>The shifted remainder value.</returns>
        private ushort ReflectedShift(ushort remainder)
        {
            unchecked
            {
                if (this.definition.ReverseDataBytes)
                {
                    return (ushort)(remainder >> 8);
                }
                else
                {
                    return (ushort)(remainder << 8);
                }
            }
        }

        /// <summary>
        /// Holds parameters for a CRC-16 algorithm.
        /// </summary>
        public sealed class Definition
        {
            /// <summary>
            /// Gets a common CRC-16, used by ARC and LHA.
            /// </summary>
            public static Definition Default
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x8005,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 used by floppy disk formats, commonly misidentified as CCITT.
            /// </summary>
            public static Definition CcittFalse
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                        Initializer = 0xFFFF,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 known as CCITT, used by Kermit. Appears in "Numerical Recipes in C".
            /// </summary>
            public static Definition Ccitt
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 used by XMODEM and ZMODEM. Appears in "Numerical Recipes in C".
            /// </summary>
            public static Definition XModem
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                    };
                }
            }

            /// <summary>
            /// Gets a CRC-16 used by X.25, V.42, T.30, RFC 1171. Appears in "Numerical Recipes in C".
            /// </summary>
            public static Definition X25
            {
                get
                {
                    return new Definition
                    {
                        TruncatedPolynomial = 0x1021,
                        Initializer = 0xFFFF,
                        FinalXorValue = 0xFFFF,
                        ReverseDataBytes = true,
                        ReverseResultBeforeFinalXor = true,
                    };
                }
            }

            /// <summary>
            /// Gets or sets the normal (non-reversed, non-reciprocal) polynomial to use for the CRC calculations.
            /// </summary>
            public ushort TruncatedPolynomial { get; set; }

            /// <summary>
            /// Gets or sets the value to which the remainder is initialized at the beginning of the CRC calculation.
            /// </summary>
            public ushort Initializer { get; set; }

            /// <summary>
            /// Gets or sets the value by which the remainder is XOR'ed at the end of the CRC calculation.
            /// </summary>
            public ushort FinalXorValue { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether incoming data bytes are reversed/reflected.
            /// </summary>
            public bool ReverseDataBytes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the final remainder is reversed/reflected at the end of the CRC calculation before it is XOR'ed with <see cref="FinalXorValue"/>.
            /// </summary>
            public bool ReverseResultBeforeFinalXor { get; set; }

            /// <summary>
            /// Creates a new <see cref="CRC16"/> instance using this definition.
            /// </summary>
            /// <returns>A new <see cref="CRC16"/> instance using this definition.</returns>
            public CRC16 Create()
            {
                return new CRC16(this);
            }
        }
    }
}
