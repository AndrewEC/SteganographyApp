using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.Data;

using static Moq.Times;
using static Moq.It;

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

        [Mockup(typeof(IFileProvider))]
        public Mock<IFileProvider> mockFileProvider;

        [Mockup(typeof(IReadWriteStream))]
        public Mock<IReadWriteStream> mockReadWriteStream;

        [Mockup(typeof(IDataEncoderUtil))]
        public Mock<IDataEncoderUtil> mockEncoderUtil;

        protected override void SetupMocks()
        {
            mockFileProvider.Setup(provider => provider.OpenFileForWrite(IsAny<string>())).Returns(mockReadWriteStream.Object);
        }

        [Test]
        public void TestWriteContentChunkToFile()
        {
            byte[] bytes = new byte[1024];
            mockFileProvider.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(false);
            mockEncoderUtil.Setup(encoder => encoder.Decode(IsAny<string>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>()))
                .Returns(bytes);

            using (var writer = new ContentWriter(Arguments))
            {
                writer.WriteContentChunkToFile(BinaryString);
            }

            mockFileProvider.Verify(provider => provider.IsExistingFile(DecodedOutputFile), Once());
            mockFileProvider.Verify(provider => provider.Delete(IsAny<string>()), Never());
            mockFileProvider.Verify(provider => provider.OpenFileForWrite(DecodedOutputFile), Once());

            mockEncoderUtil.Verify(encoder => encoder.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed), Once());

            mockReadWriteStream.Verify(stream => stream.Flush(), AtLeastOnce());
            mockReadWriteStream.Verify(stream => stream.Dispose(), Once());
            mockReadWriteStream.Verify(stream => stream.Write(bytes, 0, bytes.Length), Once());
        }

        [Test]
        public void TestWriteContentChunkToFileWhenOutputFileExistsTriesToDeleteFileFirst()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(true);
            mockEncoderUtil.Setup(encoder => encoder.Decode(IsAny<string>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>()))
                .Returns(new byte[1024]);

            using (var writer = new ContentWriter(Arguments))
            {
                writer.WriteContentChunkToFile(BinaryString);
            }

            mockFileProvider.Verify(provider => provider.Delete(DecodedOutputFile), Once());
            mockEncoderUtil.Verify(encoder => encoder.Decode(BinaryString, Password, UseCompression, DummyCount, RandomSeed), Once());            
        }

    }

}