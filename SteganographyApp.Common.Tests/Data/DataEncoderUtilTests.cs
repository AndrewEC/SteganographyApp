namespace SteganographyApp.Common.Tests;

using System;
using System.Text;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;

[TestFixture]
public class DataEncoderUtilTests
{
    private const string Password = "password";
    private const bool UseCompression = true;
    private const int DummyCount = 10;
    private const string RandomSeed = "randomSeed";
    private const string StringToDecode = "stringToDecode";
    private const int AdditionalHashIterations = 20;
    private static readonly byte[] InputBytes = new byte[1];

    private readonly Mock<IKeyUtil> mockKeyUtil = new(MockBehavior.Strict);
    private readonly Mock<IEncryptionUtil> mockEncryptionProxy = new(MockBehavior.Strict);
    private readonly Mock<IBinaryUtil> mockBinaryUtil = new(MockBehavior.Strict);
    private readonly Mock<IDummyUtil> mockDummyUtil = new(MockBehavior.Strict);
    private readonly Mock<IRandomizeUtil> mockRandomUtil = new(MockBehavior.Strict);
    private readonly Mock<ICompressionUtil> mockCompressionUtil = new(MockBehavior.Strict);

    private DataEncoderUtil dataEncoderUtil;

    [SetUp]
    public void SetUp()
    {
        dataEncoderUtil = new(
            mockKeyUtil.Object,
            mockEncryptionProxy.Object,
            mockDummyUtil.Object,
            mockRandomUtil.Object,
            mockCompressionUtil.Object,
            mockBinaryUtil.Object);
    }

    [Test]
    public void TestEncode()
    {
        byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
        mockKeyUtil.Setup(keyUtil => keyUtil.GenerateKey(RandomSeed, AdditionalHashIterations)).Returns(randomKey);
        string randomKeyString = Convert.ToBase64String(randomKey);

        byte[] encryptedBytes = [1];
        mockEncryptionProxy.Setup(encryptionProxy => encryptionProxy.Encrypt(InputBytes, Password, AdditionalHashIterations)).Returns(encryptedBytes);

        byte[] dummyBytes = [2];
        mockDummyUtil.Setup(dummyUtil => dummyUtil.InsertDummies(DummyCount, encryptedBytes, randomKeyString)).Returns(dummyBytes);

        byte[] randomizedBytes = [3];
        mockRandomUtil.Setup(randomUtil => randomUtil.Randomize(dummyBytes, randomKeyString)).Returns(randomizedBytes);

        byte[] compressedBytes = [4];
        mockCompressionUtil.Setup(compressionUtil => compressionUtil.Compress(randomizedBytes)).Returns(compressedBytes);

        string binaryString = "binary_string";
        mockBinaryUtil.Setup(binaryUtil => binaryUtil.ToBinaryString(compressedBytes)).Returns(binaryString);

        string result = dataEncoderUtil.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

        Assert.That(result, Is.EqualTo(binaryString));
    }

    [Test]
    public void TestDecode()
    {
        byte[] stringToDecodeBytes = [11];
        mockBinaryUtil.Setup(binaryUtil => binaryUtil.ToBytes(StringToDecode)).Returns(stringToDecodeBytes);

        byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
        mockKeyUtil.Setup(keyUtil => keyUtil.GenerateKey(RandomSeed, AdditionalHashIterations)).Returns(randomKey);
        string randomKeyString = Convert.ToBase64String(randomKey);

        byte[] decompressBytes = [1];
        mockCompressionUtil.Setup(compressionUtil => compressionUtil.Decompress(stringToDecodeBytes)).Returns(decompressBytes);

        byte[] orderedBytes = [2];
        mockRandomUtil.Setup(randomUtil => randomUtil.Reorder(decompressBytes, randomKeyString)).Returns(orderedBytes);

        byte[] undummiedBytes = [3];
        mockDummyUtil.Setup(dummyUtil => dummyUtil.RemoveDummies(DummyCount, orderedBytes, randomKeyString)).Returns(undummiedBytes);

        byte[] decryptedBytes = [4];
        mockEncryptionProxy.Setup(encryption => encryption.Decrypt(undummiedBytes, Password, AdditionalHashIterations)).Returns(decryptedBytes);

        byte[] result = dataEncoderUtil.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

        Assert.That(result, Is.EqualTo(decryptedBytes));
    }
}