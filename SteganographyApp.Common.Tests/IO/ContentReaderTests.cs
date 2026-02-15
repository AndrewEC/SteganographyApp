namespace SteganographyApp.Common.Tests;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO.Content;

[TestFixture]
public class ContentReaderTests
{
    private const int ChunkByteSize = 100;
    private const string FileToEncode = "file_to_encode";
    private const string Password = "password";
    private const bool UseCompression = true;
    private const int DummyCount = 10;
    private const string RandomSeed = "randomSeed";
    private const int AdditionalHashIterations = 2;

    private static readonly IInputArguments Arguments = new CommonArguments
    {
        ChunkByteSize = ChunkByteSize,
        FileToEncode = FileToEncode,
        Password = Password,
        UseCompression = UseCompression,
        DummyCount = DummyCount,
        RandomSeed = RandomSeed,
        AdditionalPasswordHashIterations = AdditionalHashIterations,
    };

    private readonly Mock<IFileIOProxy> mockFileIOProxy = new(MockBehavior.Strict);
    private readonly Mock<IReadWriteStream> mockReadWriteStream = new(MockBehavior.Strict);
    private readonly Mock<IDataEncoderUtil> mockDataEncoderUtil = new(MockBehavior.Strict);

    [SetUp]
    public void SetUp()
    {
        mockFileIOProxy.Setup(fileProxy => fileProxy.OpenFileForRead(It.IsAny<string>()))
            .Returns(mockReadWriteStream.Object);

        mockReadWriteStream.Setup(stream => stream.Flush()).Verifiable();
        mockReadWriteStream.Setup(stream => stream.Dispose()).Verifiable();
    }

    [TearDown]
    public void TearDown()
    {
        mockReadWriteStream.VerifyAll();
        mockFileIOProxy.VerifyAll();
        mockDataEncoderUtil.VerifyAll();

        mockFileIOProxy.Reset();
        mockReadWriteStream.Reset();
        mockDataEncoderUtil.Reset();
    }

    [Test]
    public void TestReadContentChunkFromFile()
    {
        mockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(100)
            .Verifiable();

        string expected = "encoded_value";
        mockDataEncoderUtil.Setup(encoder => encoder.Encode(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(expected)
            .Verifiable();

        using (ContentReader reader = new(Arguments, mockDataEncoderUtil.Object, mockFileIOProxy.Object))
        {
            string? actual = reader.ReadContentChunkFromFile();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    [Test]
    public void TestReadContentChunkFromFile0BitesAreReadreturnsNull()
    {
        mockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(0)
            .Verifiable();

        using (ContentReader contentReader = new(Arguments, mockDataEncoderUtil.Object, mockFileIOProxy.Object))
        {
            string? result = contentReader.ReadContentChunkFromFile();
            Assert.That(result, Is.Null);
        }
        
        mockDataEncoderUtil.VerifyNoOtherCalls();
    }

    [Test]
    public void TestReadContentChunkWhenBitsReadIsLessThanChunkSize()
    {
        int alternateByteCount = 10;
        mockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(alternateByteCount)
            .Verifiable();

        mockDataEncoderUtil.Setup(encoder => encoder.Encode(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns("encoded_value")
            .Verifiable();

        using (ContentReader contentReader = new(Arguments, mockDataEncoderUtil.Object, mockFileIOProxy.Object))
        {
            contentReader.ReadContentChunkFromFile();
        }
    }
}