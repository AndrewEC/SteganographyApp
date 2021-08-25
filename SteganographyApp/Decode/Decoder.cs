namespace SteganographyApp.Decode
{
    using System;
    using System.Collections.Concurrent;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Used to indicate if there is more data to be read from the storage images.
    /// Incomplete means there is more data to be read, complete means there is none.
    /// </summary>
    internal enum Status
    {
        Incomplete,
        Complete,
    }

    /// <summary>
    /// Specifies the values to be added to the queue whenever data is read
    /// from the file to encode.
    /// </summary>
    internal struct WriteArgs
    {
        public Status Status;
        public string Data;
    }

    public class Decoder
    {
        private readonly IInputArguments arguments;

        /// <summary>
        /// This queue allows for communication between the decoder and the <see cref="FileWriteThread"/>.
        /// Once each content chunk is read from the ImageStore it will be
        /// added to the this collection so the raw binary content from the image can
        /// be decoded and written to the decoded output file location.
        /// </summary>
        private readonly BlockingCollection<WriteArgs> writeQueue;

        private readonly ErrorContainer errorContainer;

        private readonly ILogger log;

        private Decoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            writeQueue = new BlockingCollection<WriteArgs>(2);
            errorContainer = new ErrorContainer();
            log = Injector.LoggerFor<Decoder>();
        }

        /// <summary>
        /// Creates a decode instance and invokes the
        /// <see cref="DecodeFileFromImage" /> method.
        /// </summary>
        public static void CreateAndDecode(IInputArguments arguments) => new Decoder(arguments).DecodeFileFromImage();

        private void DecodeFileFromImage()
        {
            Console.WriteLine("Decoding to File: {0}", arguments.DecodedOutputFile);
            log.Debug("Decoding to file: [{0}]", arguments.DecodedOutputFile);

            var store = new ImageStore(arguments);

            using (var wrapper = store.CreateIOWrapper())
            {
                var thread = FileWriteThread.CreateAndStartThread(writeQueue, errorContainer, arguments);
                try
                {
                    DoDecode(store, wrapper);
                }
                catch (Exception e)
                {
                    errorContainer.PutException(e);
                    log.Error("Decoder found exception in container: [{0}]", e.Message);
                    throw;
                }
                finally
                {
                    Suppressed.TryRun(() => thread.Join());
                }
            }

            log.Trace("Decoding process completed.");
            Console.WriteLine("Decoding process complete.");
        }

        private void DoDecode(ImageStore store, ImageStoreIO wrapper)
        {
            Console.WriteLine("Reading content chunk table.");
            var contentChunkTable = store.ReadContentChunkTable();
            var tracker = ProgressTracker.CreateAndDisplay(contentChunkTable.Length, "Decoding file contents", "All input file contents have been decoded, completing last write to output file.");

            log.Debug("Content chunk table contains [{0}] entries.", contentChunkTable.Length);
            foreach (int chunkLength in contentChunkTable)
            {
                log.Debug("Processing chunk of [{0}] bits.", chunkLength);
                string binary = wrapper.ReadContentChunkFromImage(chunkLength);
                writeQueue.Add(new WriteArgs { Data = binary, Status = Status.Incomplete });
                tracker.UpdateAndDisplayProgress();

                if (errorContainer.HasException())
                {
                    throw errorContainer.TakeException();
                }
            }

            writeQueue.Add(new WriteArgs { Status = Status.Complete });

            if (errorContainer.HasException())
            {
                throw errorContainer.TakeException();
            }
        }
    }
}