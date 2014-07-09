using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationTests
{
    [TestClass]
    public class CRC16Tests
    {
        [TestMethod]
        public void SingleByteTest()
        {
            byte[] msg = new byte[] { 0x31 };
            var target = new PDI.Communication.CRC16();
            var result = target.Calculate(msg);
            if(result[0] != 0x80 & result[1] != 0xa5)
                Assert.Fail("Контрольная сумма не сошлась");
        }

        [TestMethod]
        public void TwoByteTest()
        {
            byte[] msg = new byte[] { 0x31, 0x78 };
            var target = new PDI.Communication.CRC16();
            var result = target.Calculate(msg);
            if (result[0] != 0x27 & result[1] != 0x13)
                Assert.Fail("Контрольная сумма не сошлась");
        }

        [TestMethod]
        public void ThreeByteTest()
        {
            byte[] msg = new byte[] { 0x31, 0x78, 0xF2 };
            var target = new PDI.Communication.CRC16();
            var result = target.Calculate(msg);
            if (result[0] != 0x91 & result[1] != 0xFD)
                Assert.Fail("Контрольная сумма не сошлась");
        }

        [TestMethod]
        public void FourByteTest()
        {
            byte[] msg = new byte[] { 0x31, 0x78, 0xF2, 0xA9 };
            var target = new PDI.Communication.CRC16();
            var result = target.Calculate(msg);
            if (result[0] != 0x7D & result[1] != 0x93)
                Assert.Fail("Контрольная сумма не сошлась");
        }
    }
}
