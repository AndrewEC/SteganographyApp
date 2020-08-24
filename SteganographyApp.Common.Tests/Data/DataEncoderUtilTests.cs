using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{
    [TestFixture]
    public class DataEncoderUtilTests : FixtureWithTestObjects
    {

        private static readonly byte[] InputBytes = new byte[1];
        private static readonly string Password = "password";
        private static readonly bool UseCompression = false;
        private static readonly int DummyCount = 10;
        private static readonly string RandomSeed = "randomSeed";

        private static readonly string StringToDecode = "stringToDecode";

        [Mockup(typeof(IEncryptionProvider))]
        public Mock<IEncryptionProvider> mockEncryptionProvider;

        [Mockup(typeof(IBinaryUtil))]
        public Mock<IBinaryUtil> mockBinaryUtil;

        [Mockup(typeof(IDummyUtil))]
        public Mock<IDummyUtil> mockDummyUtil;

        [Mockup(typeof(IRandomizeUtil))]
        public Mock<IRandomizeUtil> mockRandomUtil;

        [Mockup(typeof(ICompressionUtil))]
        public Mock<ICompressionUtil> mockCompressionUtil;

        [Test]
        public void TestEncode()
        {
            string encryptedString = "encrypted_string";
            mockEncryptionProvider.Setup(provider => provider.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(encryptedString);

            string binaryString = "binary_string";
            mockBinaryUtil.Setup(provider => provider.ToBinaryString(It.IsAny<string>())).Returns(binaryString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(provider => provider.InsertDummies(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(dummyString);

            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(provider => provider.RandomizeBinaryString(It.IsAny<string>(), It.IsAny<string>())).Returns(randomizedString);

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed);

            Assert.AreEqual(randomizedString, result);

            mockEncryptionProvider.Verify(provider => provider.Encrypt(It.IsAny<string>(), Password), Times.Once());
            mockBinaryUtil.Verify(util => util.ToBinaryString(encryptedString), Times.Once());
            mockDummyUtil.Verify(util => util.InsertDummies(DummyCount, binaryString, RandomSeed), Times.Once());
            mockRandomUtil.Verify(util => util.RandomizeBinaryString(dummyString, RandomSeed), Times.Once());
            mockCompressionUtil.Verify(util => util.Compress(It.IsAny<byte[]>()), Times.Never());
        }

        [Test]
        public void TestEncodeWithCompressionEnabled()
        {
            mockEncryptionProvider.Setup(provider => provider.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns("encrypted_string");
            mockBinaryUtil.Setup(util => util.ToBinaryString(It.IsAny<string>())).Returns("binary_string");
            mockDummyUtil.Setup(util => util.InsertDummies(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns("dummy_string");
            mockRandomUtil.Setup(util => util.RandomizeBinaryString(It.IsAny<string>(), It.IsAny<string>())).Returns("randomized_string");

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, true, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Compress(InputBytes), Times.Once());
        }

        [Test]
        public void TestDecode()
        {
            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(util => util.ReorderBinaryString(It.IsAny<string>(), It.IsAny<string>())).Returns(randomizedString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(util => util.RemoveDummies(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(dummyString);

            string base64String = "base64String";
            mockBinaryUtil.Setup(util => util.ToBase64String(It.IsAny<string>())).Returns(base64String);

            string encryptedString = "ZW5jcnlwdGVkX3N0cmluZw==";
            mockEncryptionProvider.Setup(provider => provider.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(encryptedString);

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed);

            mockRandomUtil.Verify(provider => provider.ReorderBinaryString(StringToDecode, RandomSeed), Times.Once());
            mockDummyUtil.Verify(provider => provider.RemoveDummies(DummyCount, randomizedString, RandomSeed), Times.Once());
            mockBinaryUtil.Verify(provider => provider.ToBase64String(dummyString), Times.Once());
            mockEncryptionProvider.Verify(provider => provider.Decrypt(base64String, Password), Times.Once());
            mockCompressionUtil.Verify(util => util.Decompress(It.IsAny<byte[]>()), Times.Never());
        }

        [Test]
        public void TestDecodWithCompressionDisabled()
        {
            mockRandomUtil.Setup(util => util.ReorderBinaryString(It.IsAny<string>(), It.IsAny<string>())).Returns("randomized_string");
            mockDummyUtil.Setup(util => util.RemoveDummies(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns("dummy_string");
            mockBinaryUtil.Setup(util => util.ToBase64String(It.IsAny<string>())).Returns("base64String");
            mockEncryptionProvider.Setup(provider => provider.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns("ZW5jcnlwdGVkX3N0cmluZw==");

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, false, DummyCount, RandomSeed);

            mockCompressionUtil.Verify(util => util.Decompress(It.IsAny<byte[]>()), Times.Never());
        }

    }
}
