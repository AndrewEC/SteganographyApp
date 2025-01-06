namespace SteganographyApp.Common.IO.Content;

using System;
using System.Collections.Immutable;
using System.Linq;

using SteganographyApp.Common.Logging;

/// <summary>
/// Responsible for reading the content chunk table from the leading cover image.
/// </summary>
/// <param name="stream">The stream opened from the image store.</param>
public class ChunkTableReader(IImageStoreStream stream)
{
    private readonly ILogger log = new LazyLogger<ChunkTableReader>();

    private readonly IImageStoreStream stream = stream;

    /// <summary>
    /// Reads in and returns an array in which each element represents the number of bits in a chunk. This will
    /// advance the <see cref="ImageStoreStream"/> by the number of pixels required to read in the full
    /// content chunk table.
    /// </summary>
    /// <returns>An immutable array in whcih each element specifies the number of bits per chunk saved in the
    /// cover images.</returns>
    public ImmutableArray<int> ReadContentChunkTable()
    {
        log.Trace("Reading content chunk table.");

        short chunkCount = ReadChunkCount();
        log.Debug("Chunk table contains [{0}] chunks.", chunkCount);

        return ReadTableChunkLengths(chunkCount);
    }

    private static string NextBinaryChunk(int index, string binaryString)
        => binaryString.Substring(index * Calculator.ChunkDefinitionBitSizeWithPadding, Calculator.ChunkDefinitionBitSizeWithPadding);

    private static int BinaryStringToInt(string binary) => Convert.ToInt32(binary, 2);

    private short ReadChunkCount()
    {
        string headerBinary = stream.ReadContentChunkFromImage(Calculator.ChunkTableHeaderSizeWithPadding);
        log.Debug("Chunk table header: [{0}]", headerBinary);
        return Convert.ToInt16(headerBinary, 2);
    }

    private ImmutableArray<int> ReadTableChunkLengths(short chunkCount)
    {
        int chunkTableSize = Calculator.ChunkDefinitionBitSizeWithPadding * chunkCount;
        string tableBinary = stream.ReadContentChunkFromImage(chunkTableSize);
        log.Debug("Chunk table content: [{0}]", tableBinary);

        return Enumerable.Range(0, chunkCount)
            .Select(i => NextBinaryChunk(i, tableBinary))
            .Select(BinaryStringToInt)
            .ToImmutableArray();
    }
}