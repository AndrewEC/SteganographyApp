using SixLabors.ImageSharp;
using SteganographyApp.Common;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Help;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using System;

namespace SteganographyAppCalculator
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography Calculator\n");
            if (Array.IndexOf(args, "--help") != -1 || Array.IndexOf(args, "-h") != -1)
            {
                PrintHelp();
                return;
            }

            var parser = new ArgumentParser();
            if(!parser.TryParse(args, out InputArguments arguments))
            {
                Console.WriteLine("An exception occured while parsing provided arguments: {0}", parser.LastError.Message);
                if (parser.LastError.InnerException != null)
                {
                    Console.WriteLine("The exception was caused by: {0}", parser.LastError.InnerException.Message);
                }
                Console.WriteLine("\nRun the program with --help to get more information.");
                return;
            }

            if (arguments.EncodeOrDecode == EncodeDecodeAction.CalculateStorageSpace)
            {
                Console.WriteLine("Calculating storage space in {0} images.", arguments.CoverImages.Length);
                CalculateStorageSpace(arguments);
            }
            else if (arguments.EncodeOrDecode == EncodeDecodeAction.CalculateEncryptedSize)
            {
                Console.WriteLine("Calculating encypted size of file {0}.", arguments.FileToEncode);
                CalculateEncryptedSize(arguments);
            }

            Console.WriteLine("");
        }

        private static void PrintHelp()
        {
            var parser = new HelpParser();
            if (!parser.TryParse(out HelpInfo info))
            {
                Console.WriteLine("An error occurred while parsing the help file: {0}", parser.LastError);
                Console.WriteLine("Check that the help.prop file is in the same directory as the application and try again.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach(string message in info.GetMessagesFor("AppDescription", "AppAction", "Input", "Images", "Password", "Compress", "ChunkSize", "RandomSeed", "Dummies"))
            {
                Console.WriteLine("{0}\n", message);
            }
        }

        /// <summary>
        /// Calculates the total available storage space of all the specified images.
        /// </summary>
        /// <param name="args">The InputArguments instance parsed from the user provided command
        /// line arguments.</param>
        private static void CalculateStorageSpace(InputArguments args)
        {
            double binarySpace = 0;
            try
            {
                int count = 0;
                foreach (string imagePath in args.CoverImages)
                {
                    using (var image = Image.Load(imagePath))
                    {
                        binarySpace += (image.Width * image.Height);
                        count++;
                        DisplayPercent(count, args.CoverImages.Length, "Calculating image storage space");
                    }
                }
                binarySpace *= 3;
                Console.WriteLine("Completing calculating image storage space.");

                Console.WriteLine("\nImages are able to store:");
                PrintSize(binarySpace);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured while calculating storage space: {0}", e.Message);
                if (args.PrintStack)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return;
            }
        }

        /// <summary>
        /// Calculate the total size of the input file after base64 conversion, binary conversion,
        /// and optionally encryption if a password argument was provided.
        /// </summary>
        /// <param name="args">The InputArguments instance parsed from the user provided command
        /// line arguments.</param>
        private static void CalculateEncryptedSize(InputArguments args)
        {
            try
            {
                Console.WriteLine("\nCalculating encrypted size.");
                double size = GetSize(args);

                Console.WriteLine("\nEncrypted file size is:");
                PrintSize(size);

                Console.WriteLine("\n# of images required to store this file at common resolutions:");
                PrintComparison(size);
            }
            catch (TransformationException e)
            {
                Console.WriteLine("An error occured while encoding file: {0}", e.Message);
                if (args.PrintStack)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Calculate the size of the input file.
        /// </summary>
        /// <param name="args">The values parsed from the command line arguments.</param>
        /// <param name="compressed">States whether or not to compress the file or not. Overwrites the current
        /// value in the args parameter.</param>
        /// <returns>The size of the file in bits.</returns>
        private static int GetSize(InputArguments args)
        {
            int length = 0;
            int reads = 0;
            using (var reader = new ContentReader(args))
            {
                string content = "";
                while ((content = reader.ReadNextChunk()) != null)
                {
                    reads++;
                    DisplayPercent(reads, reader.RequiredNumberOfReads, "Calculating file size");
                    length += content.Length;
                }
            }
            length += new ImageStore(args).RequiredContentChunkTableBitSize;
            Console.WriteLine("Completed calculating file size");
            return length;
        }

        /// <summary>
        /// Displays a message with a completion percent.
        /// </summary>
        /// <param name="cur">The current step in the process.</param>
        /// <param name="max">The value dictating the point of completion.</param>
        /// <param name="prefix">The message to prefix to the calculated percentage.</param>
        private static void DisplayPercent(double cur, double max, string prefix)
        {
            double percent = cur / max * 100.0;
            if (percent > 100.0)
            {
                percent = 100.0;
            }
            Console.Write("{0} :: {1}%\r", prefix, (int)percent);
        }

        /// <summary>
        /// Prints out the binary size of an encypted file or storage space in bits, bytes,
        /// megabytes and gigabytes.
        /// </summary>
        /// <param name="size">The size in bits to print out.</param>
        private static void PrintSize(double size)
        {
            Console.WriteLine("\t{0} bits", size);
            Console.WriteLine("\t{0} bytes", size / 8);
            Console.WriteLine("\t{0} KB", size / 8 / 1024);
            Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
        }

        /// <summary>
        /// Prints how many images at common resolutions it would take to store this content.
        /// </summary>
        /// <param name="size">The size of the encoded file in bits.</param>
        private static void PrintComparison(double size)
        {
            Console.WriteLine("\tAt 360p: \t{0}", size / CommonResolutionStorageSpace.P360);
            Console.WriteLine("\tAt 480p: \t{0}", size / CommonResolutionStorageSpace.P480);
            Console.WriteLine("\tAt 720p: \t{0}", size / CommonResolutionStorageSpace.P720);
            Console.WriteLine("\tAt 1080p: \t{0}", size / CommonResolutionStorageSpace.P1080);
            Console.WriteLine("\tAt 1440p: \t{0}", size / CommonResolutionStorageSpace.P1440);
            Console.WriteLine("\tAt 4K (2160p): \t{0}", size / CommonResolutionStorageSpace.P2160);
        }
    }
}