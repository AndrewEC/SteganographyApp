namespace SteganographyApp.Common.IO;

/// <summary>
/// Event arguments passed into the OnNextImageLoaded event handler whenever the next image
/// has been loaded in the read, or write process.
/// </summary>
/// <param name="Path">The path of the image file that was loaded into memory.</param>
/// <param name="Index">The index of the cover image that was just loaded.</param>
public readonly record struct NextImageLoadedEventArgs(string Path, int Index);
