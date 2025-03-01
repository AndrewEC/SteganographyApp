namespace SteganographyApp.Common.Tests;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO.Content;

using static Moq.Times;

[TestFixture]
public class ContentReaderTests : FixtureWithTestObjects
{
    [Mockup(typeof(IFileIOProxy))]
    public Mock<IFileIOProxy> MockFileIOProxy = new();

    [Mockup(typeof(IReadWriteStream), true)]
    public Mock<IReadWriteStream> MockReadWriteStream = new();

    [Mockup(typeof(IDataEncoderUtil))]
    public Mock<IDataEncoderUtil> MockDataEncoderUtil = new();

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

    [Test]
    public void TestReadContentChunkFromFile()
    {
        MockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(100);

        string expected = "encoded_value";
        MockDataEncoderUtil.Setup(encoder => encoder.Encode(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(expected);

        using (var reader = new ContentReader(Arguments))
        {
            string? actual = reader.ReadContentChunkFromFile();
            Assert.That(actual, Is.EqualTo(expected));
        }

        MockReadWriteStream.Verify(stream => stream.Flush(), Once());
        MockReadWriteStream.Verify(stream => stream.Dispose(), Once());
        MockFileIOProxy.Verify(provider => provider.OpenFileForRead(FileToEncode), Once());
        MockReadWriteStream.Verify(stream => stream.Read(It.IsAny<byte[]>(), 0, ChunkByteSize), Once());
        MockDataEncoderUtil
            .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == ChunkByteSize), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Once());
    }

    [Test]
    public void TestReadContentChunkFromFile0BitesAreReadreturnsNull()
    {
        MockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);

        using (var contentReader = new ContentReader(Arguments))
        {
            string? result = contentReader.ReadContentChunkFromFile();
            Assert.That(result, Is.Null);
        }

        MockReadWriteStream.Verify(stream => stream.Flush(), Once());
        MockReadWriteStream.Verify(stream => stream.Dispose(), Once());
        MockFileIOProxy.Verify(provider => provider.OpenFileForRead(FileToEncode), Once());
        MockReadWriteStream.Verify(stream => stream.Read(It.IsAny<byte[]>(), 0, ChunkByteSize), Once());
        MockDataEncoderUtil
            .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == ChunkByteSize), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Never());
    }

    [Test]
    public void TestReadContentChunkWhenBitsReadIsLessThanChunkSize()
    {
        int alternateByteCount = 10;
        MockReadWriteStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(alternateByteCount);
        MockDataEncoderUtil.Setup(encoder => encoder.Encode(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns("encoded_value");

        using (var contentReader = new ContentReader(Arguments))
        {
            contentReader.ReadContentChunkFromFile();
        }

        MockDataEncoderUtil
            .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == alternateByteCount), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Once());
    }

    protected override void SetupMocks()
    {
        MockFileIOProxy.Setup(fileProxy => fileProxy.OpenFileForRead(It.IsAny<string>())).Returns(MockReadWriteStream.Object);
        MockReadWriteStream.Setup(stream => stream.Flush()).Verifiable();
        MockReadWriteStream.Setup(stream => stream.Dispose()).Verifiable();
    }
}