namespace SteganographyApp.Common.IO;

using System;

/// <summary>
/// Event arguments passed into the OnNextImageLoaded event handler whenever the next image
/// has been loaded in the read, write, or clean process.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="imageName">The name of the image file that was loaded into memory.</param>
/// <param name="imageIndex">The index of the cover image that was just loaded.</param>
public readonly struct NextImageLoadedEventArgs(string imageName, int imageIndex)
{
    /// <summary>
    /// Gets the name of the file that was loaded.
    /// </summary>
    public readonly string ImageName { get; } = imageName;

    /// <summary>
    /// Gets the index of the image that was loaded. This represents the index of the image within the
    /// cover images parsed from the user's input.
    /// </summary>
    public readonly int ImageIndex { get; } = imageIndex;
}

/// <summary>
/// Event arguments passed into the OnChunkWritten event handler whenever an encoded binary
/// content has been writtent to an image.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="chunkLength">The number of bits written to the cover image.</param>
public readonly struct ChunkWrittenArgs(int chunkLength)
{
    /// <summary>
    /// Gets the length, in bytes, of the number of bits that were just written to the cover images.
    /// </summary>
    public readonly int ChunkLength { get; } = chunkLength;
}

/// <summary>
/// A general exception to represent a specific error occured
/// while reading or writing data to the images.
/// </summary>
public sealed class ImageProcessingException : Exception
{
    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
    public ImageProcessingException(string message) : base(message) { }

    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
    public ImageProcessingException(string message, Exception inner) : base(message, inner) { }
}