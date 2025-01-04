namespace SteganographyApp.Common;

using System;

using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Injection.Proxies;

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
    /// Indicates the minimum number of bits that can be stored in a single pixel.
    /// </summary>
    public static readonly int MinimumBitsPerPixel = 3;

    /// <summary>
    /// Specifies the number of bits that will be reserved for each entry in the content
    /// chunk table.
    /// </summary>
    public static readonly int ChunkDefinitionBitSizeWithPadding = 33;

    /// <summary>
    /// Specifies the number of bits that will be reserved for the header of the content
    /// chunk table.
    /// </summary>
    public static readonly int ChunkTableHeaderSizeWithPadding = 18;

    /// <summary>
    /// Specifies the number of times the file has to be read from, encoded, and written to the storage
    /// image. The number of writes is essentially based on the total size of the image divided by the
    /// size, in bytes, of each chunk that will be read from the input file to encode.
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
        return CalculateRequiredBitsForContentTable(requiredNumberOfWrites);
    }

    /// <summary>
    /// Returns a count of the total number of bits that will be required to store a content chunk table
    /// given the number of chunks that will need to be written to said table.
    /// </summary>
    /// <param name="numberOfChunks">The number of chunks that will need to be written to the content chunk table
    /// less the chunk table header.</param>
    /// <returns>A count of the total number of bits that will be required to store the content chunk table.</returns>
    public static int CalculateRequiredBitsForContentTable(int numberOfChunks) => (numberOfChunks * ChunkDefinitionBitSizeWithPadding) + ChunkTableHeaderSizeWithPadding;

    /// <summary>
    /// Computes the number of bits that can be stored in a given pixel. This effectively equals the prduct
    /// of 3 and the bitsToUse.
    /// </summary>
    /// <param name="bitsToUse">The number of bits to store in each RGB channel of the pixel.</param>
    /// <returns>The product of 3 and the number of bitsToUse per RGB channel of a given pixel.</returns>
    public static int CalculateBitsPerPixel(int bitsToUse) => MinimumBitsPerPixel * bitsToUse;

    /// <summary>
    /// Computes the number of bits that can be stored in the image.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="bitsToUse">The number of bits to store in each RGB channel of each pixel.</param>
    /// <returns>The total number of bits that can be stored in an image of the given width and height.</returns>
    public static long CalculateStorageSpaceOfImage(int width, int height, int bitsToUse) => width * height * CalculateBitsPerPixel(bitsToUse);
}