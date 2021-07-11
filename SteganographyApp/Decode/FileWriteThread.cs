namespace SteganographyApp.Decode
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;

    /// <summary>
    /// Handles taking in the raw binary data read from an image, decoding it,
    /// and writing it to the target location.
    /// </summary>
    internal class FileWriteThread
    {
        private readonly BlockingCollection<WriteArgs> queue;
        private readonly ErrorContainer decodeError;
        private readonly IInputArguments arguments;
        private readonly ILogger log;
        private Thread readThread;

        private FileWriteThread(BlockingCollection<WriteArgs> queue, ErrorContainer errorContainer, IInputArguments arguments)
        {
            this.arguments = arguments;
            this.queue = queue;
            this.decodeError = errorContainer;
            log = Injector.LoggerFor<FileWriteThread>();
        }

        public static FileWriteThread CreateAndStartThread(BlockingCollection<WriteArgs> queue, ErrorContainer errorContainer, IInputArguments arguments)
        {
            var thread = new FileWriteThread(queue, errorContainer, arguments);
            thread.StartWriting();
            return thread;
        }

        public void StartWriting()
        {
            log.Trace("Starting file write thread.");
            readThread = new Thread(new ThreadStart(Write));
            readThread.Start();
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

        /// <summary>
        /// Here the thread will wait to be able to take an item from the
        /// queue. The queue item will be the raw binary string read the from
        /// images storing the content. This thread will then take that content,
        /// decode it, and write it to the destination file.
        /// If an exception ocurrs while reading from the source file the exception will be added
        /// to the erro queue and the loop will exit.
        /// </summary>
        private void Write()
        {
            using (var writer = new ContentWriter(arguments))
            {
                while (true)
                {
                    var writeArgs = queue.Take();
                    if (writeArgs.Status == Status.Complete)
                    {
                        log.Trace("Write thread completed.");
                        break;
                    }
                    else
                    {
                        try
                        {
                            log.Debug("Writing next binary chunk of [{0}] bits.", writeArgs.Data.Length);
                            writer.WriteContentChunkToFile(writeArgs.Data);
                        }
                        catch (Exception e)
                        {
                            log.Error("An error ocurred while writing content chunk to file: [{0}]", e.Message);
                            decodeError.PutException(e);
                            break;
                        }
                    }
                }
            }
        }
    }
}