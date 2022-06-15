namespace SteganographyApp.Encode
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Handles reading in and encoding data from a source file and adding it
    /// to the queue so it can be consumed by the encoder and written to the
    /// destination images.
    /// </summary>
    internal class FileReadThread
    {
        private readonly BlockingCollection<ReadArgs> queue;
        private readonly IInputArguments arguments;
        private readonly ErrorContainer errorContainer;
        private readonly ILogger log;
        private readonly Thread readThread;

        private FileReadThread(BlockingCollection<ReadArgs> queue, IInputArguments arguments, ErrorContainer errorContainer)
        {
            this.queue = queue;
            this.arguments = arguments;
            this.errorContainer = errorContainer;
            log = Injector.LoggerFor<FileReadThread>();
            readThread = new Thread(new ThreadStart(Read));
        }

        public static FileReadThread CreateAndStart(BlockingCollection<ReadArgs> queue, IInputArguments arguments, ErrorContainer errorContainer)
        {
            var thread = new FileReadThread(queue, arguments, errorContainer);
            thread.StartReading();
            return thread;
        }

        public void StartReading()
        {
            log.Trace("Stating file read thread.");
            readThread.Start();
        }

        public void Join()
        {
            Suppressed.TryRun(() => readThread.Join(1000));
        }

        /// <summary>
        /// Starts reading data from the source file, encoding it, then adding it to the
        /// message queue.
        /// If an error occurs while reading/encoding the source file then the exception will
        /// be added to the errorQueue and the read loop will end.
        /// </summary>
        private void Read()
        {
            try
            {
                using (var reader = new ContentReader(arguments))
                {
                    string? contentChunk = string.Empty;
                    while ((contentChunk = reader.ReadContentChunkFromFile()) != null)
                    {
                        if (errorContainer.HasException())
                        {
                            return;
                        }
                        queue.Add(new ReadArgs(Status.Incomplete, contentChunk), errorContainer.CancellationToken);
                    }
                    queue.Add(new ReadArgs(Status.Complete), errorContainer.CancellationToken);
                }
            }
            catch (Exception e)
            {
                queue.Add(new ReadArgs(Status.Failure, exception: e), errorContainer.CancellationToken);
                return;
            }
        }
    }
}