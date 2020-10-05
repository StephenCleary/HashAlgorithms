using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nito.CRCs;
using Xunit;

namespace Tests.Unit_Tests
{
    public class CRC32UnitTests
    {
        [Fact]
        public void CRC32_Default_ChecksumVerification()
        {
            var calculator = new CRC32();
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal(0xCBF43926, BitConverter.ToUInt32(result, 0));
        }

        [Fact]
        public void CRC32_Bzip2_ChecksumVerification()
        {
            var calculator = new CRC32(CRC32.Definition.BZip2);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal(0xFC891918, BitConverter.ToUInt32(result, 0));
        }

        [Fact]
        public void CRC32_Castagnoli_ChecksumVerification()
        {
            var calculator = new CRC32(CRC32.Definition.Castagnoli);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal(0xE3069283, BitConverter.ToUInt32(result, 0));
        }

        [Fact]
        public void CRC32_Mpeg2_ChecksumVerification()
        {
            var calculator = new CRC32(CRC32.Definition.Mpeg2);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal((uint)0x0376E6E7, BitConverter.ToUInt32(result, 0));
        }

        [Fact]
        public void CRC32_Posix_ChecksumVerification()
        {
            var calculator = new CRC32(CRC32.Definition.Posix);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal((uint)0x765E7680, BitConverter.ToUInt32(result, 0));
        }

        [Fact]
        public void CRC32_Aixm_ChecksumVerification()
        {
            var calculator = new CRC32(CRC32.Definition.Aixm);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal((uint)0x3010BF7F, BitConverter.ToUInt32(result, 0));
        }

        [Fact]
        public void CRC32_Xfer_ChecksumVerification()
        {
            var calculator = new CRC32(CRC32.Definition.Xfer);
            var test = Encoding.ASCII.GetBytes("123456789");
            var result = calculator.ComputeHash(test, 0, test.Length);
            Assert.Equal(0xBD0BE338, BitConverter.ToUInt32(result, 0));
        }
    }
}
