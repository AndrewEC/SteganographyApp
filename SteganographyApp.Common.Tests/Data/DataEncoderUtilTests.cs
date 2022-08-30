namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Text;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    using static Moq.It;
    using static Moq.Times;

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
        private const bool UseCompression = false;
        private const int DummyCount = 10;
        private const string RandomSeed = "randomSeed";
        private const string StringToDecode = "stringToDecode";
        private static readonly byte[] InputBytes = new byte[1];

        [Test]
        public void TestEncode()
        {
            byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
            mockEncryptionProxy.Setup(provider => provider.GenerateKey(RandomSeed, EncryptionUtil.DefaultIterations)).Returns(randomKey);
            string randomKeyString = Convert.ToBase64String(randomKey);

            string encryptedString = "encrypted_string";
            mockEncryptionProxy.Setup(provider => provider.Encrypt(IsAny<string>(), Password)).Returns(encryptedString);

            string binaryString = "binary_string";
            mockBinaryUtil.Setup(provider => provider.ToBinaryString(encryptedString)).Returns(binaryString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(provider => provider.InsertDummies(DummyCount, binaryString, randomKeyString)).Returns(dummyString);

            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(provider => provider.RandomizeBinaryString(dummyString, randomKeyString, DummyCount)).Returns(randomizedString);

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed);

            Assert.AreEqual(randomizedString, result);

            mockCompressionUtil.Verify(util => util.Compress(IsAny<byte[]>()), Never());
        }

        [Test]
        public void TestEncodeWithCompressionEnabled()
        {
            mockEncryptionProxy.Setup(provider => provider.Encrypt(IsAny<string>(), IsAny<string>())).Returns("encrypted_string");
            mockBinaryUtil.Setup(util => util.ToBinaryString(IsAny<string>())).Returns("binary_string");
            mockDummyUtil.Setup(util => util.InsertDummies(IsAny<int>(), IsAny<string>(), IsAny<string>())).Returns("dummy_string");
            mockRandomUtil.Setup(util => util.RandomizeBinaryString(IsAny<string>(), IsAny<string>(), IsAny<int>())).Returns("randomized_string");

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, true, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Compress(InputBytes), Once());
        }

        [Test]
        public void TestDecode()
        {
            byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
            mockEncryptionProxy.Setup(provider => provider.GenerateKey(RandomSeed, EncryptionUtil.DefaultIterations)).Returns(randomKey);
            string randomKeyString = Convert.ToBase64String(randomKey);

            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(util => util.ReorderBinaryString(StringToDecode, randomKeyString, DummyCount)).Returns(randomizedString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(util => util.RemoveDummies(DummyCount, randomizedString, randomKeyString)).Returns(dummyString);

            string base64String = "base64String";
            mockBinaryUtil.Setup(util => util.ToBase64String(dummyString)).Returns(base64String);

            string decryptedString = "ZW5jcnlwdGVkX3N0cmluZw==";
            mockEncryptionProxy.Setup(provider => provider.Decrypt(base64String, Password)).Returns(decryptedString);

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Decompress(IsAny<byte[]>()), Never());
        }

        [Test]
        public void TestDecodWithCompressionDisabled()
        {
            mockRandomUtil.Setup(util => util.ReorderBinaryString(IsAny<string>(), IsAny<string>(), IsAny<int>())).Returns("randomized_string");
            mockDummyUtil.Setup(util => util.RemoveDummies(IsAny<int>(), IsAny<string>(), IsAny<string>())).Returns("dummy_string");
            mockBinaryUtil.Setup(util => util.ToBase64String(IsAny<string>())).Returns("base64String");
            mockEncryptionProxy.Setup(provider => provider.Decrypt(IsAny<string>(), IsAny<string>())).Returns("ZW5jcnlwdGVkX3N0cmluZw==");

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, false, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Decompress(IsAny<byte[]>()), Never());
        }
    }
}