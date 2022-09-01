namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class BinaryUtilTests
    {
        private const string BinaryString = "0000000100000010";
        private readonly BinaryUtil util = new BinaryUtil();

        [Test]
        public void TestToBinaryString()
        {
            string result = util.ToBinaryString(new byte[] { 1, 2 });
            Assert.AreEqual(BinaryString, result);
        }

        [Test]
        public void TestBinaryToStringDirect()
        {
            string result = util.ToBinaryStringDirect(new byte[] { 1, 0, 1 });
            Assert.AreEqual("101", result);
        }

        [Test]
        public void TestToBytes()
        {
            byte[] result = util.ToBytes(BinaryString);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
        }

        [Test]
        public void TestToBytesDirect()
        {
            byte[] result = util.ToBytesDirect("101");
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(0, result[1]);
            Assert.AreEqual(1, result[2]);
        }
    }
}