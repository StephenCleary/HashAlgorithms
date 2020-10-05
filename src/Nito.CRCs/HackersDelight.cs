using System;

namespace Nito.CRCs
{
    /// <summary>
    /// Helpful methods from the book <a href="http://www.amazon.com/gp/product/0201914654?ie=UTF8&amp;tag=stepheclearys-20&amp;linkCode=as2&amp;camp=1789&amp;creative=390957&amp;creativeASIN=0201914654">Hacker's Delight</a>.
    /// </summary>
    internal static class HackersDelight
    {
        /// <summary>
        /// Reverses the bits in an unsigned integer of data.
        /// </summary>
        /// <param name="data">The unsigned integer whose bits are to be reversed.</param>
        /// <returns>The reversed data.</returns>
        public static uint Reverse(uint data)
        {
            unchecked
            {
                uint ret = data;
                ret = (ret & 0x55555555) << 1 | (ret >> 1) & 0x55555555;
                ret = (ret & 0x33333333) << 2 | (ret >> 2) & 0x33333333;
                ret = (ret & 0x0F0F0F0F) << 4 | (ret >> 4) & 0x0F0F0F0F;
                ret = (ret << 24) | ((ret & 0xFF00) << 8) | ((ret >> 8) & 0xFF00) | (ret >> 24);
                return ret;
            }
        }

        /// <summary>
        /// Reverses the bits in an unsigned short of data.
        /// </summary>
        /// <param name="data">The unsigned short whose bits are to be reversed.</param>
        /// <returns>The reversed data.</returns>
        public static ushort Reverse(ushort data)
        {
            unchecked
            {
                ushort ret = data;
                ret = (ushort)((ret & 0x5555) << 1 | (ret >> 1) & 0x5555);
                ret = (ushort)((ret & 0x3333) << 2 | (ret >> 2) & 0x3333);
                ret = (ushort)((ret & 0x0F0F) << 4 | (ret >> 4) & 0x0F0F);
                ret = (ushort)((ret & 0x00FF) << 8 | (ret >> 8) & 0x00FF);
                return ret;
            }
        }

        /// <summary>
        /// Reverses the bits in a byte of data.
        /// </summary>
        /// <param name="data">The byte whose bits are to be reversed.</param>
        /// <returns>The reversed data.</returns>
        public static byte Reverse(byte data)
        {
            unchecked
            {
                uint u = (uint)data * 0x00020202;
                uint m = 0x01044010;
                uint s = u & m;
                uint t = (u << 2) & (m << 1);
                return (byte)((0x01001001 * (s + t)) >> 24);
            }
        }
    }
}
