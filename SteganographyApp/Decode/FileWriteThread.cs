using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO.Content;

using System.Collections.Concurrent;
using System.Threading;

namespace SteganographyApp.Decode
{

    class FileWriteThread
    {

        private readonly BlockingCollection<WriteArgs> queue;
        private readonly IInputArguments arguments;

        public FileWriteThread(BlockingCollection<WriteArgs> queue, IInputArguments arguments)
        {
            this.arguments = arguments;
            this.queue = queue;
        }

        public void StartWriting()
        {
            new Thread(new ThreadStart(Write)).Start();
        }

        private void Write()
        {
            using (var writer = new ContentWriter(arguments))
            {
                while (true)
                {
                    var writeArgs = queue.Take();
                    if (writeArgs.Status == Status.Complete)
                    {
                        break;
                    }
                    else
                    {
                        writer.WriteContentChunkToFile(writeArgs.Data);
                    }
                }
            }
        }

    }

}