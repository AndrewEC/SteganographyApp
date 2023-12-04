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
/// <param name="store">The image store instance.</param>
/// <param name="arguments">The user provided arguments.</param>
public class ChunkTableReader(ImageStore store, IInputArguments arguments) : AbstractChunkTableIO(store, arguments)
{
    private readonly ILogger log = new LazyLogger<ChunkTableReader>();

    /// <summary>
    /// Reads in and returns an array in which each element represents the number of bits in a chunk.
    /// </summary>
    /// <returns>An immutable array in whcih each element specifies the number of bits per chunk saved in the
    /// cover images.</returns>
    public ImmutableArray<int> ReadContentChunkTable() => RunIfNotDisposedWithResult(() =>
    {
        log.Trace("Reading content chunk table.");
        var randomizeUtil = Injector.Provide<IRandomizeUtil>();
        var binaryUtil = Injector.Provide<IBinaryUtil>();

        short chunkCount = ReadChunkCount(randomizeUtil, binaryUtil);
        log.Debug("Chunk table contains [{0}] chunks.", chunkCount);

        return ReadTableChunkLengths(randomizeUtil, binaryUtil, chunkCount);
    });

    private static string NextBinaryChunk(int index, string binaryString)
        => binaryString.Substring(index * Calculator.ChunkDefinitionBitSizeWithPadding, Calculator.ChunkDefinitionBitSizeWithPadding);

    private static int BinaryStringToInt(string binary) => Convert.ToInt32(binary, 2);

    private short ReadChunkCount(IRandomizeUtil randomizeUtil, IBinaryUtil binaryUtil)
    {
        string headerBinary = ImageStoreIO.ReadContentChunkFromImage(Calculator.ChunkTableHeaderSizeWithPadding);
        log.Debug("Chunk table header: [{0}]", headerBinary);
        if (!string.IsNullOrEmpty(Arguments.RandomSeed))
        {
            headerBinary = Reorder(randomizeUtil, binaryUtil, headerBinary);
            log.Debug("Reordered chunk table header: [{0}]", headerBinary);
        }
        return Convert.ToInt16(headerBinary, 2);
    }

    private ImmutableArray<int> ReadTableChunkLengths(IRandomizeUtil randomizeUtil, IBinaryUtil binaryUtil, short chunkCount)
    {
        int chunkSize = Calculator.ChunkDefinitionBitSizeWithPadding * chunkCount;
        string tableBinary = ImageStoreIO.ReadContentChunkFromImage(chunkSize);
        log.Debug("Chunk table content: [{0}]", tableBinary);
        if (!string.IsNullOrEmpty(Arguments.RandomSeed))
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
        byte[] ordered = randomize.Reorder(binaryBytes, Arguments.RandomSeed, DummyCount, IterationMultiplier);
        return binaryUtil.ToBinaryStringDirect(ordered);
    }
}