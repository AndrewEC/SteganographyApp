using SixLabors.ImageSharp;
using SteganographyApp.Common;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using SteganographyApp.Common.Arguments;
using System;

namespace SteganographyAppCalculator
{

    /// <summary>
    /// Static reference class containing the number of available storage bits
    /// provided by images in common resolutions.
    /// </summary>
    public static class CommonResolutionStorageSpace
    {
        public static readonly int P360 = 518_400;
        public static readonly int P480 = 921_600;
        public static readonly int P720 = 2_764_800;
        public static readonly int P1080 = 6_220_800;
        public static readonly int P1440 = 11_059_200;
        public static readonly int P2160 = 25_012_800;
    }

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
            if(!parser.TryParse(args, out IInputArguments arguments, PostValidate))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            if (arguments.EncodeOrDecode == ActionEnum.CalculateStorageSpace)
            {
                Console.WriteLine("Calculating storage space in {0} images.", arguments.CoverImages.Length);
                CalculateStorageSpace(arguments);
            }
            else if (arguments.EncodeOrDecode == ActionEnum.CalculateEncryptedSize)
            {
                Console.WriteLine("Calculating encypted size of file {0}.", arguments.FileToEncode);
                CalculateEncryptedSize(arguments);
            }

            Console.WriteLine("");
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string PostValidate(IInputArguments input)
        {
            if (input.EncodeOrDecode != ActionEnum.CalculateStorageSpace
                && input.EncodeOrDecode != ActionEnum.CalculateEncryptedSize)
            {
                return "The action must either be calculate-storage-space or calculate-encrypted-size.";    
            }

            if (input.EncodeOrDecode == ActionEnum.CalculateEncryptedSize && Checks.IsNullOrEmpty(input.FileToEncode))
            {
                if (Checks.IsNullOrEmpty(input.FileToEncode))
                {
                    return "A file must be specified in order to calculate the encrypted file size.";
                }
                else if (input.InsertDummies && Checks.IsNullOrEmpty(input.CoverImages))
                {
                    return "When insertDummies has been specified you must also provide at least one image "
                        + "to properly calculate the number of dummy entries to insert.";
                }
            }

            if (input.EncodeOrDecode == ActionEnum.CalculateStorageSpace && Checks.IsNullOrEmpty(input.CoverImages))
            {
                return "At least one image must be specified in order to calculate the available storage space of those images.";
            }
            return null;
        }

        /// <summary>
        /// Attempts to print the help information retrieved from the help.prop file.
        /// </summary>
        private static void PrintHelp()
        {
            var parser = new HelpParser();
            if (!parser.TryParse(out HelpInfo info))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            Console.WriteLine("SteganographyApp Help\n");

            foreach(string message in info.GetMessagesFor(HelpItemSet.Calculator))
            {
                Console.WriteLine("{0}\n", message);
            }
        }

        /// <summary>
        /// Calculates the total available storage space of all the specified images.
        /// </summary>
        /// <param name="args">The InputArguments instance parsed from the user provided command
        /// line arguments.</param>
        private static void CalculateStorageSpace(IInputArguments args)
        {
            double binarySpace = 0;
            try
            {
                var tracker = new ProgressTracker(args.CoverImages.Length, "Calculating image storage space", "Completed calculating image storage space.");
                tracker.Display();
                foreach (string imagePath in args.CoverImages)
                {
                    using (var image = Image.Load(imagePath))
                    {
                        binarySpace += (image.Width * image.Height);
                    }
                    tracker.TickAndDisplay();
                }
                binarySpace *= 3;

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
        private static void CalculateEncryptedSize(IInputArguments args)
        {
            try
            {
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
        private static double GetSize(IInputArguments args)
        {
            double length = 0;
            using (var reader = new ContentReader(args))
            {
                var tracker = new ProgressTracker(reader.RequiredNumberOfReads, "Calculating file size", "Completed calculating file size");
                tracker.Display();
                string content = "";
                while ((content = reader.ReadNextChunk()) != null)
                {
                    tracker.TickAndDisplay();
                    length += content.Length;
                }
            }
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