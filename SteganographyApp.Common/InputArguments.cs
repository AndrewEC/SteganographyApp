namespace SteganographyApp.Common;

using System;
using System.Collections.Immutable;

#pragma warning disable SA1402

/// <summary>
/// Generic interface containing all the getter methods required for retrieving
/// all the parsed input argument values.
/// </summary>
public interface IInputArguments
{
    /// <summary>
    /// Gets the password used to encrypt or decrypt the contents of a file.
    /// Only used when encoding or decoding a file.
    /// </summary>
    string Password { get; }

    /// <summary>
    /// Gets the relative or absolute path to the file to encode. Only used when encoding a file.
    /// </summary>
    string FileToEncode { get; }

    /// <summary>
    /// Gets the path the decoded file contents will be written to. Only used when decoding a file.
    /// </summary>
    string DecodedOutputFile { get; }

    /// <summary>
    /// Gets the paths to the images to read from or write to. This may be used when encoding a file,
    /// decoding a file, cleaning the images, or calculating the storage space of a series of images.
    /// </summary>
    ImmutableArray<string> CoverImages { get; }

    /// <summary>
    /// Gets a value indicating whether or not the contents being encoded, decoded, or calculate,
    /// should be compressed using a standard GZip compression algorithm.
    /// </summary>
    bool UseCompression { get; }

    /// <summary>
    /// Gets a starting seed for the random number generator used when encoding, decoding, and calculating encrypted
    /// size of a file.
    /// </summary>
    string RandomSeed { get; }

    /// <summary>
    /// Gets the number of dummies to insert or remove from the contents of a file when encoding, decoding,
    /// and calculating the encrypted size of a file.
    /// </summary>
    int DummyCount { get; }

    /// <summary>
    /// Gets a value indicating whether or not the original image should be deleted after being converted
    /// to a PNG image. Only used by the Converter.
    /// </summary>
    bool DeleteAfterConversion { get; }

    /// <summary>
    /// Gets the chunk size. I.e. the number of bytes to read, encode, and write at any given time.
    /// <para>Higher values will improve the time to encode a larger file and reduce
    /// the overall encoded file size though values too high run the risk of having memory related errors.</para>
    /// <para>Default value of 131,072.</para>
    /// </summary>
    int ChunkByteSize { get; }

    /// <summary>
    /// Gets the desired output file type. Can be either png or webp.
    /// <para>Webp is a smaller format but takes much longer to process than a png.</para>
    /// <para>Can only be used with the converter.</para>
    /// </summary>
    ImageFormat ImageFormat { get; }

    /// <summary>
    /// Gets the additional number of times the password should be hashed before being encrypting or decrypting content.
    /// <para>Has no effect if no password has been provided.</para>
    /// </summary>
    int AdditionalPasswordHashIterations { get; }

    /// <summary>
    /// Gets the number of bits to read or write, starting the least significant bit, from each RGB value in
    /// each pixel from the cover images.
    /// </summary>
    int BitsToUse { get; }
}

/// <summary>
/// Immutable class containg the parsed argument values. All of the properties within this class
/// are immutable and readonly.
/// </summary>
public sealed class CommonArguments : IInputArguments
{
    /// <inheritdoc/>
    public string Password { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string FileToEncode { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string DecodedOutputFile { get; set; } = string.Empty;

    /// <inheritdoc/>
    public ImmutableArray<string> CoverImages { get; set; } = [];

    /// <inheritdoc/>
    public bool UseCompression { get; set; } = false;

    /// <inheritdoc/>
    public string RandomSeed { get; set; } = string.Empty;

    /// <inheritdoc/>
    public int DummyCount { get; set; } = 0;

    /// <inheritdoc/>
    public bool DeleteAfterConversion { get; set; } = false;

    /// <inheritdoc/>
    public int ChunkByteSize { get; set; } = 131_072;

    /// <inheritdoc/>
    public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;

    /// <inheritdoc/>
    public int AdditionalPasswordHashIterations { get; set; } = 0;

    /// <inheritdoc/>
    public int BitsToUse { get; set; } = 1;
}

/// <summary>
/// The exceptio thrown when an error occurs while trying to parse a command line argument value.
/// </summary>
/// <remarks>
/// Initializes the exception instance.
/// </remarks>
/// <param name="message">The message to initialize the exception with.</param>
public sealed class ArgumentValueException(string message) : Exception(message) { }

#pragma warning restore SA1402