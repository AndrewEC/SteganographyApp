namespace SteganographyApp.Common;

using System;

using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Interface for integrating with the Calculator implementation.
/// </summary>
public interface ICalculator
{
    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateRequiredNumberOfWrites/*' />
    int CalculateRequiredNumberOfWrites(string fileToEncode, int chunkByteSize);

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateRequiredBitsForContentTable1/*' />
    int CalculateRequiredBitsForContentTable(string fileToEncode, int chunkByteSize);

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateRequiredBitsForContentTable2/*' />
    int CalculateRequiredBitsForContentTable(int numberOfChunks);

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateStorageSpaceOfImage/*' />
    long CalculateStorageSpaceOfImage(int width, int height, int bitsToUse);
}

/// <summary>
/// Utility class for performing some basic calculations to help in reading and writing
/// the content chunk table.
/// </summary>
public class Calculator(IFileIOProxy fileIOProxy) : ICalculator
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

    private readonly IFileIOProxy fileIOProxy = fileIOProxy;

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateRequiredNumberOfWrites/*' />
    public int CalculateRequiredNumberOfWrites(string fileToEncode, int chunkByteSize)
    {
        long fileSizeBytes = fileIOProxy.GetFileSizeBytes(fileToEncode);
        return (int)Math.Ceiling((double)fileSizeBytes / chunkByteSize);
    }

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateRequiredBitsForContentTable1/*' />
    public int CalculateRequiredBitsForContentTable(string fileToEncode, int chunkByteSize)
    {
        int requiredNumberOfWrites = CalculateRequiredNumberOfWrites(fileToEncode, chunkByteSize);
        return CalculateRequiredBitsForContentTable(requiredNumberOfWrites);
    }

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateRequiredBitsForContentTable2/*' />
    public int CalculateRequiredBitsForContentTable(int numberOfChunks)
        => (numberOfChunks * ChunkDefinitionBitSizeWithPadding) + ChunkTableHeaderSizeWithPadding;

    /// <summary>
    /// Computes the number of bits that can be stored in a given pixel. This effectively equals the prduct
    /// of 3 and the bitsToUse.
    /// </summary>
    /// <param name="bitsToUse">The number of bits to store in each RGB channel of the pixel.</param>
    /// <returns>The product of 3 and the number of bitsToUse per RGB channel of a given pixel.</returns>
    public int CalculateBitsPerPixel(int bitsToUse) => MinimumBitsPerPixel * bitsToUse;

    /// <include file='docs.xml' path='docs/members[@name="Calculator"]/CalculateStorageSpaceOfImage/*' />
    public long CalculateStorageSpaceOfImage(int width, int height, int bitsToUse)
        => width * height * CalculateBitsPerPixel(bitsToUse);
}