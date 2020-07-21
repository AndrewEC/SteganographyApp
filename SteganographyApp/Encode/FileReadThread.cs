using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO.Content;

using System;
using System.Threading;
using System.Collections.Concurrent;

namespace SteganographyApp.Encode
{

    /// <summary>
    /// Handles reading in and encoding data from a source file and adding it
    /// to the queue so it can be consumed by the encoder and written to the
    /// destination images.
    /// </summary>
    class FileReadThread
    {

        private readonly BlockingCollection<ReadArgs> queue;
        private readonly IInputArguments arguments;
        private readonly ErrorContainer errorContainer;
        private Thread readThread;

        private FileReadThread(BlockingCollection<ReadArgs> queue, IInputArguments arguments, ErrorContainer errorContainer)
        {
            this.queue = queue;
            this.arguments = arguments;
            this.errorContainer = errorContainer;
        }

        public static FileReadThread CreateAndStart(BlockingCollection<ReadArgs> queue, IInputArguments arguments, ErrorContainer errorContainer)
        {
            var thread = new FileReadThread(queue, arguments, errorContainer);
            thread.StartReading();
            return thread;
        }

        public void StartReading()
        {
            readThread = new Thread(new ThreadStart(Read));
            readThread.Start();
        }

        /// <summary>
        /// Starts reading data from the source file, encoding it, then adding it to the
        /// message queue.
        /// If an error occurs while reading/encoding the source file then the exception will
        /// be added to the errorQueue and the read loop will end.
        /// </summary>
        private void Read()
        {
            using (var reader = new ContentReader(arguments))
            {
                try
                {
                    string contentChunk = "";
                    while((contentChunk = reader.ReadContentChunkFromFile()) != null)
                    {
                        if (errorContainer.HasException())
                        {
                            return;
                        }
                        queue.Add(new ReadArgs { Status = Status.Incomplete, Data = contentChunk });
                    }
                    queue.Add(new ReadArgs { Status = Status.Complete });
                }
                catch (Exception e)
                {
                    queue.Add(new ReadArgs { Status = Status.Failure, Exception = e });
                    return;
                }
            }
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