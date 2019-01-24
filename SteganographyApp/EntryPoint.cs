using SteganographyApp.Common;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyApp
{
    public class EntryPoint
    {

        /// <summary>
        /// The model with values parsed from the user's input.
        /// </summary>
        private readonly InputArguments args;

        /// <summary>
        /// Instantiates a new InputArguments instance with user provided
        /// command line values.
        /// </summary>
        /// <param name="args">The model with values parsed from the user's input.</param>
        public EntryPoint(InputArguments args)
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
                case EncodeDecodeAction.Clean:
                    StartClean();
                    break;
                case EncodeDecodeAction.Encode:
                    StartEncode();
                    break;
                case EncodeDecodeAction.Decode:
                    StartDecode();
                    break;
            }
        }

        /// <summary>
        /// Starts the cleaning process.
        /// Uses the image store to reset the LSB values in all the user provided
        /// images to a value of 0.
        /// </summary>
        private void StartClean()
        {
            int cleaned = 0;
            DisplayPercent(cleaned, args.CoverImages.Length, "Cleaning image LSB data");
            var store = new ImageStore(args);
            store.OnNextImageLoaded += (object sender, NextImageLoadedEventArgs eventArg) =>
            {
                cleaned++;
                DisplayPercent(cleaned, args.CoverImages.Length, "Cleaning image LSB data");
            };
            store.CleanAll();
            Console.WriteLine("Finished cleaning all {0} images.", args.CoverImages.Length);
        }

        /// <summary>
        /// Starts the encoding process.
        /// Uses the ContentReader and ImageStore to read in chunks of the input file,
        /// encode the read content, write the content to the images, and then write the
        /// content chunk table to the leading image.
        /// </summary>
        private void StartEncode()
        {
            Console.WriteLine("Started encoding file {0}", args.FileToEncode);

            var store = new ImageStore(args);
            var table = new List<int>();
            int start = store.RequiredContentChunkTableBitSize;

            var imagesUsed = new List<string>();
            store.OnNextImageLoaded += (object sender, NextImageLoadedEventArgs args) =>
            {
                imagesUsed.Add(args.ImageName);
            };

            using (var wrapper = store.CreateIOWrapper())
            {
                // skip past the first few pixels as the leading pixels of the first image will
                // be used to store the content chunk table.
                wrapper.Seek(start);

                int chunksRead = 0; //used to display how much data has been read as a percent

                // check that the leading image has enough storage space to store the content table
                if (!wrapper.HasEnoughSpaceForContentChunkTable())
                {
                    Console.WriteLine("There is not enough space in the leading image to store the content chunk table.");
                    Console.WriteLine("The content chunk table requires {0} bits to store for the specified input file.", start);
                    return;
                }

                using (var reader = new ContentReader(args))
                {
                    string content = "";
                    while ((content = reader.ReadNextChunk()) != null)
                    {
                        // record the length of the encoded content so it can be stored in the
                        // content chunk table once the total encoding process has been completed.
                        table.Add(content.Length);
                        wrapper.Write(content);
                        chunksRead++; //used to display how much data has been read as a percent
                        DisplayPercent(chunksRead, reader.RequiredNumberOfReads, "Encoding file contents"); //used to display how much data has been read as a percent
                    }

                    Console.WriteLine("All input file contents have been encoded.");
                }

                wrapper.Complete();
            }

            PrintUnused(imagesUsed);
            Console.WriteLine("Writing content chunk table.");
            store.WriteContentChunkTable(table);
            Console.WriteLine("Encoding process complete.");
        }

        /// <summary>
        /// Starts the decoding process.
        /// Reads the content chunk table, reads each chunk, decodes it, writes it to
        /// the output file.
        /// </summary>
        private void StartDecode()
        {
            Console.WriteLine("Decoding data to file {0}", args.DecodedOutputFile);

            var store = new ImageStore(args);
            var imagesUsed = new List<string>();
            store.OnNextImageLoaded += (object sender, NextImageLoadedEventArgs args) =>
            {
                imagesUsed.Add(args.ImageName);
            };
            
            using (var wrapper = store.CreateIOWrapper())
            {
                // read in the content chunk table so we know how many bits to read 
                Console.WriteLine("Reading content chunk table.");
                var chunkTable = wrapper.ReadContentChunkTable();

                using (var writer = new ContentWriter(args))
                {
                    int chunksWritten = 0; //used to display how much data has been read as a percent
                    foreach (int length in chunkTable)
                    {
                        // as we read in each chunk from the images start writing the decoded
                        // values to the target output file.
                        string binary = wrapper.Read(length);
                        writer.WriteChunk(binary);
                        chunksWritten++; //used to display how much data has been read as a percent
                        DisplayPercent(chunksWritten, chunkTable.Count, "Decoding file contents"); //used to display how much data has been read as a percent
                    }
                    Console.WriteLine("All encoded file contents has been decoded.");
                }
            }

            PrintUnused(imagesUsed);
            Console.WriteLine("Decoding process complete.");
        }

        /// <summary>
        /// Displays a message with a completion percent.
        /// </summary>
        /// <param name="cur">The current step in the process.</param>
        /// <param name="max">The value dictating the point of completion.</param>
        /// <param name="prefix">The message to prefix to the calculated percentage.</param>
        private void DisplayPercent(double cur, double max, string prefix)
        {
            double percent = cur / max * 100.0;
            if(percent > 100.0)
            {
                percent = 100.0;
            }
            Console.Write("{0} :: {1}%\r", prefix, (int)percent);
        }

        /// <summary>
        /// Utility method to print out a list the list of images that were used in the
        /// encoding decoding process. Will only print something out it the number of image
        /// uses was less than the number of images parsed in the arguments.
        /// </summary>
        /// <param name="imagesUsed">A list containing the names of the images used in the
        /// encoding/decoding process.</param>
        private void PrintUnused(List<string> imagesUsed)
        {
            if (imagesUsed.Count == args.CoverImages.Length)
            {
                return;
            }
            Console.WriteLine("Not all images were used when encoding/decoding the file contents.");
            Console.WriteLine("The following files were used:");
            foreach (string image in imagesUsed)
            {
                Console.WriteLine("\t{0}", image);
            }
            Console.Write("Any image not specified in the list is not needed to decode the original file.\n");
        }

    }
}
