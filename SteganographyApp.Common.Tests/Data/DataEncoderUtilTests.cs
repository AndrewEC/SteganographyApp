namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Text;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

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
        private const int AdditionalHashIterations = 20;
        private const int IterationMultiplier = 5;
        private const int RandomSeedHashIterations = 415_000;
        private static readonly byte[] InputBytes = new byte[1];

        [Test]
        public void TestEncode()
        {
            byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
            mockEncryptionProxy.Setup(encryptionProxy => encryptionProxy.GenerateKey(RandomSeed, RandomSeedHashIterations + AdditionalHashIterations)).Returns(randomKey);
            string randomKeyString = Convert.ToBase64String(randomKey);

            byte[] encryptedBytes = new byte[] { 1 };
            mockEncryptionProxy.Setup(encryptionProxy => encryptionProxy.Encrypt(InputBytes, Password, AdditionalHashIterations)).Returns(encryptedBytes);

            byte[] dummyBytes = new byte[] { 2 };
            mockDummyUtil.Setup(dummyUtil => dummyUtil.InsertDummies(DummyCount, encryptedBytes, randomKeyString)).Returns(dummyBytes);

            byte[] randomizedBytes = new byte[] { 3 };
            mockRandomUtil.Setup(randomUtil => randomUtil.Randomize(dummyBytes, randomKeyString, DummyCount, IterationMultiplier)).Returns(randomizedBytes);

            byte[] compressedBytes = new byte[] { 4 };
            mockCompressionUtil.Setup(compressionUtil => compressionUtil.Compress(randomizedBytes)).Returns(compressedBytes);

            string binaryString = "binary_string";
            mockBinaryUtil.Setup(binaryUtil => binaryUtil.ToBinaryString(compressedBytes)).Returns(binaryString);

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

            Assert.AreEqual(binaryString, result);
        }

        [Test]
        public void TestDecode()
        {
            byte[] stringToDecodeBytes = new byte[] { 11 };
            mockBinaryUtil.Setup(binaryUtil => binaryUtil.ToBytes(StringToDecode)).Returns(stringToDecodeBytes);

            byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
            mockEncryptionProxy.Setup(encryption => encryption.GenerateKey(RandomSeed, RandomSeedHashIterations + AdditionalHashIterations)).Returns(randomKey);
            string randomKeyString = Convert.ToBase64String(randomKey);

            byte[] decompressBytes = new byte[] { 1 };
            mockCompressionUtil.Setup(compressionUtil => compressionUtil.Decompress(stringToDecodeBytes)).Returns(decompressBytes);

            byte[] orderedBytes = new byte[] { 2 };
            mockRandomUtil.Setup(randomUtil => randomUtil.Reorder(decompressBytes, randomKeyString, DummyCount, IterationMultiplier)).Returns(orderedBytes);

            byte[] undummiedBytes = new byte[] { 3 };
            mockDummyUtil.Setup(dummyUtil => dummyUtil.RemoveDummies(DummyCount, orderedBytes, randomKeyString)).Returns(undummiedBytes);

            byte[] decryptedBytes = new byte[] { 4 };
            mockEncryptionProxy.Setup(encryption => encryption.Decrypt(undummiedBytes, Password, AdditionalHashIterations)).Returns(decryptedBytes);

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

            Assert.AreEqual(decryptedBytes, result);
        }
    }
}