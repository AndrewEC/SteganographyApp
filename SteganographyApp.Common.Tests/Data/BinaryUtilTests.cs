using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class BinaryUtilTests
    {

        private static readonly string OriginalBinaryString = "1101010101000011101011111000000010101010";

        [TestMethod]
        public void TestToBase64AndBackToBinary()
        {
            var util = new BinaryUtil();

            string base64 = util.ToBase64String(OriginalBinaryString);
            Assert.AreNotEqual(OriginalBinaryString, base64);

            string binary = util.ToBinaryString(base64);
            Assert.AreEqual(OriginalBinaryString, binary);
        }

    }

}