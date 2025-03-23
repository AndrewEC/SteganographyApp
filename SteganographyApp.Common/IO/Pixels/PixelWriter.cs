namespace SteganographyApp.Common.IO.Pixels;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Handles reading bit data from the input queue and using said bits to update the
/// least, or second least, significant bits of the RGB channels of each input pixel.
/// </summary>
/// <param name="bitQueue">The queue containing the bits used to modify each pixel's
/// RGB values.</param>
/// <param name="writableBitsPerPixel">The number of bits to write to each colour
/// channel in each pixel.</param>
internal sealed class PixelWriter(ReadBitQueue bitQueue, int writableBitsPerPixel)
{
    private readonly ReadBitQueue bitQueue = bitQueue;

    private static ImmutableDictionary<int, Func<byte, ReadBitQueue, byte>> UpdateFunctions
    = new Dictionary<int, Func<byte, ReadBitQueue, byte>>()
    {
        { 1, UpdateSingleBit },
        { 2, UpdateTwoBits },
    }.ToImmutableDictionary();

    private readonly Func<byte, ReadBitQueue, byte> updateFunction
        = UpdateFunctions[writableBitsPerPixel];

    /// <summary>
    /// Takes in a source pixel and returns a net new pixel with the RGB values
    /// updated with data pulled from the <see cref="ReadBitQueue"/>. This will
    /// make no changes to the alpha channel. Instead the alpha will be copied to the
    /// new pixel struct as is.
    /// </summary>
    /// <param name="source">The input pixel.</param>
    /// <returns>A new pixel with the updated RGB values.</returns>
    public Rgba32 UpdatePixel(Rgba32 source) => new()
    {
        R = updateFunction.Invoke(source.R, bitQueue),
        G = updateFunction.Invoke(source.G, bitQueue),
        B = updateFunction.Invoke(source.B, bitQueue),
        A = source.A,
    };

    private static byte UpdateSingleBit(byte sourceColour, ReadBitQueue bitQueue)
    {
        return Bitwise.SwapLeastSigificantBit(sourceColour, bitQueue.Next('0'));
    }

    private static byte UpdateTwoBits(byte sourceColour, ReadBitQueue bitQueue)
    {
        byte destinationColour = Bitwise.SwapSecondLeastSignificantBit(sourceColour, bitQueue.Next('0'));
        destinationColour = Bitwise.SwapLeastSigificantBit(destinationColour, bitQueue.Next('0'));
        return destinationColour;
    }
}