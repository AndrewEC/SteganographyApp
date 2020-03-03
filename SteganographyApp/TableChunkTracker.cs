using System;
using System.IO;
using System.Collections.Generic;

using SteganographyApp.Common.IO;

namespace SteganographyApp
{

    public class TableChunkTracker
    {

        private readonly LinkedList<int> contentChunks = new LinkedList<int>();

        private LinkedListNode<int> lastNode = null;

        public LinkedList<int> ContentTable
        {
            get
            {
                return contentChunks;
            }
        }

        public TableChunkTracker(ImageStore store)
        {
            store.OnChunkWritten += ChunkWritten;
        }

        private void ChunkWritten(object sender, ChunkWrittenArgs args)
        {
            LinkedListNode<int> node = new LinkedListNode<int>(args.ChunkLength);
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

}