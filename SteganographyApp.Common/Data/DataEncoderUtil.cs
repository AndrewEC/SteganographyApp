﻿namespace SteganographyApp.Common.Data;

using System;

using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

/// <summary>
/// The contract for interacting with the DataEncoderUtil instance.
/// </summary>
public interface IDataEncoderUtil
{
    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Encode/*' />
    string Encode(byte[] bytes, string password, bool useCompression, int dummyCount, string randomSeed, int additionalPasswordHashIterations);

    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Decode/*' />
    byte[] Decode(string binary, string password, bool useCompression, int dummyCount, string randomSeed, int additionalPasswordHashIterations);
}

/// <summary>
/// A case class inheriting from Exception that specifies an error occured
/// when an IFileCoder instance attempted to transform input data.
/// </summary>
public class TransformationException : Exception
{
    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
    public TransformationException(string message) : base(message) { }

    /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
    public TransformationException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Utility class to encode a file to encrypted binary data or decode the encrypted binary string to the
/// original file bytes.
/// </summary>
[Injectable(typeof(IDataEncoderUtil))]
public sealed class DataEncoderUtil : IDataEncoderUtil
{
    private const int IterationMultiplier = 5;

    private readonly ILogger logger = new LazyLogger<DataEncoderUtil>();

    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Encode/*' />
    public string Encode(byte[] bytes, string password, bool useCompression, int dummyCount, string randomSeed, int additionalPasswordHashIterations)
    {
        logger.Trace("Before encoding: [{0}]", () => [Convert.ToBase64String(bytes)]);

        if (randomSeed != string.Empty)
        {
            byte[] randomKey = Injector.Provide<IKeyUtil>().GenerateKey(randomSeed, additionalPasswordHashIterations);
            randomSeed = Convert.ToBase64String(randomKey);
        }

        if (password != string.Empty)
        {
            try
            {
                bytes = Injector.Provide<IEncryptionUtil>().Encrypt(bytes, password, additionalPasswordHashIterations);
            }
            catch (Exception e)
            {
                throw new TransformationException("An exception occured while encrypting content.", e);
            }
            logger.Trace("After encryption: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (dummyCount > 0 && randomSeed != string.Empty)
        {
            bytes = Injector.Provide<IDummyUtil>().InsertDummies(dummyCount, bytes, randomSeed);
            logger.Trace("After inserting dummies: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (randomSeed != string.Empty)
        {
            bytes = Injector.Provide<IRandomizeUtil>().Randomize(bytes, randomSeed, IterationMultiplier);
            logger.Trace("After randomizing: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (useCompression)
        {
            bytes = Injector.Provide<ICompressionUtil>().Compress(bytes);
            logger.Trace("After compression: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        return Injector.Provide<IBinaryUtil>().ToBinaryString(bytes);
    }

    /// <include file='../docs.xml' path='docs/members[@name="DataEncoderUtil"]/Decode/*' />
    public byte[] Decode(string binary, string password, bool useCompression, int dummyCount, string randomSeed, int additionalPasswordHashIterations)
    {
        byte[] bytes = Injector.Provide<IBinaryUtil>().ToBytes(binary);

        logger.Trace("Original binary: [{0}]", binary);
        logger.Trace("Before decoding: [{0}]", () => [Convert.ToBase64String(bytes)]);

        if (randomSeed != string.Empty)
        {
            byte[] randomKey = Injector.Provide<IKeyUtil>().GenerateKey(randomSeed, additionalPasswordHashIterations);
            randomSeed = Convert.ToBase64String(randomKey);
        }

        if (useCompression)
        {
            try
            {
                bytes = Injector.Provide<ICompressionUtil>().Decompress(bytes);
                logger.Trace("After decompressing: [{0}]", () => [Convert.ToBase64String(bytes)]);
            }
            catch (Exception e)
            {
                throw new TransformationException("An exception occurred while decompressing content.", e);
            }
        }

        if (randomSeed != string.Empty)
        {
            bytes = Injector.Provide<IRandomizeUtil>().Reorder(bytes, randomSeed, IterationMultiplier);
            logger.Trace("After reordering: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (dummyCount > 0 && randomSeed != string.Empty)
        {
            bytes = Injector.Provide<IDummyUtil>().RemoveDummies(dummyCount, bytes, randomSeed);
            logger.Trace("After removing dummies: [{0}]", () => [Convert.ToBase64String(bytes)]);
        }

        if (password != string.Empty)
        {
            try
            {
                bytes = Injector.Provide<IEncryptionUtil>().Decrypt(bytes, password, additionalPasswordHashIterations);
                logger.Trace("After decrypting: [{0}]", () => [Convert.ToBase64String(bytes)]);
            }
            catch (Exception e)
            {
                throw new TransformationException("An exception occured while decrypting content.", e);
            }
        }

        return bytes;
    }
}