namespace SteganographyApp.Common
{
    using System;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Utility class for performing some basic calculations to help in reading and writing
    /// the content chunk table.
    /// </summary>
    public static class Calculator
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
        /// <returns>A count of the number of times the file must be read from, encoded, and written for
        /// the encoding process to complete.</returns>
        public static int CalculateRequiredNumberOfWrites(string fileToEncode, int chunkByteSize)
        {
            long fileSizeBytes = Injector.Provide<IFileIOProxy>().GetFileSizeBytes(fileToEncode);
            return (int)Math.Ceiling((double)fileSizeBytes / chunkByteSize);
        }

        /// <summary>
        /// The content chunk table is a table containing the sizes of the encoded output after each
        /// read/write/encode iteration. So the number of bits required for the content chunk table will
        /// be the number of chunk table entries required plus one (plus one is to account for the table header
        /// that indicates how many entries there are in the chunk table) times the size of each table entry (33 bits).
        /// </summary>
        /// <param name="fileToEncode">The path to the file that is going to be encoded.</param>
        /// <param name="chunkByteSize">The number of bytes to read in at a time.</param>
        /// <returns>A count of the total number of bits that will be required to store the content chunk table
        /// so the contents of the file can be decoded from the cover images later.</returns>
        public static int CalculateRequiredBitsForContentTable(string fileToEncode, int chunkByteSize)
        {
            int requiredNumberOfWrites = CalculateRequiredNumberOfWrites(fileToEncode, chunkByteSize);
            return (requiredNumberOfWrites + 1) * ChunkDefinitionBitSizeWithPadding;
        }

        /// <summary>
        /// Returns a count of the total number of bits that will be required to store a content chunk table
        /// given the number of chunks that will need to be written to said table.
        /// </summary>
        /// <param name="numberOfChunks">The number of chunks that will need to be written to the content chunk table
        /// less the chunk table header.</param>
        /// <returns>A count of the total number of bits that will be required to store the content chunk table.</returns>
        public static int CalculateRequiredBitsForContentTable(int numberOfChunks) => (numberOfChunks + 1) * ChunkDefinitionBitSizeWithPadding;
    }
}