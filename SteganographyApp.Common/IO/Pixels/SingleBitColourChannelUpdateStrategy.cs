namespace SteganographyApp.Common.IO.Pixels;

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
