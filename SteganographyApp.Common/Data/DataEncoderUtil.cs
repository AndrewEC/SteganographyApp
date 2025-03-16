﻿namespace SteganographyApp.Common.Data;

using System;

using SteganographyApp.Common.Logging;

#pragma warning disable SA1402

/// <summary>
/// The contract for interacting with the DataEncoderUtil instance.
/// </summary>
public interface IDataEncoderUtil
{
    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Encode/*' />
    string Encode(byte[] bytes, string password, bool useCompression, int dummyCount, string randomSeed, int additionalHashIterations);

    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Decode/*' />
    byte[] Decode(string binary, string password, bool useCompression, int dummyCount, string randomSeed, int additionalHashIterations);
}

/// <summary>
/// Utility class to encode a file to encrypted binary data or decode the encrypted binary string to the
/// original file bytes.
/// </summary>
public sealed class DataEncoderUtil : IDataEncoderUtil
{
    private readonly ILogger log = new LazyLogger<DataEncoderUtil>();

    private readonly IKeyUtil keyUtil;
    private readonly IEncryptionUtil encryptionUtil;
    private readonly IDummyUtil dummyUtil;
    private readonly IRandomizeUtil randomizeUtil;
    private readonly ICompressionUtil compressionUtil;
    private readonly IBinaryUtil binaryUtil;

#pragma warning disable CS1591, SA1600
    public DataEncoderUtil(
        IKeyUtil keyUtil,
        IEncryptionUtil encryptionUtil,
        IDummyUtil dummyUtil,
        IRandomizeUtil randomizeUtil,
        ICompressionUtil compressionUtil,
        IBinaryUtil binaryUtil)
    {
        this.keyUtil = keyUtil;
        this.encryptionUtil = encryptionUtil;
        this.dummyUtil = dummyUtil;
        this.randomizeUtil = randomizeUtil;
        this.compressionUtil = compressionUtil;
        this.binaryUtil = binaryUtil;
    }
#pragma warning restore CS1591, SA1600

    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Encode/*' />
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

    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Decode/*' />
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