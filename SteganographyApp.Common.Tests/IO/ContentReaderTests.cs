namespace SteganographyApp.Common.Tests;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO.Content;

using static Moq.Times;

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
        mockFileIOProxy.Reset();
        mockReadWriteStream.Reset();
        mockDataEncoderUtil.Reset();

        mockFileIOProxy.Setup(fileProxy => fileProxy.OpenFileForRead(It.IsAny<string>()))
            .Returns(mockReadWriteStream.Object);

        mockReadWriteStream.Setup(stream => stream.Flush()).Verifiable();
        mockReadWriteStream.Setup(stream => stream.Dispose()).Verifiable();
    }

    [Test]
    public void TestReadContentChunkFromFile()
    {
        mockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(100);

        string expected = "encoded_value";
        mockDataEncoderUtil.Setup(encoder => encoder.Encode(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(expected);

        using (ContentReader reader = new(Arguments, mockDataEncoderUtil.Object, mockFileIOProxy.Object))
        {
            string? actual = reader.ReadContentChunkFromFile();
            Assert.That(actual, Is.EqualTo(expected));
        }

        mockReadWriteStream.Verify(stream => stream.Flush(), Once());
        mockReadWriteStream.Verify(stream => stream.Dispose(), Once());
        mockFileIOProxy.Verify(provider => provider.OpenFileForRead(FileToEncode), Once());
        mockReadWriteStream.Verify(stream => stream.Read(It.IsAny<byte[]>(), 0, ChunkByteSize), Once());
        mockDataEncoderUtil
            .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == ChunkByteSize), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Once());
    }

    [Test]
    public void TestReadContentChunkFromFile0BitesAreReadreturnsNull()
    {
        mockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);

        using (ContentReader contentReader = new(Arguments, mockDataEncoderUtil.Object, mockFileIOProxy.Object))
        {
            string? result = contentReader.ReadContentChunkFromFile();
            Assert.That(result, Is.Null);
        }

        mockReadWriteStream.Verify(stream => stream.Flush(), Once());
        mockReadWriteStream.Verify(stream => stream.Dispose(), Once());
        mockFileIOProxy.Verify(provider => provider.OpenFileForRead(FileToEncode), Once());
        mockReadWriteStream.Verify(stream => stream.Read(It.IsAny<byte[]>(), 0, ChunkByteSize), Once());
        mockDataEncoderUtil
            .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == ChunkByteSize), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Never());
    }

    [Test]
    public void TestReadContentChunkWhenBitsReadIsLessThanChunkSize()
    {
        int alternateByteCount = 10;
        mockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(alternateByteCount);
        mockDataEncoderUtil.Setup(encoder => encoder.Encode(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns("encoded_value");

        using (ContentReader contentReader = new(Arguments, mockDataEncoderUtil.Object, mockFileIOProxy.Object))
        {
            contentReader.ReadContentChunkFromFile();
        }

        mockDataEncoderUtil
            .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == alternateByteCount), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Once());
    }
}