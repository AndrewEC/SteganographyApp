namespace SteganographyApp.Common
{
    using System;

    using SteganographyApp.Common.Injection;

    public class Calculator
    {
        /// <summary>
        /// Specifies the number of bits that will be reserved for each entry in the content
        /// chunk table.
        /// </summary>
        public static readonly int ChunkDefinitionBitSize = 32;

        /// <summary>
        /// Indicates the maximum number of bits that can be stored in a single pixel.
        /// </summary>
        public static readonly int BitsPerPixel = 3;

        /// <summary>
        /// Specifies the number of bits that will be reserved for each entry in the content
        /// chunk table + 1. The plus one is because each pixel in the image stores 3 bits
        /// and since 11 pixels are used to store a content chunk table entry then it can store 33
        /// bits in total. Although only 32 of them are needed for the actual entry.
        /// </summary>
        public static readonly int ChunkDefinitionBitSizeWithPadding = 33;

        /// <summary>
        /// Specifies the number of times the file has to be read from, encoded, and written to the storage
        /// image. The number of writes is essentially based on the total size of the image divided by the
        /// number of bytes to read from each iteration from the input file.
        /// </summary>
        /// <param name="fileToEncode">The path to the file that is going to be encoded.</param>
        /// <param name="chunkByteSize">The number of bytes to read in at a time.</param>
        public static int CalculateRequiredNumberOfWrites(string fileToEncode, int chunkByteSize)
        {
            long fileSizeBytes = Injector.Provide<IFileIOProxy>().GetFileSizeBytes(fileToEncode);
            return (int)Math.Ceiling((double)fileSizeBytes / chunkByteSize);
        }

        /// <summary>
        /// The table size is essentially the number of read/encode/write iterations times the number
        /// of RGB bytes required to store the content chunk table.
        /// Each time we read and encode the a portion of the input file we will write an entry to the content chunk table
        /// outlining the number of bits that were written at the time of the write so we know how to decode
        /// and rebuild the input file when we are decoding.
        /// </summary>
        public static int CalculateRequiredBitsForContentTable(string fileToEncode, int chunkByteSize)
        {
            int requiredNumberOfWrites = CalculateRequiredNumberOfWrites(fileToEncode, chunkByteSize);
            return (requiredNumberOfWrites * ChunkDefinitionBitSize) + ChunkDefinitionBitSize + requiredNumberOfWrites;
        }
    }
}