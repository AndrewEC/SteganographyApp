namespace SteganographyApp.Common.IO;

/// <summary>
/// Event arguments passed into the OnChunkWritten event handler whenever an encoded binary
/// content has been writtent to an image.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="ChunkLength">The number of bits written to the cover image.</param>
public readonly record struct ChunkWrittenArgs(int ChunkLength);