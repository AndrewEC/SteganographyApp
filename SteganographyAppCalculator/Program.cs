using ImageSharp;
using SteganographyAppCommon;
using SteganographyAppCommon.Data;
using SteganographyAppCommon.IO;
using SteganographyAppCommon.IO.Content;
using System;

namespace SteganographyAppCalculator
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography Calculator\n");
            if (Array.IndexOf(args, "--help") != -1)
            {
                PrintHelp();
                return;
            }
            try
            {
                var inputArguments = ArgumentParser.Parse(args);
                if (inputArguments.EncodeOrDecode == EncodeDecodeAction.CalculateStorageSpace)
                {
                    Console.WriteLine("Calculating storage space in {0} images.", inputArguments.CoverImages.Length);
                    CalculateStorageSpace(inputArguments);
                }
                else if (inputArguments.EncodeOrDecode == EncodeDecodeAction.CalculateEncryptedSize)
                {
                    Console.WriteLine("Calculating encypted size of file {0}.", inputArguments.FileToEncode);
                    CalculateEncryptedSize(inputArguments);
                }
            }
            catch (ArgumentParseException e)
            {
                Console.WriteLine("An exception occured while parsing provided arguments: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("The exception was caused by: {0}", e.InnerException.Message);
                }
                Console.WriteLine("\nRun the program with --help to get more information.");
            }

            Console.WriteLine("");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("Arguments must be specified with and = sign and no spaces.");
            Console.WriteLine("\tExample of calculating required space to store an encrypted file:");
            Console.WriteLine("\t\tdotnet .\\SteganographyAppCalculator --action=calculate-encrypted-size --input=FileToEncode.zip --password=Pass1234");
            Console.WriteLine("\tExample of calculating the amount of storage space offered by a set of images:");
            Console.WriteLine("\t\tdotnet .\\SteganographyAppCalculator --action=calculate-storage-space --images=001.png,002.png\n");
            Console.WriteLine("\t--action or -a :: Specifies whether to calculate the encrypted size of a file or the amount of storage space offered by a set of images.");
            Console.WriteLine("\t\tValue must be either 'calculate-storage-space' or 'calculate-encrypted-size'.");
            Console.WriteLine("\t--input or -i :: The path to the file to encode if 'encode' was specified in the action argument.");
            Console.WriteLine("\t--images or -im :: A comma delimited list of paths to images to be either encoded or decoded");
            Console.WriteLine("\t\tThe order of the images affects the encoding and decoding results.");
            Console.WriteLine("\t--passsword or -p :: The password to encrypt the input file when 'encode' was specified in the action argument.");
            Console.WriteLine();
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
    }
}