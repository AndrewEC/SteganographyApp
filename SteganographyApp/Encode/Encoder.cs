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
        Complete
    }

    /// <summary>
    /// Specifies the values to be added to our read queue by the file read
    /// thread.
    /// </summary>
    struct ReadArgs
    {
        public Status Status;
        public string Data;
    }

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
        /// </summary>
        private readonly BlockingCollection<ReadArgs> readQueue;
        
        /// <summary>
        /// Allows the thread performing the file read and encoding to communicate errors
        /// back to the encoder so the thread can be closed and a proper exception message
        /// displayed.
        /// <summary>
        private readonly BlockingCollection<Exception> errorQueue;

        private Encoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            readQueue = new BlockingCollection<ReadArgs>(2);
            errorQueue = new BlockingCollection<Exception>(1);
        }

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
        public void EncodeFileToImage()
        {
            Console.WriteLine("Encoding File: {0}", arguments.FileToEncode);

            var utilities = new EncodingUtilities(arguments);

            using (var wrapper = utilities.ImageStore.CreateIOWrapper())
            {
                int start = Calculator.CalculateRequiredBitsForContentTable(arguments.FileToEncode, arguments.ChunkByteSize);

                if (!wrapper.HasEnoughSpaceForContentChunkTable())
                {
                    Console.WriteLine("There is not enough space in the leading image to store the content chunk table.");
                    Console.WriteLine("The content chunk table requires {0} bits to store for the specified input file.", start);
                    return;
                }

                Encode(wrapper, start);
            }

            Cleanup(utilities);
        }

        private void Encode(ImageStore.ImageStoreWrapper wrapper, int startingPixel)
        {
            wrapper.SeekToPixel(startingPixel);

            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);
            var progressTracker = ProgressTracker.CreateAndDisplay(requiredNumberOfWrites,
                "Encoding file contents", "All input file contents have been encoded.");

            var thread = FileReadThread.CreateAndStart(readQueue, arguments, errorQueue);

            while(true)
            {
                var readArgs = readQueue.Take();
                if (readArgs.Status == Status.Complete)
                {
                    thread.Join();
                    wrapper.EncodeComplete();
                    break;
                }
                else
                {
                    wrapper.WriteContentChunkToImage(readArgs.Data);
                    progressTracker.UpdateAndDisplayProgress();
                }

                if (errorQueue.TryTake(out Exception exception))
                {
                    thread.Join();
                    throw exception;
                }
            }
        }

        private void Cleanup(EncodingUtilities utilities)
        {
            Console.WriteLine("Writing content chunk table.");
            utilities.ImageStore.WriteContentChunkTable(utilities.TableTracker.ContentTable);
            Console.WriteLine("Encoding process complete.");
            utilities.ImageTracker.PrintImagesUtilized();
        }

    }

}