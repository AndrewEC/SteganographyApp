namespace SteganographyApp.Common.Data;

using System;

using SteganographyApp.Common.Logging;

/// <summary>
/// The facade component that handle transforming the input data
/// through the various configured steps.
/// </summary>
public interface IDataEncoderUtil
{
    /// <summary>
    /// Takes in a raw byte array read from the input file and converts it to an encoded
    /// binary string.
    /// </summary>
    /// <param name="bytes">The array of bytes to be encoded to a binary string.</param>
    /// <param name="password">The password used to encrypt the contents of the file.</param>
    /// <param name="useCompression">Indicates if the byte array should be gzip
    /// compressed before converting it to the final binary string.</param>
    /// <param name="dummyCount">The number of dummies entries to insert into the
    /// input byte array. Requires the <paramref name="randomSeed"/> to be set.</param>
    /// <param name="randomSeed">Used to initialize the pseudo-random number generator
    /// when inserting dummies and randomizing the byte array.</param>
    /// <param name="additionalHashIterations">The additional number of times the password
    /// and random seed should be hashed before being used as a key in any step in this process.
    /// </param>
    /// <returns>A binary string representing the final encoded byte data.</returns>
    string Encode(
        byte[] bytes,
        string password,
        bool useCompression,
        int dummyCount,
        string randomSeed,
        int additionalHashIterations);

    /// <summary>
    /// Takes in the binary data read from the LSBs of one or more cover image,
    /// converts it to it's byte representation, and decodes it into it's original
    /// byte array provided when encoding.
    /// </summary>
    /// <param name="binary">The binary data read from the cover images to be decoded.</param>
    /// <param name="password">The password used to decrypt the input data.</param>
    /// <param name="useCompression">Indicates if the input data has been gzip compressed
    /// and needs to be decompressed.</param>
    /// <param name="dummyCount">The number of dummies entries to removed from the
    /// byte data. Requires the <paramref name="randomSeed"/> to be set.</param>
    /// <param name="randomSeed">Used to initialize the pseudo-random number generator
    /// when removing dummies and reordering the byte array.</param>
    /// <param name="additionalHashIterations">The additional number of times the password
    /// and random seed should be hashed before being used as a key in any step in this process.
    /// </param>
    /// <returns>The byte representation of the fully decoded binary string.</returns>
    byte[] Decode(string binary, string password, bool useCompression, int dummyCount, string randomSeed, int additionalHashIterations);
}

/// <inheritdoc/>
public sealed class DataEncoderUtil(
    IKeyUtil keyUtil,
    IEncryptionUtil encryptionUtil,
    IDummyUtil dummyUtil,
    IRandomizeUtil randomizeUtil,
    ICompressionUtil compressionUtil,
    IBinaryUtil binaryUtil) : IDataEncoderUtil
{
    private readonly ILogger log = new LazyLogger<DataEncoderUtil>();

    private readonly IKeyUtil keyUtil = keyUtil;
    private readonly IEncryptionUtil encryptionUtil = encryptionUtil;
    private readonly IDummyUtil dummyUtil = dummyUtil;
    private readonly IRandomizeUtil randomizeUtil = randomizeUtil;
    private readonly ICompressionUtil compressionUtil = compressionUtil;
    private readonly IBinaryUtil binaryUtil = binaryUtil;

    /// <inheritdoc/>
    public string Encode(
        byte[] bytes,
        string password,
        bool useCompression,
        int dummyCount,
        string randomSeed,
        int additionalHashIterations)
    {
        log.Trace("Before encoding: [{0}]", () => [Convert.ToBase64String(bytes)]);

        if (randomSeed != string.Empty)
        {
            byte[] randomKey = keyUtil.GenerateKey(randomSeed, additionalHashIterations);
            randomSeed = Convert.ToBase64String(randomKey);
        }

        if (password != string.Empty)
        {
            try
            {
                bytes = encryptionUtil.Encrypt(bytes, password, additionalHashIterations);
            }
            catch (Exception e)
            {
                throw new TransformationException("An exception occured while encrypting content.", e);
            }

            log.Trace("After encryption: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (dummyCount > 0 && randomSeed != string.Empty)
        {
            bytes = dummyUtil.InsertDummies(dummyCount, bytes, randomSeed);
            log.Trace("After inserting dummies: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (randomSeed != string.Empty)
        {
            bytes = randomizeUtil.Randomize(bytes, randomSeed);
            log.Trace("After randomizing: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (useCompression)
        {
            bytes = compressionUtil.Compress(bytes);
            log.Trace("After compression: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        return binaryUtil.ToBinaryString(bytes);
    }

    /// <inheritdoc/>
    public byte[] Decode(
        string binary,
        string password,
        bool useCompression,
        int dummyCount,
        string randomSeed,
        int additionalHashIterations)
    {
        byte[] bytes = binaryUtil.ToBytes(binary);

        log.Trace("Original binary: [{0}]", binary);
        log.Trace("Before decoding: [{0}]", () => [Convert.ToBase64String(bytes)]);

        if (randomSeed != string.Empty)
        {
            byte[] randomKey = keyUtil.GenerateKey(randomSeed, additionalHashIterations);
            randomSeed = Convert.ToBase64String(randomKey);
        }

        if (useCompression)
        {
            try
            {
                bytes = compressionUtil.Decompress(bytes);
                log.Trace("After decompressing: [{0}]", () => [Convert.ToBase64String(bytes)]);
            }
            catch (Exception e)
            {
                throw new TransformationException("An exception occurred while decompressing content.", e);
            }
        }

        if (randomSeed != string.Empty)
        {
            bytes = randomizeUtil.Reorder(bytes, randomSeed);
            log.Trace("After reordering: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (dummyCount > 0 && randomSeed != string.Empty)
        {
            bytes = dummyUtil.RemoveDummies(dummyCount, bytes, randomSeed);
            log.Trace("After removing dummies: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (password != string.Empty)
        {
            try
            {
                bytes = encryptionUtil.Decrypt(bytes, password, additionalHashIterations);
                log.Trace("After decrypting: [{0}]", () => [Convert.ToBase64String(bytes)]);
            }
            catch (Exception e)
            {
                throw new TransformationException("An exception occured while decrypting content.", e);
            }
        }

        return bytes;
    }
}