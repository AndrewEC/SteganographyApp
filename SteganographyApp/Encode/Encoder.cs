using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;
using SteganographyApp.Common;

using System;
using System.Collections.Concurrent;

namespace SteganographyApp.Encode
{

    /// <summary>
    /// Used to indicate if there is more data that can be potentially read
    /// from the file to encode. Incomplete indicates there might be more data
    /// to read, complete means there is for sure none left.
    /// </summary>
    enum Status
    {
        Incomplete,
        Complete,
        Failure
    }

    /// <summary>
    /// Specifies the values to be added to our read queue by the file read
    /// thread.
    /// </summary>
    struct ReadArgs
    {
        public Status Status;
        public string Data;
        public Exception Exception;
    }

    /// <summary>
    /// Helps initialize the utilitiy classes required to fulfill the encoding
    /// process.
    /// </summary>
    class EncodingUtilities
    {

        public ImageStore ImageStore { get; }
        public TableChunkTracker TableTracker { get; }
        public ImageTracker ImageTracker { get; }

        public EncodingUtilities(IInputArguments args)
        {
            ImageStore = new ImageStore(args);
            TableTracker = new TableChunkTracker(ImageStore);
            ImageTracker = ImageTracker.CreateTrackerFrom(args, ImageStore);
        }

    }

    public class Encoder
    {

        private readonly IInputArguments arguments;

        /// <summary>
        /// Allows for communication between the file read thread and the main thread.
        /// In each iteration the read thread will read in a chunk from a file, encode it,
        /// and add it to this collection so it can be picked up and written to an image.
        /// <para>This also enables the read thread to send errors back to the main thread
        /// so a proper error message can be displayed back to the user.</para>
        /// </summary>
        private readonly BlockingCollection<ReadArgs> readQueue;

        /// <summary>
        /// Used to help communicate the ocurrence of an error to the file read thread so
        /// it can attempt to shutdown gracefully.
        /// </summary>
        private readonly ErrorContainer errorContainer;

        private Encoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            readQueue = new BlockingCollection<ReadArgs>(2);
            errorContainer = new ErrorContainer();
        }

        /// <summary>
        /// Creates an Encoder instances and invokes the private
        /// <see cref="EncodeFileToImage"/> method.
        /// </summary>
        public static void CreateAndEncode(IInputArguments arguments)
        {
            new Encoder(arguments).EncodeFileToImage();
        }

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

            using (var wrapper = utilities.ImageStore.CreateIOWrapper())
            {
                Encode(wrapper);
            }

            Cleanup(utilities);
        }

        private void Encode(ImageStore.ImageStoreWrapper wrapper)
        {
            
            int startingPixel = Calculator.CalculateRequiredBitsForContentTable(arguments.FileToEncode, arguments.ChunkByteSize);
            wrapper.SeekToPixel(startingPixel);

            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);
            var progressTracker = ProgressTracker.CreateAndDisplay(requiredNumberOfWrites,
                "Encoding file contents", "All input file contents have been encoded.");

            var thread = FileReadThread.CreateAndStart(readQueue, arguments, errorContainer);

            while(true)
            {
                var readArgs = readQueue.Take();
                if (readArgs.Status == Status.Complete)
                {
                    thread.Join();
                    wrapper.EncodeComplete();
                    break;
                }
                else if (readArgs.Status == Status.Failure)
                {
                    thread.Join();
                    throw readArgs.Exception;
                }
                else if (readArgs.Status == Status.Incomplete)
                {
                    try
                    {
                        wrapper.WriteContentChunkToImage(readArgs.Data);
                    }
                    catch (Exception e)
                    {
                        errorContainer.PutException(e);
                        thread.Join();
                        throw e;
                    }
                    progressTracker.UpdateAndDisplayProgress();
                }
            }
        }

        private void Cleanup(EncodingUtilities utilities)
        {
            Console.WriteLine("Writing content chunk table.");
            utilities.ImageStore.WriteContentChunkTable(utilities.TableTracker.GetContentTable());
            Console.WriteLine("Encoding process complete.");
            utilities.ImageTracker.PrintImagesUtilized();
        }

    }

}