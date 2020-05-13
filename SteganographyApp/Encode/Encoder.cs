using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;
using SteganographyApp.Common;

using System;
using System.Collections.Concurrent;

namespace SteganographyApp.Encode
{

    enum Status
    {
        Incomplete,
        Complete
    }

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
        private readonly BlockingCollection<ReadArgs> readQueue;
        private readonly BlockingCollection<Exception> errorQueue;

        public Encoder(IInputArguments arguments)
        {
            this.arguments = arguments;
            readQueue = new BlockingCollection<ReadArgs>(2);
            errorQueue = new BlockingCollection<Exception>(1);
        }

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

                wrapper.SeekToPixel(start);

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

            EncodeCleanup(utilities);
        }

        private void EncodeCleanup(EncodingUtilities utilities)
        {
            Console.WriteLine("Writing content chunk table.");
            utilities.ImageStore.WriteContentChunkTable(utilities.TableTracker.ContentTable);
            Console.WriteLine("Encoding process complete.");
            utilities.ImageTracker.PrintImagesUtilized();
        }

    }

}