namespace SteganographyApp.Decode
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Handles taking in the raw binary data read from an image, decoding it,
    /// and writing it to the target location.
    /// </summary>
    internal class FileWriteThread
    {
        private readonly BlockingCollection<WriteArgs> queue;
        private readonly ErrorContainer errorContainer;
        private readonly IInputArguments arguments;
        private readonly ILogger log;
        private readonly Thread readThread;

        private FileWriteThread(BlockingCollection<WriteArgs> queue, ErrorContainer errorContainer, IInputArguments arguments)
        {
            this.arguments = arguments;
            this.queue = queue;
            this.errorContainer = errorContainer;
            log = Injector.LoggerFor<FileWriteThread>();
            readThread = new Thread(new ThreadStart(Write));
        }

        public static FileWriteThread CreateAndStartThread(BlockingCollection<WriteArgs> queue, ErrorContainer errorContainer, IInputArguments arguments)
        {
            var thread = new FileWriteThread(queue, errorContainer, arguments);
            thread.StartWriting();
            return thread;
        }

        public void Join()
        {
            Suppressed.TryRun(() => readThread.Join(1000));
        }

        private void StartWriting()
        {
            log.Trace("Starting file write thread.");
            readThread.Start();
        }

        private void Write()
        {
            try
            {
                using (var writer = new ContentWriter(arguments))
                {
                    while (true)
                    {
                        var writeArgs = queue.Take(errorContainer.CancellationToken);
                        if (writeArgs.Status == Status.Complete)
                        {
                            log.Trace("Write thread completed.");
                            break;
                        }
                        else
                        {
                            log.Debug("Writing next binary chunk of [{0}] bits.", writeArgs.Data!.Length);
                            writer.WriteContentChunkToFile(writeArgs.Data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error ocurred in file write thread: [{0}]", e.Message);
                errorContainer.PutException(e);
            }
        }
    }
}