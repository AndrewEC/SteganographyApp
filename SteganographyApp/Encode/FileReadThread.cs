using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO.Content;

using System.Threading;
using System.Collections.Concurrent;

namespace SteganographyApp.Encode
{

    class FileReadThread
    {

        private readonly BlockingCollection<ReadArgs> queue;
        private readonly IInputArguments arguments;
        private Thread readThread;

        private FileReadThread(BlockingCollection<ReadArgs> queue, IInputArguments arguments)
        {
            this.queue = queue;
            this.arguments = arguments;
        }

        public static FileReadThread CreateAndStart(BlockingCollection<ReadArgs> queue, IInputArguments arguments)
        {
            var thread = new FileReadThread(queue, arguments);
            thread.Read();
            return thread;
        }

        public void StartReading()
        {
            readThread = new Thread(new ThreadStart(Read));
            readThread.Start();
        }

        private void Read()
        {
            using (var reader = new ContentReader(arguments))
            {
                string contentChunk = "";
                while((contentChunk = reader.ReadContentChunkFromFile()) != null)
                {
                    queue.Add(new ReadArgs { Status = Status.Incomplete, Data = contentChunk });
                }
            }
            queue.Add(new ReadArgs { Status = Status.Complete });
        }

        public void Join()
        {
            try
            {
                readThread.Join();
            }
            catch
            {
            }
        }

    }

}