namespace SteganographyApp.Encode
{
    using System;
    using System.Collections.Concurrent;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Used to indicate if there is more data that can be potentially read
    /// from the file to encode. Incomplete indicates there might be more data
    /// to read, complete means there is for sure none left.
    /// </summary>
    internal enum Status
    {
        Incomplete,
        Complete,
        Failure,
    }

    /// <summary>
    /// Specifies the values to be added to our read queue by the file read
    /// thread.
    /// </summary>
    internal readonly struct ReadArgs
    {
        public readonly Status Status;
        public readonly string? Data;
        public readonly Exception? Exception;

        public ReadArgs(Status status, string? data = null, Exception? exception = null)
        {
            Status = status;
            Data = data;
            Exception = exception;
        }
    }

    public class Encoder
    {
        private readonly IInputArguments arguments;

        private readonly ILogger log;

        private Encoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            log = Injector.LoggerFor<Encoder>();
        }

        /// <summary>
        /// Creates an Encoder instances and invokes the private
        /// <see cref="EncodeFileToImage"/> method.
        /// </summary>
        public static void CreateAndEncode(IInputArguments arguments) => new Encoder(arguments).EncodeFileToImage();

        /// <summary>
        /// Initiates the process of encoding a file and storing it within an image.
        /// The main read/encode/write loop consists of the file read thread reading and
        /// encoding a portion of the file to encode and adding it to the read queue.
        /// The item on the read queue will then be picked up by the main thread and written
        /// to the target storage images.
        /// </summary>
        private void EncodeFileToImage()
        {
            Console.WriteLine("Encoding File: {0}", arguments.FileToEncode);

            var utilities = new EncodingUtilities(arguments);

            Encode(utilities);

            Cleanup(utilities);
        }

        private void Encode(EncodingUtilities utilities)
        {
            using (var wrapper = utilities.ImageStore.CreateIOWrapper())
            {
                log.Debug("Encoding file: [{0}]", arguments.FileToEncode);
                int startingPixel = Calculator.CalculateRequiredBitsForContentTable(arguments.FileToEncode, arguments.ChunkByteSize);
                log.Debug("Content chunk table requires [{0}] bits of space to store.", startingPixel);
                wrapper.SeekToPixel(startingPixel);

                int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);
                log.Debug("File requires [{0}] iterations to encode.", requiredNumberOfWrites);
                var progressTracker = ProgressTracker.CreateAndDisplay(requiredNumberOfWrites, "Encoding file contents", "All input file contents have been encoded.");

                using (var reader = new ContentReader(arguments))
                {
                    string? contentChunk = string.Empty;
                    while (true)
                    {
                        log.Debug("===== ===== ===== Begin Encoding Iteration ===== ===== =====");
                        contentChunk = reader.ReadContentChunkFromFile();
                        if (contentChunk == null)
                        {
                            break;
                        }
                        log.Debug("Processing chunk of [{0}] bits.", contentChunk.Length);
                        wrapper.WriteContentChunkToImage(contentChunk);
                        progressTracker.UpdateAndDisplayProgress();
                        log.Debug("===== ===== ===== End Encoding Iteration ===== ===== =====");
                    }
                }
                wrapper.EncodeComplete();
            }
        }

        private void Cleanup(EncodingUtilities utilities)
        {
            Console.WriteLine("Writing content chunk table.");
            using (var writer = new ChunkTableWriter(utilities.ImageStore, arguments))
            {
                writer.WriteContentChunkTable(utilities.TableTracker.GetContentTable());
            }
            Console.WriteLine("Encoding process complete.");
            log.Trace("Encoding process complete.");
            utilities.ImageTracker.PrintImagesUtilized();
        }
    }

    /// <summary>
    /// Helps initialize the utilitiy classes required to fulfill the encoding
    /// process.
    /// </summary>
    internal class EncodingUtilities
    {
        public EncodingUtilities(IInputArguments args)
        {
            ImageStore = new ImageStore(args);
            TableTracker = new TableChunkTracker(ImageStore);
            ImageTracker = ImageTracker.Create(args, ImageStore);
        }

        public ImageStore ImageStore { get; }

        public TableChunkTracker TableTracker { get; }

        public ImageTracker ImageTracker { get; }
    }
}