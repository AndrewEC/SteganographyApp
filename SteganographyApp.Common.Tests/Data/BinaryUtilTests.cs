namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class BinaryUtilTests
    {
        private static readonly string OriginalBinaryString = "1101010101000011101011111000000010101010";

        [Test]
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