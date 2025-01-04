namespace SteganographyApp.Common.IO.Pixels;

using SixLabors.ImageSharp.PixelFormats;

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