using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class ContentWriterTests : FixtureWithTestObjects
    {

        private static readonly string BinaryString = "00010010100100111110101001001001";

        private static readonly int ChunkByteSize = 100;
        private static readonly string DecodedOutputFile = "file_to_encode";
        private static readonly string Password = "password";
        private static readonly bool UseCompression = true;
        private static readonly int DummyCount = 10;
        private static readonly string RandomSeed = "randomSeed";

        private static readonly IInputArguments Arguments = new InputArguments
        {
            ChunkByteSize = ChunkByteSize,
            DecodedOutputFile = DecodedOutputFile,
            Password = Password,
            UseCompression = UseCompression,
            DummyCount = DummyCount,
            RandomSeed = RandomSeed
        }
        .ToImmutable();

        private Mock<IFileProvider> mockFileProvider;
        private Mock<IReadWriteStream> mockReadWriteStream;
        private Mock<IDataEncoderUtil> mockEncoderUtil;

        [SetUp]
        public void ContentWriterSetUp()
        {
            mockReadWriteStream = new Mock<IReadWriteStream>();

            mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(provider => provider.OpenFileForWrite(It.IsAny<string>())).Returns(mockReadWriteStream.Object);
            Injector.UseInstance(mockFileProvider.Object);

            mockEncoderUtil = new Mock<IDataEncoderUtil>();
            Injector.UseInstance(mockEncoderUtil.Object);
        }

        [Test]
        public void TestWriteContentChunkToFile()
        {
            byte[] bytes = new byte[1024];
            mockFileProvider.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(false);
            mockEncoderUtil.Setup(encoder => encoder.Decode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(bytes);

            using (var writer = new ContentWriter(Arguments))
            {
                writer.WriteContentChunkToFile(BinaryString);
            }

            mockFileProvider.Verify(provider => provider.IsExistingFile(DecodedOutputFile), Times.Once());
            mockFileProvider.Verify(provider => provider.Delete(It.IsAny<string>()), Times.Never());
            mockFileProvider.Verify(provider => provider.OpenFileForWrite(DecodedOutputFile), Times.Once());

            mockEncoderUtil.Verify(encoder => encoder.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed), Times.Once());

            mockReadWriteStream.Verify(stream => stream.Flush(), Times.AtLeastOnce());
            mockReadWriteStream.Verify(stream => stream.Dispose(), Times.Once());
            mockReadWriteStream.Verify(stream => stream.Write(bytes, 0, bytes.Length), Times.Once());
        }

        [Test]
        public void TestWriteContentChunkToFileWhenOutputFileExistsTriesToDeleteFileFirst()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(true);
            mockEncoderUtil.Setup(encoder => encoder.Decode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new byte[1024]);

            using (var writer = new ContentWriter(Arguments))
            {
                writer.WriteContentChunkToFile(BinaryString);
            }

            mockFileProvider.Verify(provider => provider.Delete(DecodedOutputFile), Times.Once());
            mockEncoderUtil.Verify(encoder => encoder.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed), Times.Once());            
        }

    }

}