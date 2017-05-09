using System;

using ImageSharp;
using SteganographyAppCommon;

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

            Console.WriteLine("\nPress enter to continue...");
            Console.ReadLine();
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
                foreach (string imagePath in args.CoverImages)
                {
                    using (var image = Image.Load(imagePath))
                    {
                        binarySpace += (image.Width * image.Height);
                    }
                }
                binarySpace *= 3;
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

            Console.WriteLine("Images are able to store:");
            PrintSize(binarySpace);
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
                Console.WriteLine("Calculating compressed size.");
                double compressedSpace = GetSize(args, true);

                Console.WriteLine("Calculating uncompressed size.");
                double uncompressedSpace = GetSize(args, false);

                Console.WriteLine("\nCompressed file size is:");
                PrintSize(compressedSpace);

                Console.WriteLine("\nUncompressed file size is:");
                PrintSize(uncompressedSpace);

                Console.WriteLine("\nUsing compression you will save: ");
                PrintSize(uncompressedSpace - compressedSpace);

                Console.WriteLine("\nRequired images to store file at common resolutions:");
                PrintComparison((compressedSpace < uncompressedSpace) ? compressedSpace : uncompressedSpace);
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

        private static int GetSize(InputArguments args, bool compressed)
        {
            int length = 0;
            bool old = args.UseCompression;
            args.UseCompression = compressed;
            using (var reader = new ContentReader(args))
            {
                string content = "";
                while ((content = reader.ReadNextChunk()) != null)
                {
                    length += content.Length;
                }
            }
            args.UseCompression = old;
            length += new ImageStore(args).RequiredContentChunkTableBitSize;
            return length;
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