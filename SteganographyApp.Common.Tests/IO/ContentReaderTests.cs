namespace SteganographyApp.Common.Tests
{
    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;

    using static Moq.It;
    using static Moq.Times;

    [TestFixture]
    public class ContentReaderTests : FixtureWithTestObjects
    {
        [Mockup(typeof(IFileProvider))]
        public Mock<IFileProvider> mockFileProvider;

        [Mockup(typeof(IReadWriteStream))]
        public Mock<IReadWriteStream> mockReadWriteStream;

        [Mockup(typeof(IDataEncoderUtil))]
        public Mock<IDataEncoderUtil> mockDataEncoderUtil;

        private static readonly int ChunkByteSize = 100;
        private static readonly string FileToEncode = "file_to_encode";
        private static readonly string Password = "password";
        private static readonly bool UseCompression = true;
        private static readonly int DummyCount = 10;
        private static readonly string RandomSeed = "randomSeed";

        private static readonly IInputArguments Arguments = new InputArguments
        {
            ChunkByteSize = ChunkByteSize,
            FileToEncode = FileToEncode,
            Password = Password,
            UseCompression = UseCompression,
            DummyCount = DummyCount,
            RandomSeed = RandomSeed,
        }
        .ToImmutable();

        [Test]
        public void TestReadContentChunkFromFile()
        {
            mockReadWriteStream.Setup(stream => stream.Read(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Returns(100);

            string expected = "encoded_value";
            mockDataEncoderUtil.Setup(encoder => encoder.Encode(IsAny<byte[]>(), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>()))
                .Returns("encoded_value");

            using (var reader = new ContentReader(Arguments))
            {
                string actual = reader.ReadContentChunkFromFile();
                Assert.AreEqual(expected, actual);
            }

            mockReadWriteStream.Verify(stream => stream.Flush(), AtLeastOnce());
            mockReadWriteStream.Verify(stream => stream.Dispose(), Once());
            mockFileProvider.Verify(provider => provider.OpenFileForRead(FileToEncode), Once());
            mockReadWriteStream.Verify(stream => stream.Read(IsAny<byte[]>(), 0, ChunkByteSize), Once());
            mockDataEncoderUtil
                .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == ChunkByteSize), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>()), Once());
        }

        [Test]
        public void TestReadContentChunkFromFile0BitesAreReadreturnsNull()
        {
            mockReadWriteStream.Setup(stream => stream.Read(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Returns(0);

            using (var contentReader = new ContentReader(Arguments))
            {
                string result = contentReader.ReadContentChunkFromFile();
                Assert.IsNull(result);
            }

            mockReadWriteStream.Verify(stream => stream.Flush(), AtLeastOnce());
            mockReadWriteStream.Verify(stream => stream.Dispose(), Once());
            mockFileProvider.Verify(provider => provider.OpenFileForRead(FileToEncode), Once());
            mockReadWriteStream.Verify(stream => stream.Read(IsAny<byte[]>(), 0, ChunkByteSize), Once());
            mockDataEncoderUtil
                .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == ChunkByteSize), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>()), Never());
        }

        [Test]
        public void TestReadContentChunkWhenBitsReadIsLessThanChunkSize()
        {
            int alternateByteCount = 10;
            mockReadWriteStream.Setup(stream => stream.Read(IsAny<byte[]>(), IsAny<int>(), IsAny<int>())).Returns(alternateByteCount);

            using (var contentReader = new ContentReader(Arguments))
            {
                string result = contentReader.ReadContentChunkFromFile();
            }

            mockDataEncoderUtil
                .Verify(encoder => encoder.Encode(It.Is<byte[]>(bytes => bytes.Length == alternateByteCount), IsAny<string>(), IsAny<bool>(), IsAny<int>(), IsAny<string>()), Once());
        }

        protected override void SetupMocks()
        {
            mockFileProvider.Setup(provider => provider.OpenFileForRead(IsAny<string>())).Returns(mockReadWriteStream.Object);
        }
    }
}