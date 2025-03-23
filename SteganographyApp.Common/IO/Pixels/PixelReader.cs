namespace SteganographyApp.Common.IO.Pixels;

using System;

using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Handles reading in N, <paramref name="readableBitsPerPixel"/>, bits per colour channel
/// per pixel and storing them in a binary string.
/// </summary>
/// <param name="bitAggregator">The aggregator which the bits pulled from each pixel
/// will be added to.</param>
/// <param name="readableBitsPerPixel">Specifies the number of bits to read from
/// each pixel colour.</param>
internal sealed class PixelReader(BinaryStringBuilder bitAggregator, int readableBitsPerPixel)
{
    private readonly BinaryStringBuilder bitAggregator = bitAggregator;
    private readonly int readableBitsPerPixel = readableBitsPerPixel;

    /// <summary>
    /// Reads the appropriate number of bits from the input pixel's RGB channels and writes
    /// them to the <see cref="BinaryStringBuilder"/>.
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
        var binary = Convert.ToString(sourceColour, 2).PadLeft(8, '0');
        return binary.Substring(binary.Length - readableBitsPerPixel);
    }
}