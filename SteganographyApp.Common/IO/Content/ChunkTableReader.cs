namespace SteganographyApp.Common.IO;

using System;
using System.Collections.Immutable;
using System.Linq;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

/// <summary>
/// Responsible for reading the content chunk table from the leading cover image.
/// </summary>
/// <param name="stream">The stream opened from the image store.</param>
/// <param name="arguments">The user provided arguments.</param>
public class ChunkTableReader(ImageStoreStream stream, IInputArguments arguments)
{
    private readonly ILogger log = new LazyLogger<ChunkTableReader>();

    private readonly ImageStoreStream stream = stream;

    private readonly IInputArguments arguments = arguments;

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
        var randomizeUtil = Injector.Provide<IRandomizeUtil>();
        var binaryUtil = Injector.Provide<IBinaryUtil>();

        short chunkCount = ReadChunkCount(randomizeUtil, binaryUtil);
        log.Debug("Chunk table contains [{0}] chunks.", chunkCount);

        return ReadTableChunkLengths(randomizeUtil, binaryUtil, chunkCount);
    }

    private static string NextBinaryChunk(int index, string binaryString)
        => binaryString.Substring(index * Calculator.ChunkDefinitionBitSizeWithPadding, Calculator.ChunkDefinitionBitSizeWithPadding);

    private static int BinaryStringToInt(string binary) => Convert.ToInt32(binary, 2);

    private short ReadChunkCount(IRandomizeUtil randomizeUtil, IBinaryUtil binaryUtil)
    {
        string headerBinary = stream.ReadContentChunkFromImage(Calculator.ChunkTableHeaderSizeWithPadding);
        log.Debug("Chunk table header: [{0}]", headerBinary);
        if (!string.IsNullOrEmpty(arguments.RandomSeed))
        {
            headerBinary = Reorder(randomizeUtil, binaryUtil, headerBinary);
            log.Debug("Reordered chunk table header: [{0}]", headerBinary);
        }
        return Convert.ToInt16(headerBinary, 2);
    }

    private ImmutableArray<int> ReadTableChunkLengths(IRandomizeUtil randomizeUtil, IBinaryUtil binaryUtil, short chunkCount)
    {
        int chunkTableSize = Calculator.ChunkDefinitionBitSizeWithPadding * chunkCount;
        string tableBinary = stream.ReadContentChunkFromImage(chunkTableSize);
        log.Debug("Chunk table content: [{0}]", tableBinary);
        if (!string.IsNullOrEmpty(arguments.RandomSeed))
        {
            tableBinary = Reorder(randomizeUtil, binaryUtil, tableBinary);
            log.Debug("Reordered chunk table content: [{0}]", tableBinary);
        }

        return Enumerable.Range(0, chunkCount)
            .Select(i => NextBinaryChunk(i, tableBinary))
            .Select(BinaryStringToInt)
            .ToImmutableArray();
    }

    private string Reorder(IRandomizeUtil randomize, IBinaryUtil binaryUtil, string binary)
    {
        byte[] binaryBytes = binaryUtil.ToBytesDirect(binary);
        byte[] ordered = randomize.Reorder(binaryBytes, arguments.RandomSeed, ChunkTableConstants.DummyCount, ChunkTableConstants.IterationMultiplier);
        return binaryUtil.ToBinaryStringDirect(ordered);
    }
}