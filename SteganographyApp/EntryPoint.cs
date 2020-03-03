using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using System;
using System.Collections.Generic;

namespace SteganographyApp
{
    public class EntryPoint
    {

        /// <summary>
        /// The model with values parsed from the user's input.
        /// </summary>
        private readonly IInputArguments args;

        /// <summary>
        /// Instantiates a new InputArguments instance with user provided
        /// command line values.
        /// </summary>
        /// <param name="args">The model with values parsed from the user's input.</param>
        public EntryPoint(IInputArguments args)
        {
            this.args = args;
        }

        /// <summary>
        /// Delegate method to determine which action to take based on the EncodeOrDecode action.
        /// </summary>
        public void Start()
        {
            switch (args.EncodeOrDecode)
            {
                case ActionEnum.Clean:
                    CleanImages();
                    break;
                case ActionEnum.Encode:
                    EncodeFileToImages();
                    break;
                case ActionEnum.Decode:
                    DecodeImagesToFile();
                    break;
            }
        }

        /// <summary>
        /// Starts the cleaning process.
        /// Uses the image store to reset the LSB values in all the user provided
        /// images to a value of 0.
        /// </summary>
        private void CleanImages()
        {
            var tracker = ProgressTracker.CreateAndDisplay(args.CoverImages.Length, "Cleaning image LSB data", "Finished cleaning all images.");
            var store = new ImageStore(args);
            store.OnNextImageLoaded += (object sender, NextImageLoadedEventArgs eventArg) =>
            {
                tracker.UpdateAndDisplayProgress();
            };
            store.CreateIOWrapper().CleanImageLSBs();
        }

        /// <summary>
        /// Starts the encoding process.
        /// Uses the ContentReader and ImageStore to read in chunks of the input file,
        /// encode the read content, write the content to the images, and then write the
        /// content chunk table to the leading image.
        /// </summary>
        private void EncodeFileToImages()
        {
            Console.WriteLine("Started encoding file {0}", args.FileToEncode);

            var imageStore = new ImageStore(args);
            var tableTracker = new TableChunkTracker(imageStore);
            var unusedTracker = UnusedImageTracker.StartRecordingFor(args, imageStore);

            using (var wrapper = imageStore.CreateIOWrapper())
            {
                int start = imageStore.RequiredContentChunkTableBitSize;

                // check that the leading image has enough storage space to store the content table
                if (!wrapper.HasEnoughSpaceForContentChunkTable())
                {
                    Console.WriteLine("There is not enough space in the leading image to store the content chunk table.");
                    Console.WriteLine("The content chunk table requires {0} bits to store for the specified input file.", start);
                    return;
                }

                // skip past the first few pixels as the leading pixels of the first image will
                // be used to store the content chunk table.
                wrapper.SeekToPixel(start);

                using (var reader = new ContentReader(args))
                {
                    var progressTracker = ProgressTracker.CreateAndDisplay(reader.RequiredNumberOfReads,
                        "Encoding file contents", "All input file contents have been encoded.");

                    string binaryChunk = "";
                    while ((binaryChunk = reader.ReadContentChunk()) != null)
                    {
                        // record the length of the encoded content so it can be stored in the
                        // content chunk table once the total encoding process has been completed.
                        wrapper.WriteBinaryChunk(binaryChunk);
                        progressTracker.UpdateAndDisplayProgress();
                    }
                }

                wrapper.Complete();
            }

            Console.WriteLine("Writing content chunk table.");
            imageStore.WriteContentChunkTable(tableTracker.ContentTable);
            Console.WriteLine("Encoding process complete.");
            unusedTracker.PrintUnusedImages();
        }

        /// <summary>
        /// Starts the decoding process.
        /// Reads the content chunk table, reads each chunk, decodes it, writes it to
        /// the output file.
        /// </summary>
        private void DecodeImagesToFile()
        {
            Console.WriteLine("Decoding data to file {0}", args.DecodedOutputFile);

            var store = new ImageStore(args);
            var unusedTracker = UnusedImageTracker.StartRecordingFor(args, store);
            
            using (var wrapper = store.CreateIOWrapper())
            {
                // read in the content chunk table so we know how many bits to read 
                Console.WriteLine("Reading content chunk table.");
                var contentChunkTable = wrapper.ReadContentChunkTable();
                var tracker = ProgressTracker.CreateAndDisplay(contentChunkTable.Count,
                    "Decoding file contents", "All encoded file contents have been decoded.");

                using (var writer = new ContentWriter(args))
                {
                    foreach (int chunkBinaryLength in contentChunkTable)
                    {
                        // as we read in each chunk from the images start writing the decoded
                        // values to the target output file.
                        string binary = wrapper.ReadBinaryChunk(chunkBinaryLength);
                        writer.WriteContentChunk(binary);
                        tracker.UpdateAndDisplayProgress();
                    }
                }
            }

            unusedTracker.PrintUnusedImages();
            Console.WriteLine("Decoding process complete.");
        }

    }
}
