namespace SteganographyApp.Common.Tests;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO.Content;

using static Moq.It;
using static Moq.Times;

[TestFixture]
public class ContentWriterTests : FixtureWithTestObjects
{
    [Mockup(typeof(IFileIOProxy))]
    public Mock<IFileIOProxy> MockFileIOProxy = new();

    [Mockup(typeof(IReadWriteStream))]
    public Mock<IReadWriteStream> MockReadWriteStream = new();

    [Mockup(typeof(IDataEncoderUtil))]
    public Mock<IDataEncoderUtil> MockEncoderUtil = new();

    private const string BinaryString = "00010010100100111110101001001001";

    private const int ChunkByteSize = 100;
    private const string DecodedOutputFile = "file_to_encode";
    private const string Password = "password";
    private const bool UseCompression = true;
    private const int DummyCount = 10;
    private const string RandomSeed = "randomSeed";
    private const int AdditionalHashIterations = 2;

    private static readonly IInputArguments Arguments = new CommonArguments
    {
        ChunkByteSize = ChunkByteSize,
        DecodedOutputFile = DecodedOutputFile,
        Password = Password,
        UseCompression = UseCompression,
        DummyCount = DummyCount,
        RandomSeed = RandomSeed,
        AdditionalPasswordHashIterations = AdditionalHashIterations,
    };

    [Test]
    public void TestWriteContentChunkToFile()
    {
        byte[] bytes = new byte[1024];
        MockFileIOProxy.Setup(fileProxy => fileProxy.IsExistingFile(IsAny<string>())).Returns(false);
        MockEncoderUtil.Setup(encoder => encoder.Decode(IsAny<string>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>(), IsAny<int>()))
            .Returns(bytes);
        MockReadWriteStream.Setup(stream => stream.Write(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Verifiable();

        using (var writer = new ContentWriter(Arguments))
        {
            writer.WriteContentChunkToFile(BinaryString);
        }

        MockFileIOProxy.Verify(fileProxy => fileProxy.IsExistingFile(DecodedOutputFile), Once());
        MockFileIOProxy.Verify(fileProxy => fileProxy.Delete(IsAny<string>()), Never());
        MockFileIOProxy.Verify(fileProxy => fileProxy.OpenFileForWrite(DecodedOutputFile), Once());

        MockEncoderUtil.Verify(encoderUtil => encoderUtil.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations), Once());

        MockReadWriteStream.Verify(stream => stream.Flush(), AtLeastOnce());
        MockReadWriteStream.Verify(stream => stream.Dispose(), Once());
        MockReadWriteStream.Verify(stream => stream.Write(bytes, 0, bytes.Length), Once());
    }

    [Test]
    public void TestWriteContentChunkToFileWhenOutputFileExistsTriesToDeleteFileFirst()
    {
        MockFileIOProxy.Setup(fileProxy => fileProxy.IsExistingFile(DecodedOutputFile)).Returns(true);
        MockEncoderUtil.Setup(encoderUtil => encoderUtil.Decode(IsAny<string>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>(), IsAny<int>()))
            .Returns(new byte[1024]);
        MockFileIOProxy.Setup(fileProxy => fileProxy.Delete(DecodedOutputFile)).Verifiable();
        MockReadWriteStream.Setup(stream => stream.Write(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Verifiable();

        using (var writer = new ContentWriter(Arguments))
        {
            writer.WriteContentChunkToFile(BinaryString);
        }

        MockFileIOProxy.Verify(provider => provider.Delete(DecodedOutputFile), Once());
        MockEncoderUtil.Verify(encoder => encoder.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations), Once());
    }

    protected override void SetupMocks()
    {
        MockFileIOProxy.Setup(fileProxy => fileProxy.OpenFileForWrite(DecodedOutputFile)).Returns(MockReadWriteStream.Object);
        MockReadWriteStream.Setup(stream => stream.Flush()).Verifiable();
        MockReadWriteStream.Setup(stream => stream.Dispose()).Verifiable();
    }
}