using Moq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Providers;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class DataEncoderUtilTests
    {

        private static readonly byte[] InputBytes = new byte[1];
        private static readonly string Password = "password";
        private static readonly bool UseCompression = false;
        private static readonly int DummyCount = 10;
        private static readonly string RandomSeed = "randomSeed";

        private static readonly string StringToDecode = "stringToDecode";

        private Mock<IEncryptionProvider> mockEncryptionProvider;
        private Mock<IBinaryUtil> mockBinaryUtil;
        private Mock<IDummyUtil> mockDummyUtil;
        private Mock<IRandomizeUtil> mockRandomUtil;

        [TestInitialize]
        public void Initialize()
        {
            mockEncryptionProvider = new Mock<IEncryptionProvider>();
            mockBinaryUtil = new Mock<IBinaryUtil>();
            mockDummyUtil = new Mock<IDummyUtil>();
            mockRandomUtil = new Mock<IRandomizeUtil>();

            Injector.UseProvider<IEncryptionProvider>(mockEncryptionProvider.Object);
            Injector.UseProvider<IBinaryUtil>(mockBinaryUtil.Object);
            Injector.UseProvider<IDummyUtil>(mockDummyUtil.Object);
            Injector.UseProvider<IRandomizeUtil>(mockRandomUtil.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Injector.ResetProviders();
        }

        [TestMethod]
        public void TestEncode()
        {
            string encryptedString = "encrypted_string";
            mockEncryptionProvider.Setup(provider => provider.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(encryptedString);

            string binaryString = "binary_string";
            mockBinaryUtil.Setup(provider => provider.ToBinaryString(It.IsAny<string>())).Returns(binaryString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(provider => provider.InsertDummies(It.IsAny<int>(), It.IsAny<string>())).Returns(dummyString);

            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(provider => provider.RandomizeBinaryString(It.IsAny<string>(), It.IsAny<string>())).Returns(randomizedString);

            var util = new DataEncoderUtil();

            string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed);

            Assert.AreEqual(randomizedString, result);

            mockEncryptionProvider.Verify(provider => provider.Encrypt(It.IsAny<string>(), Password), Times.Once());
            mockBinaryUtil.Verify(provider => provider.ToBinaryString(encryptedString), Times.Once());
            mockDummyUtil.Verify(provider => provider.InsertDummies(DummyCount, binaryString), Times.Once());
            mockRandomUtil.Verify(provider => provider.RandomizeBinaryString(dummyString, RandomSeed), Times.Once());
        }

        [TestMethod]
        public void TestDecode()
        {
            string randomizedString = "randomized_string";
            mockRandomUtil.Setup(provider => provider.ReorderBinaryString(It.IsAny<string>(), It.IsAny<string>())).Returns(randomizedString);

            string dummyString = "dummy_string";
            mockDummyUtil.Setup(provider => provider.RemoveDummies(It.IsAny<int>(), It.IsAny<string>())).Returns(dummyString);

            string base64String = "base64String";
            mockBinaryUtil.Setup(provider => provider.ToBase64String(It.IsAny<string>())).Returns(base64String);

            string encryptedString = "ZW5jcnlwdGVkX3N0cmluZw==";
            mockEncryptionProvider.Setup(provider => provider.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns(encryptedString);

            var util = new DataEncoderUtil();

            byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed);

            mockRandomUtil.Verify(provider => provider.ReorderBinaryString(StringToDecode, RandomSeed), Times.Once());
            mockDummyUtil.Verify(provider => provider.RemoveDummies(DummyCount, randomizedString), Times.Once());
            mockBinaryUtil.Verify(provider => provider.ToBase64String(dummyString), Times.Once());
            mockEncryptionProvider.Verify(provider => provider.Decrypt(base64String, Password), Times.Once());
        }

    }
}
