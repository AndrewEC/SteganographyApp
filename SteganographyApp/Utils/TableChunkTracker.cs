namespace SteganographyApp;

using System.Collections.Generic;
using System.Collections.Immutable;

using SteganographyApp.Common.IO;

/// <summary>
/// Hooks into the image store's chunk written event to
/// record the length of a binary chunk once it has been written to a
/// storage image so the entire content chunk table can be written
/// to the leading image at the end of the encoding process.
/// </summary>
public class TableChunkTracker
{
    private readonly LinkedList<int> contentChunks = new();

    private LinkedListNode<int>? lastNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableChunkTracker"/> class.
    /// </summary>
    /// <param name="store">The ImageStore instance to attach to the OnChunkWritten event of.</param>
    public TableChunkTracker(ImageStore store)
    {
        store.OnChunkWritten += ChunkWritten;
    }

    /// <summary>
    /// Returns the current list of content chunks as an array. The original list is a
    /// LinkedList meaning the order of this array matches the order in which each
    /// content chunk was written to the storage image.
    /// </summary>
    /// <returns>An immutable array with each element representing the length of a chunk.</returns>
    public ImmutableArray<int> GetContentTable() => contentChunks.ToImmutableArray();

    private void ChunkWritten(object? sender, ChunkWrittenArgs args)
    {
        var node = new LinkedListNode<int>(args.ChunkLength);
        if (lastNode == null)
        {
            contentChunks.AddFirst(node);
        }
        else
        {
            contentChunks.AddAfter(lastNode, node);
        }

        lastNode = node;
    }
}