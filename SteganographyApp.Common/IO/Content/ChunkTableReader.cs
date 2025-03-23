namespace SteganographyApp.Common.IO.Content;

using System;
using System.Collections.Immutable;
using System.Linq;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

/// <summary>
/// Responsible for reading the content chunk table from the leading cover image.
/// </summary>
public static class ChunkTableReader
{
    /// <summary>
    /// Reads in and returns an array in which each element represents the number
    /// of bits in a chunk. This will advance the <see cref="ImageStoreStream"/>
    /// by the number of pixels required to read in the full content chunk table.
    /// </summary>
    /// <param name="stream">The <see cref="IImageStoreStream"/> open in read mode to the images
    /// from which the content chunk table will be read. The stream passed should not have been
    /// read from by this point. It should be pointing to the first pixel of the first image
    /// as this is the location where the content chunk table starts.</param>
    /// <returns>An immutable array in whcih each element specifies the number of bits per
    /// chunk saved in the cover images.</returns>
    public static ImmutableArray<int> ReadContentChunkTable(IImageStoreStream stream)
    {
        var log = ServiceContainer.GetLogger(typeof(ChunkTableReader));
        log.Trace("Reading content chunk table.");

        short chunkCount = ReadChunkCount(stream, log);
        log.Debug("Chunk table contains [{0}] chunks.", chunkCount);

        return ReadTableChunkLengths(stream, log, chunkCount);
    }

    private static string NextBinaryChunk(int index, string binaryString)
        => binaryString.Substring(index * Calculator.ChunkDefinitionBitSizeWithPadding, Calculator.ChunkDefinitionBitSizeWithPadding);

    private static int BinaryStringToInt(string binary) => Convert.ToInt32(binary, 2);

    private static short ReadChunkCount(IImageStoreStream stream, ILogger log)
    {
        string headerBinary = stream.ReadContentChunkFromImage(Calculator.ChunkTableHeaderSizeWithPadding);
        log.Debug("Chunk table header: [{0}]", headerBinary);
        return Convert.ToInt16(headerBinary, 2);
    }

    private static ImmutableArray<int> ReadTableChunkLengths(IImageStoreStream stream, ILogger log, short chunkCount)
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