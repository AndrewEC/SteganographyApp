namespace SteganographyAppCalculator
{
    using System;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.IO;

    public static class EncryptedSizeCalculator
    {
        /// <summary>
        /// Calculate the total size of the input file after base64 conversion, binary conversion,
        /// and optionally encryption if a password argument was provided.
        /// </summary>
        /// <param name="arguments">The InputArguments instance parsed from the user provided command
        /// line arguments.</param>
        public static void CalculateEncryptedSize(IInputArguments arguments)
        {
            Console.WriteLine("Calculating encypted size of file {0}.", arguments.FileToEncode);
            try
            {
                int singleChunkSize = CalculateChunkLength(arguments);
                int numberOfChunks = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);

                // Plus 1 because we need an additional entry in the chunk table to indicate the number of entries in the table
                int chunkTableSize = Calculator.CalculateRequiredBitsForContentTable(numberOfChunks);
                double size = ((double)singleChunkSize * (double)numberOfChunks) + (double)chunkTableSize;

                Console.WriteLine("\nEncrypted file size is:");
                PrintSize(size);

                Console.WriteLine("\n# of images required to store this file at common resolutions:");
                PrintComparison(size);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while encoding file: {0}", e.Message);
                if (arguments.PrintStack)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Read in a chunk of the file to encode and pass the read in bytes to the
        /// encoder util.
        /// </summary>
        /// <param name="chunkId">Used to specify where the thread will start and stop reading.
        /// The thread will use the equation chunkId * arguments.ChunkByteSize
        /// to determine where to start reading the file to encode from.</param>
        private static int CalculateChunkLength(IInputArguments arguments)
        {
            using (var contentReader = new ContentReader(arguments))
            {
                return contentReader.ReadContentChunkFromFile().Length;
            }
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