using SteganographyAppCommon;
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
            new ImageStore(args).CleanAll(() =>
            {
                cleaned++;
                DisplayPercent(cleaned, args.CoverImages.Length, "Cleaning image LSB data");
            });
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
            int start = store.RequiredContentChunkTableBitSize;
            store.Next();
            store.Seek(start);
            var table = new List<int>();
            var imagesUsed = new List<string>();
            imagesUsed.Add(store.CurrentImage);
            int chunksRead = 0; //used to display how much data has been read as a percent
            using(var reader = new ContentReader(args))
            {
                string content = "";
                while((content = reader.ReadNextChunk()) != null)
                {
                    chunksRead++; //used to display how much data has been read as a percent
                    DisplayPercent(chunksRead, reader.RequiredNumberOfReads, "Encoding file contents"); //used to display how much data has been read as a percent
                    table.Add(content.Length);
                    bool stillWriting = true;
                    while (stillWriting)
                    {
                        int wrote = store.Write(content);
                        if (wrote < content.Length)
                        {
                            content = content.Substring(wrote);
                            store.Next();
                            imagesUsed.Add(store.CurrentImage);
                        }
                        else
                        {
                            stillWriting = false;
                        }
                    }
                }

                Console.WriteLine("All input file contents have been encoded.");
            }
            PrintUnused(imagesUsed);
            store.ResetTo(args.CoverImages[0]);
            Console.WriteLine("Writing content chunk table.");
            store.WriteContentChunkTable(table);
            Console.WriteLine("Encoding process complete.");
        }

        private void PrintUnused(List<string> imagesUsed)
        {
            if(imagesUsed.Count == args.CoverImages.Length)
            {
                return;
            }
            Console.WriteLine("Not all images were used when encoding/decoding the file contents.");
            Console.WriteLine("The following files were used:");
            foreach(string image in imagesUsed)
            {
                Console.WriteLine("\t{0}", image);
            }
            Console.Write("Any image not specified in the list is not needed to decode the original file.\n");
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
            store.Next();
            Console.WriteLine("Reading content chunk table.");
            var chunkTable = store.ReadContentChunkTable();
            var imagesUsed = new List<string>();
            imagesUsed.Add(store.CurrentImage);
            var less = 0;
            int chunksWritten = 0; //used to display how much data has been read as a percent
            using (var writer = new ContentWriter(args))
            {
                foreach (int length in chunkTable)
                {
                    bool stillReading = true;
                    string binary = "";
                    while (stillReading)
                    {
                        binary += store.Read(length - less);
                        if (binary.Length < length)
                        {
                            less += binary.Length;
                            store.Next();
                        }
                        else
                        {
                            less = 0;
                            writer.WriteChunk(binary);
                            chunksWritten++; //used to display how much data has been read as a percent
                            DisplayPercent(chunksWritten, chunkTable.Count, "Decoding file contents"); //used to display how much data has been read as a percent
                            binary = "";
                            stillReading = false;
                        }
                    }
                }
                Console.WriteLine("All encoded file contents has been decoded.");
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

    }
}
