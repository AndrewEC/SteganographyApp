using System.Linq;
using System.Collections.Generic;

using SteganographyApp.Common.IO;

namespace SteganographyApp
{

    public class TableChunkTracker
    {

        private readonly LinkedList<int> contentChunks = new LinkedList<int>();

        private LinkedListNode<int> lastNode = null;

        public int[] ContentTable
        {
            get
            {
                return contentChunks.ToArray();
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