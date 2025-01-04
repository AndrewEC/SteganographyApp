namespace SteganographyApp.Common.IO.Pixels;

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
