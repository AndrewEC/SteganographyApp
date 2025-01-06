namespace SteganographyApp.Common.Tests;

using Moq;
using NUnit.Framework;

using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;

[TestFixture]
public class ChunkTableWriterTests
{
    private const string ChunkTableBinary = "000000000000000001000000000000000000000000000000010";

    [Test]
    public void TestWriteContentChunkTable()
    {
        Mock<IImageStore> mockStore = new(MockBehavior.Strict);
        Mock<IImageStoreStream> mockStream = new(MockBehavior.Strict);

        mockStore.Setup(store => store.OpenStream(StreamMode.Write))
            .Returns(mockStream.Object);

        mockStream.Setup(stream => stream.WriteContentChunkToImage(ChunkTableBinary))
            .Returns(51);

        mockStream.Setup(stream => stream.Dispose()).Verifiable();

        using (var writer = new ChunkTableWriter(mockStore.Object))
        {
            writer.WriteContentChunkTable([2]);
        }

        mockStream.Verify(stream => stream.Dispose(), Times.Exactly(1));
    }
}