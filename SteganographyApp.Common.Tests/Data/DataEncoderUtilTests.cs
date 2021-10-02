namespace SteganographyApp.Common.Tests
{
    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

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

        private static readonly byte[] InputBytes = new byte[1];
        private static readonly string Password = "password";
        private static readonly bool UseCompression = false;
        private static readonly int DummyCount = 10;
        private static readonly string RandomSeed = "randomSeed";

        private static readonly string StringToDecode = "stringToDecode";

        [Test]
        public void TestEncode()
        {
            string encryptedString = "encrypted_string";
            mockEncryptionProxy.Setup(provider => provider.Encrypt(IsAny<string>(), IsAny<string>())).Returns(encryptedString);

            string binaryString = "binary_string";
            mockBinaryUtil.Setup(provider => provider.ToBinaryString(IsAny<string>())).Returns(binaryString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(provider => provider.InsertDummies(IsAny<int>(), IsAny<string>(), IsAny<string>())).Returns(dummyString);

            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(provider => provider.RandomizeBinaryString(IsAny<string>(), IsAny<string>())).Returns(randomizedString);

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed);

            Assert.AreEqual(randomizedString, result);

            mockEncryptionProxy.Verify(provider => provider.Encrypt(IsAny<string>(), Password), Once());
            mockBinaryUtil.Verify(util => util.ToBinaryString(encryptedString), Once());
            mockDummyUtil.Verify(util => util.InsertDummies(DummyCount, binaryString, RandomSeed), Once());
            mockRandomUtil.Verify(util => util.RandomizeBinaryString(dummyString, RandomSeed), Once());
            mockCompressionUtil.Verify(util => util.Compress(IsAny<byte[]>()), Never());
        }

        [Test]
        public void TestEncodeWithCompressionEnabled()
        {
            mockEncryptionProxy.Setup(provider => provider.Encrypt(IsAny<string>(), IsAny<string>())).Returns("encrypted_string");
            mockBinaryUtil.Setup(util => util.ToBinaryString(IsAny<string>())).Returns("binary_string");
            mockDummyUtil.Setup(util => util.InsertDummies(IsAny<int>(), IsAny<string>(), IsAny<string>())).Returns("dummy_string");
            mockRandomUtil.Setup(util => util.RandomizeBinaryString(IsAny<string>(), IsAny<string>())).Returns("randomized_string");

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, true, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Compress(InputBytes), Once());
        }

        [Test]
        public void TestDecode()
        {
            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(util => util.ReorderBinaryString(IsAny<string>(), IsAny<string>())).Returns(randomizedString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(util => util.RemoveDummies(IsAny<int>(), IsAny<string>(), IsAny<string>())).Returns(dummyString);

            string base64String = "base64String";
            mockBinaryUtil.Setup(util => util.ToBase64String(IsAny<string>())).Returns(base64String);

            string encryptedString = "ZW5jcnlwdGVkX3N0cmluZw==";
            mockEncryptionProxy.Setup(provider => provider.Decrypt(IsAny<string>(), IsAny<string>())).Returns(encryptedString);

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed);

            mockRandomUtil.Verify(provider => provider.ReorderBinaryString(StringToDecode, RandomSeed), Once());
            mockDummyUtil.Verify(provider => provider.RemoveDummies(DummyCount, randomizedString, RandomSeed), Once());
            mockBinaryUtil.Verify(provider => provider.ToBase64String(dummyString), Once());
            mockEncryptionProxy.Verify(provider => provider.Decrypt(base64String, Password), Once());
            mockCompressionUtil.Verify(util => util.Decompress(IsAny<byte[]>()), Never());
        }

        [Test]
        public void TestDecodWithCompressionDisabled()
        {
            mockRandomUtil.Setup(util => util.ReorderBinaryString(IsAny<string>(), IsAny<string>())).Returns("randomized_string");
            mockDummyUtil.Setup(util => util.RemoveDummies(IsAny<int>(), IsAny<string>(), IsAny<string>())).Returns("dummy_string");
            mockBinaryUtil.Setup(util => util.ToBase64String(IsAny<string>())).Returns("base64String");
            mockEncryptionProxy.Setup(provider => provider.Decrypt(IsAny<string>(), IsAny<string>())).Returns("ZW5jcnlwdGVkX3N0cmluZw==");

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, false, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Decompress(IsAny<byte[]>()), Never());
        }
    }
}