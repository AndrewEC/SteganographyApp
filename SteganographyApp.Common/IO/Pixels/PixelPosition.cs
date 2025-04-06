namespace SteganographyApp.Common.IO.Pixels;

using System.Globalization;
using System.Text;
using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Tracks the position of the current pixel to be read from, written to,
/// in the currently loaded image.
/// </summary>
internal sealed class PixelPosition
{
    private static readonly CompositeFormat ToStringFormat = CompositeFormat.Parse("(X: {0}, Y: {1})");

    /// <summary>
    /// Gets the current pixel X position.
    /// </summary>
    public int X { get; private set; }

    /// <summary>
    /// Gets the current pixel Y position.
    /// </summary>
    public int Y { get; private set; }

    /// <summary>
    /// Gets or sets the image currently being tracked and updated.
    /// </summary>
    private IBasicImageInfo? TrackedImage { get; set; }

    /// <summary>
    /// Sets the image whose pixel position is currently being tracked and starts
    /// tracking from the first pixel position (0, 0).
    /// </summary>
    /// <param name="imageInfo">The image to track.</param>
    public void TrackImage(IBasicImageInfo imageInfo)
    {
        TrackedImage = imageInfo;
        Reset();
    }

    /// <summary>
    /// Attempts to move to the next available pixel in the currently
    /// tracked image. If no more pixels are available this will return false.
    /// </summary>
    /// <returns>False if there is no further pixel to move to, otherwise true.</returns>
    public bool TryMoveToNext()
    {
        X++;
        if (X == TrackedImage!.Width)
        {
            X = 0;
            Y++;
            if (Y == TrackedImage.Height)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Stringifies the pixel position in the format (X: {0}, Y: {1}).
    /// </summary>
    /// <returns>The position of the pixel in the format (X: {0}, Y: {1}).</returns>
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, ToStringFormat, X, Y);

    private void Reset()
    {
        X = 0;
        Y = 0;
    }
}