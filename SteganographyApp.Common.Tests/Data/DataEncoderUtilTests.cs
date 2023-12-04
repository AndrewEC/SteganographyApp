namespace SteganographyApp.Common.Tests;

using System;
using System.Text;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;

[TestFixture]
public class DataEncoderUtilTests : FixtureWithTestObjects
{
    [Mockup(typeof(IEncryptionUtil))]
    public Mock<IEncryptionUtil> MockEncryptionProxy = new();

    [Mockup(typeof(IBinaryUtil))]
    public Mock<IBinaryUtil> MockBinaryUtil = new();

    [Mockup(typeof(IDummyUtil))]
    public Mock<IDummyUtil> MockDummyUtil = new();

    [Mockup(typeof(IRandomizeUtil))]
    public Mock<IRandomizeUtil> MockRandomUtil = new();

    [Mockup(typeof(ICompressionUtil))]
    public Mock<ICompressionUtil> MockCompressionUtil = new();

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
        MockEncryptionProxy.Setup(encryptionProxy => encryptionProxy.GenerateKey(RandomSeed, RandomSeedHashIterations + AdditionalHashIterations)).Returns(randomKey);
        string randomKeyString = Convert.ToBase64String(randomKey);

        byte[] encryptedBytes = [1];
        MockEncryptionProxy.Setup(encryptionProxy => encryptionProxy.Encrypt(InputBytes, Password, AdditionalHashIterations)).Returns(encryptedBytes);

        byte[] dummyBytes = [2];
        MockDummyUtil.Setup(dummyUtil => dummyUtil.InsertDummies(DummyCount, encryptedBytes, randomKeyString)).Returns(dummyBytes);

        byte[] randomizedBytes = [3];
        MockRandomUtil.Setup(randomUtil => randomUtil.Randomize(dummyBytes, randomKeyString, DummyCount, IterationMultiplier)).Returns(randomizedBytes);

        byte[] compressedBytes = [4];
        MockCompressionUtil.Setup(compressionUtil => compressionUtil.Compress(randomizedBytes)).Returns(compressedBytes);

        string binaryString = "binary_string";
        MockBinaryUtil.Setup(binaryUtil => binaryUtil.ToBinaryString(compressedBytes)).Returns(binaryString);

        var util = new DataEncoderUtil();

        string result = util.Encode(InputBytes, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

        Assert.That(result, Is.EqualTo(binaryString));
    }

    [Test]
    public void TestDecode()
    {
        byte[] stringToDecodeBytes = [11];
        MockBinaryUtil.Setup(binaryUtil => binaryUtil.ToBytes(StringToDecode)).Returns(stringToDecodeBytes);

        byte[] randomKey = Encoding.UTF8.GetBytes("random_key");
        MockEncryptionProxy.Setup(encryption => encryption.GenerateKey(RandomSeed, RandomSeedHashIterations + AdditionalHashIterations)).Returns(randomKey);
        string randomKeyString = Convert.ToBase64String(randomKey);

        byte[] decompressBytes = [1];
        MockCompressionUtil.Setup(compressionUtil => compressionUtil.Decompress(stringToDecodeBytes)).Returns(decompressBytes);

        byte[] orderedBytes = [2];
        MockRandomUtil.Setup(randomUtil => randomUtil.Reorder(decompressBytes, randomKeyString, DummyCount, IterationMultiplier)).Returns(orderedBytes);

        byte[] undummiedBytes = [3];
        MockDummyUtil.Setup(dummyUtil => dummyUtil.RemoveDummies(DummyCount, orderedBytes, randomKeyString)).Returns(undummiedBytes);

        byte[] decryptedBytes = [4];
        MockEncryptionProxy.Setup(encryption => encryption.Decrypt(undummiedBytes, Password, AdditionalHashIterations)).Returns(decryptedBytes);

        var util = new DataEncoderUtil();

        byte[] result = util.Decode(StringToDecode, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations);

        Assert.That(result, Is.EqualTo(decryptedBytes));
    }
}