namespace SteganographyApp.Common.IO.Pixels;

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
