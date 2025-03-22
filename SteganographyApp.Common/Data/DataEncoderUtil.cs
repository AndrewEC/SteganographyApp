namespace SteganographyApp.Common.Data;

using System;

using SteganographyApp.Common.Logging;

#pragma warning disable SA1402

/// <summary>
/// The contract for interacting with the DataEncoderUtil instance.
/// </summary>
public interface IDataEncoderUtil
{
    /// <summary>
    /// Takes in a raw byte array, compresses, encodes base64, encrypts, and then
    /// returns as a binary string.
    /// </summary>
    /// <param name="bytes">The array of bytes to be encoded as a binary string.</param>
    /// <param name="password">The password used to encrypt the contents of the file.</param>
    /// <param name="useCompression">Tells the encoder whether or not to compress the input byte array.
    /// If an empty string is provided then no encryption will be performed.</param>
    /// <param name="dummyCount">The number of dummies to insert into the binary string being encoded. No dummies will be
    /// inserted if the dummy count is 0.</param>
    /// <param name="randomSeed">The random seed used to randomize the binary string being encoded. If the random seed is blank
    /// or null then no randmization will be done.</param>
    /// <param name="additionalHashIterations">
    /// The additional number of times the passwordor random seed should be hashed before being encrypting or
    /// decrypting content. Has no effect if neither of these values have been provided.
    /// </param>
    /// <returns>An binary string made up of the base64 bytes read from the file and possibly passed through an AES cipher.</returns>
    /// <exception cref="TransformationException">Thrown if there was an issue trying to pass the base64 encoded string through the AES cipher.</exception>
    string Encode(byte[] bytes, string password, bool useCompression, int dummyCount, string randomSeed, int additionalHashIterations);

    /// <summary>
    /// Takes an encrypted binary string and returns a byte array that is the original bytes
    /// that made up the original input file.
    /// </summary>
    /// <param name="binary">The encrypted binary string.</param>
    /// <param name="password">The password used to decrypt the base64 string. If no password is provided then no decryption will be done to the string.</param>
    /// <param name="useCompression">Tells the encoder whether or not to uncompress the encoded binary string.</param>
    /// <param name="dummyCount">The number of dummies to remove from the binary string being decoded. If the dummy count is zero then no dummies will
    /// be removed.</param>
    /// <param name="randomSeed">The random seed to use in de-randomizing the binary string being decoded. If the random seed is null or blank then
    /// no randomization will be done.</param>
    /// <param name="additionalHashIterations">The additional number of times the passwordor random seed should be hashed before being encrypting or
    /// decrypting content. Has no effect if neither of these values have been provided.</param>
    /// <returns>A byte array containing the original decoded bytes of the file inputted during encoding.</returns>
    /// <exception cref="TransformationException">Thrown if an error occured while decrypting the base64 string or when decompressing the byte stream.</exception>
    byte[] Decode(string binary, string password, bool useCompression, int dummyCount, string randomSeed, int additionalHashIterations);
}

/// <summary>
/// Utility class to encode a file to encrypted binary data or decode the encrypted binary string to the
/// original file bytes.
/// </summary>
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

#pragma warning restore SA1402