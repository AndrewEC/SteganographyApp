namespace SteganographyApp.Common.IO.Pixels;

using System;

using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Utility class to assist in the process of reading a specified number of bits
/// from each colour channel within a given input pixel.
/// </summary>
/// <remarks>
/// Initializes a new pixel reader instance.
/// </remarks>
/// <param name="bitAggregator">The aggregator which the bits pulled from each pixel will be added to.</param>
/// <param name="readableBitsPerPixel">Specifies the number of bits to reach from each pixel colour.</param>
internal sealed class PixelReader(BinaryStringBuilder bitAggregator, int readableBitsPerPixel)
{
    private readonly BinaryStringBuilder bitAggregator = bitAggregator;
    private readonly int readableBitsPerPixel = readableBitsPerPixel;

    /// <summary>
    /// Reads the appropriate number of bits from the input pixel and writes them to the BitAggregator
    /// providing during initialization of this reader.
    /// </summary>
    /// <param name="source">The input pixel to read the binary from.</param>
    public void ReadBinaryFromPixel(Rgba32 source)
    {
        bitAggregator.Put(ReadBits(source.R));
        bitAggregator.Put(ReadBits(source.G));
        bitAggregator.Put(ReadBits(source.B));
    }

    private string ReadBits(byte sourceColour)
    {
        var binary = Convert.ToString(sourceColour, 2).PadLeft(readableBitsPerPixel, '0');
        return binary.Substring(binary.Length - readableBitsPerPixel);
    }
}