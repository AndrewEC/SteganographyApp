namespace SteganographyApp.Common.IO.Content;

using System;
using System.Collections.Immutable;
using System.Text;

using SteganographyApp.Common.Logging;

/// <summary>
/// Responsible for writing the content chunk table to the cover images.
/// </summary>
/// <param name="store">The image store instance.</param>
public sealed class ChunkTableWriter(IImageStore store) : AbstractDisposable
{
    private readonly LazyLogger<ChunkTableWriter> log = new();

    private readonly IImageStoreStream stream = store.OpenStream(StreamMode.Write);

    /// <summary>
    /// Writes the content chunk table to the cover images. This will start from the
    /// first pixel in the first image and will overwrite any data that might already
    /// be present. Hence, the first N bits of image space should be reserved for
    /// the content table. See <see cref="ICalculator.CalculateRequiredBitsForContentTable(int)"/>
    /// or <see cref="ICalculator.CalculateRequiredBitsForContentTable(string, int)"/>
    /// for calculating the required number of bits the table will need.
    /// </summary>
    /// <param name="chunkLengths">The list of chunks lengths, length referring to the number
    /// of bytes in each chunk, to be written to the cover images.</param>
    public void WriteContentChunkTable(ImmutableArray<int> chunkLengths) => RunIfNotDisposed(() =>
    {
        log.Debug("Saving content chunk table with [{0}] chunks", chunkLengths.Length);
        string tableHeader = To18BitBinaryString((short)chunkLengths.Length);
        log.Debug("Chunk table header: [{0}]", tableHeader);

        var binary = new StringBuilder();
        foreach (int chunkLength in chunkLengths)
        {
            binary.Append(To33BitBinaryString(chunkLength));
        }

        var binaryString = binary.ToString();
        log.Debug("Chunk table binary: [{0}]", binaryString);

        string tableBinary = tableHeader + binaryString;

        stream.WriteContentChunkToImage(tableBinary);
    });

    /// <summary>
    /// Disposes of the current instance. This will effectively call Dispose on the underlying
    /// stream opened from the input <see cref="ImageStore"/> instance.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() => stream.Dispose());

    private static string To33BitBinaryString(int value)
        => Convert.ToString(value, 2).PadLeft(Calculator.ChunkDefinitionBitSizeWithPadding, '0');

    private static string To18BitBinaryString(short value)
        => Convert.ToString(value, 2).PadLeft(Calculator.ChunkTableHeaderSizeWithPadding, '0');
}