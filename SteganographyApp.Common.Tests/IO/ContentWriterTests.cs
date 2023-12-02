namespace SteganographyApp.Common.Tests;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;

using static Moq.It;
using static Moq.Times;

[TestFixture]
public class ContentWriterTests : FixtureWithTestObjects
{
    [Mockup(typeof(IFileIOProxy))]
    public Mock<IFileIOProxy> mockFileIOProxy;

    [Mockup(typeof(IReadWriteStream))]
    public Mock<IReadWriteStream> mockReadWriteStream;

    [Mockup(typeof(IDataEncoderUtil))]
    public Mock<IDataEncoderUtil> mockEncoderUtil;

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
        mockFileIOProxy.Setup(fileProxy => fileProxy.IsExistingFile(IsAny<string>())).Returns(false);
        mockEncoderUtil.Setup(encoder => encoder.Decode(IsAny<string>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>(), IsAny<int>()))
            .Returns(bytes);
        mockReadWriteStream.Setup(stream => stream.Write(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Verifiable();

        using (var writer = new ContentWriter(Arguments))
        {
            writer.WriteContentChunkToFile(BinaryString);
        }

        mockFileIOProxy.Verify(fileProxy => fileProxy.IsExistingFile(DecodedOutputFile), Once());
        mockFileIOProxy.Verify(fileProxy => fileProxy.Delete(IsAny<string>()), Never());
        mockFileIOProxy.Verify(fileProxy => fileProxy.OpenFileForWrite(DecodedOutputFile), Once());

        mockEncoderUtil.Verify(encoderUtil => encoderUtil.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations), Once());

        mockReadWriteStream.Verify(stream => stream.Flush(), AtLeastOnce());
        mockReadWriteStream.Verify(stream => stream.Dispose(), Once());
        mockReadWriteStream.Verify(stream => stream.Write(bytes, 0, bytes.Length), Once());
    }

    [Test]
    public void TestWriteContentChunkToFileWhenOutputFileExistsTriesToDeleteFileFirst()
    {
        mockFileIOProxy.Setup(fileProxy => fileProxy.IsExistingFile(DecodedOutputFile)).Returns(true);
        mockEncoderUtil.Setup(encoderUtil => encoderUtil.Decode(IsAny<string>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>(), IsAny<int>()))
            .Returns(new byte[1024]);
        mockFileIOProxy.Setup(fileProxy => fileProxy.Delete(DecodedOutputFile)).Verifiable();
        mockReadWriteStream.Setup(stream => stream.Write(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Verifiable();

        using (var writer = new ContentWriter(Arguments))
        {
            writer.WriteContentChunkToFile(BinaryString);
        }

        mockFileIOProxy.Verify(provider => provider.Delete(DecodedOutputFile), Once());
        mockEncoderUtil.Verify(encoder => encoder.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed, AdditionalHashIterations), Once());
    }

    protected override void SetupMocks()
    {
        mockFileIOProxy.Setup(fileProxy => fileProxy.OpenFileForWrite(DecodedOutputFile)).Returns(mockReadWriteStream.Object);
        mockReadWriteStream.Setup(stream => stream.Flush()).Verifiable();
        mockReadWriteStream.Setup(stream => stream.Dispose()).Verifiable();
    }
}