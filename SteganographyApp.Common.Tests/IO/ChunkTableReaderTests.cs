namespace SteganographyApp.Common.Tests;

using System.Collections.Immutable;
using Moq;
using NUnit.Framework;

using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;

[TestFixture]
public class ChunkTableReaderTests
{
    private const string HeaderBinary = "000000000000000001";
    private const string HeaderBinaryChunk = "000000000000000000000000000000010";

    [Test]
    public void TestReadContentChunkTable()
    {
        Mock<IImageStoreStream> mockStream = new(MockBehavior.Strict);

        mockStream.Setup(stream => stream.ReadContentChunkFromImage(18))
            .Returns(HeaderBinary);

        mockStream.Setup(stream => stream.ReadContentChunkFromImage(33))
            .Returns(HeaderBinaryChunk);

        ImmutableArray<int> actual = ChunkTableReader.ReadContentChunkTable(mockStream.Object);

        Assert.That(actual, Has.Length.EqualTo(1));
        Assert.That(actual, Has.ItemAt(0).EqualTo(2));
    }
}