namespace SteganographyApp.Common.IO;

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