namespace SteganographyApp.Common.IO;

/// <summary>
/// Event arguments passed into the OnNextImageLoaded event handler whenever the next image
/// has been loaded in the read, or write process.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="path">The path of the image file that was loaded into memory.</param>
/// <param name="index">The index of the cover image that was just loaded.</param>
public readonly struct NextImageLoadedEventArgs(string path, int index)
{
    /// <summary>
    /// Gets the path of the file that was loaded.
    /// </summary>
    public readonly string Path { get; } = path;

    /// <summary>
    /// Gets the index of the image that was loaded. This represents the index of the image within the
    /// cover images parsed from the user's input.
    /// </summary>
    public readonly int Index { get; } = index;
}
