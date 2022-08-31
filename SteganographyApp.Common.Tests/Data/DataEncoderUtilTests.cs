namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Text;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    using static Moq.It;

    [TestFixture]
    public class DataEncoderUtilTests : FixtureWithTestObjects
    {
        [Mockup(typeof(IEncryptionUtil))]
        public Mock<IEncryptionUtil> mockEncryptionProxy;

        [Mockup(typeof(IBinaryUtil))]
        public Mock<IBinaryUtil> mockBinaryUtil;

        [Mockup(typeof(IDummyUtil))]
        public Mock<IDummyUtil> mockDummyUtil;

        [Mockup(typeof(IRandomizeUtil))]
        public Mock<IRandomizeUtil> mockRandomUtil;

        [Mockup(typeof(ICompressionUtil))]
        public Mock<ICompressionUtil> mockCompressionUtil;

        private const string Password = "password";
        private const bool UseCompression = true;
        private const int DummyCount = 10;
        private const string RandomSeed = "randomSeed";
        private const string StringToDecode = "stringToDecode";
        private const int AdditionalHashIterations = 2;
        private static readonly byte[] InputBytes = new byte[1];

        [Test]
        public void TestEncode()
        {
            byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
            mockEncryptionProxy.Setup(provider => provider.GenerateKey(RandomSeed, EncryptionUtil.DefaultIterations)).Returns(randomKey);
            string randomKeyString = Convert.ToBase64String(randomKey);

            byte[] encryptedBytes = new byte[] { 1 };
            mockEncryptionProxy.Setup(provider => provider.Encrypt(InputBytes, Password, AdditionalHashIterations)).Returns(encryptedBytes);

            byte[] dummyBytes = new byte[] { 2 };
            mockDummyUtil.Setup(provider => provider.InsertDummies(DummyCount, encryptedBytes, randomKeyString)).Returns(dummyBytes);

            byte[] randomizedBytes = new byte[] { 3 };
            mockRandomUtil.Setup(provider => provider.Randomize(dummyBytes, randomKeyString, DummyCount, 2)).Returns(randomizedBytes);

            byte[] compressedBytes = new byte[] { 4 };
            mockCompressionUtil.Setup(provider => provider.Compress(randomizedBytes)).Returns(compressedBytes);

            string binaryString = "binary_string";
            mockBinaryUtil.Setup(provider => provider.ToBinaryString(compressedBytes)).Returns(binaryString);

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

            Assert.AreEqual(binaryString, result);
        }

        [Test]
        public void TestDecode()
        {
            byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
            mockEncryptionProxy.Setup(provider => provider.GenerateKey(RandomSeed, EncryptionUtil.DefaultIterations)).Returns(randomKey);
            string randomKeyString = Convert.ToBase64String(randomKey);

            byte[] decompressBytes = new byte[] { 1 };
            mockCompressionUtil.Setup(provider => provider.Decompress(IsAny<byte[]>())).Returns(decompressBytes);

            byte[] orderedBytes = new byte[] { 2 };
            mockRandomUtil.Setup(util => util.Reorder(decompressBytes, randomKeyString, DummyCount, 2)).Returns(orderedBytes);

            byte[] undummiedBytes = new byte[] { 3 };
            mockDummyUtil.Setup(util => util.RemoveDummies(DummyCount, orderedBytes, randomKeyString)).Returns(undummiedBytes);

            byte[] decryptedBytes = new byte[] { 4 };
            mockEncryptionProxy.Setup(provider => provider.Decrypt(undummiedBytes, Password, AdditionalHashIterations)).Returns(decryptedBytes);

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

            Assert.AreEqual(decryptedBytes, result);
        }
    }
}