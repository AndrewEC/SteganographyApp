namespace SteganographyApp.Common.IO;

using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// Functional interface specifying a single method to read a set of bits from a
/// queue and use said bits to identify a new colour value.
/// </summary>
internal interface IColourChannelUpdateStrategy
{
    /// <summary>
    /// Reads a set of bits from the queue and overwriter the lower bits of the source colour
    /// with them to determine the new bit colour.
    /// </summary>
    /// <param name="sourceColour">The byte representation of a pixel colour channel.</param>
    /// <param name="bitQueue">The bit queue from which the bits to write will be pulled from.</param>
    /// <returns>The newly formed colour of the input source colour.</returns>
    byte GetNewPixelColour(byte sourceColour, ReadBitQueue bitQueue);
}

/// <summary>
/// Static utility class to assist in performing some bitwise operations.
/// </summary>
internal static class Bitwise
{
    /// <summary>
    /// Swaps the least significant bit in the input value byte with the input binary character.
    /// </summary>
    /// <param name="value">The input byte whose least significant bit will be swapped with the input bit.</param>
    /// <param name="lastBit">The character specifiying a bit like value, 0 or 1, to swap in.</param>
    /// <returns>The newly formed byte.</returns>
    internal static byte SwapLeastSigificantBit(byte value, char lastBit) => Shift(value, 1, lastBit);

    /// <summary>
    /// Swaps the second least significant bit in the input value byte with the input binary character.
    /// </summary>
    /// <param name="value">The input byte whose second least significant bit will be swapped with the input bit.</param>
    /// <param name="lastBit">The character specifying a bit like value, 0 or 1, to swap in.</param>
    /// <returns>The newly formed byte.</returns>
    internal static byte SwapSecondLeastSignificantBit(byte value, char lastBit) => Shift(value, 2, lastBit);

    private static byte Shift(byte value, byte shift, char lastBit) => (lastBit == '0')
        ? (byte)(value & ~shift)
        : (byte)(value | shift);
}

/// <summary>
/// A colour channel update strategy intended to update only the least significant bit of a give input byte.
/// </summary>
internal sealed class SingleBitColourChannelUpdateStrategy : IColourChannelUpdateStrategy
{
    /// <summary>
    /// Replaces the least significant bit of the source colour byte with the value pulled from the bit queue. If
    /// there is no value remaining in the bit queue this will return the original result.
    /// </summary>
    /// <param name="sourceColour">A byte representing a single colour channel.</param>
    /// <param name="bitQueue">The bit queue from which the bits to write will be pulled from.</param>
    /// <returns>The newly formed colour of the input source colour.</returns>
    public byte GetNewPixelColour(byte sourceColour, ReadBitQueue bitQueue)
    {
        if (!bitQueue.HasNext())
        {
            return sourceColour;
        }
        return Bitwise.SwapLeastSigificantBit(sourceColour, bitQueue.Next());
    }
}

/// <summary>
/// A colour channel update strategy intended to update the least significant and second-least significant bit of a give input byte.
/// </summary>
internal sealed class TwoBitColourChannelUpdateStrategy : IColourChannelUpdateStrategy
{
    /// <summary>
    /// Replaces the least and second least significant bit of the input source colour byte with the bits pulled from the bit queue.
    /// If the bit queue is empty then this will return the original byte.
    /// </summary>
    /// <param name="sourceColour">A byte representing a single colour channel.</param>
    /// <param name="bitQueue">The bit queue from which the bits to write will be pulled from.</param>
    /// <returns>The newly formed byte with the two least significant bits replaced with the values pulled from the bitQueue.</returns>
    public byte GetNewPixelColour(byte sourceColour, ReadBitQueue bitQueue)
    {
        if (!bitQueue.HasNext())
        {
            return sourceColour;
        }
        byte destinationColour = Bitwise.SwapSecondLeastSignificantBit(sourceColour, bitQueue.Next('0'));
        destinationColour = Bitwise.SwapLeastSigificantBit(destinationColour, bitQueue.Next('0'));
        return destinationColour;
    }
}

/// <summary>
/// Utility class to take data from a bit queue and use it to update the colour of a series of pixels
/// represented by the Rgba32 struct.
/// </summary>
/// <remarks>
/// Constructs an instance of the pixel writer.
/// </remarks>
/// <param name="bitQueue">The queue to read the bits from.</param>
/// <param name="writableBitsPerPixel">The number of bits to read, starting from the LSB, from a given input pixels
/// RGB colours.</param>
internal sealed class PixelWriter(ReadBitQueue bitQueue, int writableBitsPerPixel)
{
    private readonly ReadBitQueue bitQueue = bitQueue;
    private readonly IColourChannelUpdateStrategy strategy = (writableBitsPerPixel == 2)
        ? new TwoBitColourChannelUpdateStrategy()
        : new SingleBitColourChannelUpdateStrategy();

    /// <summary>
    /// Takes a source pixel and creates a new pixel in which the new pixel will have the least or least and second least
    /// bits updated depending on the number of pixels to update specified during the PixelWriter's initialization.
    /// The bits used to update each of the RGB values will be pulled from the bigQueue provided during initialization.
    /// </summary>
    /// <param name="source">The input pixel.</param>
    /// <returns>A new pixel with each RGB value being an RGB valued pulled from the source pixel that
    /// has been subsequently updated by pulling a bit from the ReadBitQueue instance provided during initialization.</returns>
    public Rgba32 UpdatePixel(Rgba32 source) => new()
    {
        R = strategy.GetNewPixelColour(source.R, bitQueue),
        G = strategy.GetNewPixelColour(source.G, bitQueue),
        B = strategy.GetNewPixelColour(source.B, bitQueue),
        A = source.A,
    };
}