using Rijndael256;
using System;

namespace SteganographyApp.Common.Data
{
    /// <summary>
    /// A case class inheriting from Exception that specifies an error occured
    /// when an IFileCoder instance attempted to transform input data.
    /// </summary>
    public class TransformationException : Exception
    {
        public TransformationException(string message) : base(message) { }
        public TransformationException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Utility class to encode a file to encrypted binary data or decode the encrypted binary string to the
    /// original file bytes.
    /// </summary>
    public static class DataEncoderUtil
    {   

        /// <summary>
        /// Takes in a raw byte array, compresses, encodes base64, encrypts, and then
        /// returns as a binary string.
        /// </summary>
        /// <param name="bytes">The array of bytes to be encoded as a binary string.</param>
        /// <param name="password">The password used to encrypt the contents of the file.
        /// <param name="useCompression">Tells the encoder whether or not to compress the input
        /// byte array.</param>
        /// If an empty string is provided then no encryption will be performed.</param>
        /// <returns>An binary string made up of the base64 bytes read from the file and
        /// possibly passed through an AES cipher.</returns>
        /// <exception cref="TransformationException">Thrown
        /// if there was an issue trying to pass the base64 encoded string through the AES
        /// cipher.</exception>
        public static string Encode(byte[] bytes, string password, bool useCompression, int dummyCount, string randomSeed)
        {
            if (useCompression)
            {
                bytes = CompressionUtil.Compress(bytes);
            }

            if (randomSeed != "")
            {
                bytes = RandomizeUtil.RandomizeBytes(bytes, randomSeed);
            }

            string base64 = Convert.ToBase64String(bytes);

            if (password != "")
            {
                try
                {
                    base64 = Rijndael.Encrypt(base64, password, KeySize.Aes256);
                }
                catch (Exception e)
                {
                    throw new TransformationException("An exception occured while encrypting content.", e);
                }
            }

            string binary = BinaryUtil.ToBinaryString(base64);

            if(dummyCount == 0)
            {
                return binary;
            }
            else
            {
                return DummyUtil.InsertDummies(dummyCount, binary);
            }
        }

        /// <summary>
        /// Takes an encrypted binary string and returns a byte array that is the original bytes
        /// that made up the original input file.
        /// </summary>
        /// <param name="binary">The encrypted binary string.</param>
        /// <param name="password">The password used to decrypt the base64 string. If no password is provided
        /// then no decryption will be done to the string.</param>
        /// <param name="useCompression">Tells the encoder whether or not to uncompress the encoded
        /// binary string.</param>
        /// <returns>A byte array containing the original decoded bytes of the file inputted during
        /// encoding.</returns>
        /// <exception cref="TransformationException">Thrown if an error
        /// occured while decrypting the base64 string or when decompressing the byte stream.</exception>
        public static byte[] Decode(string binary, string password, bool useCompression, int dummyCount, string randomSeed)
        {
            if(dummyCount > 0)
            {
                binary = DummyUtil.RemoveDummies(dummyCount, binary);
            }

            var decoded64String = BinaryUtil.ToBase64String(binary);
            if (password != "")
            {
                try
                {
                    decoded64String = Rijndael.Decrypt(decoded64String, password, KeySize.Aes256);
                }
                catch (Exception e)
                {
                    throw new TransformationException("An exception occured while decrypting content.", e);
                }
            }

            byte[] decoded = Convert.FromBase64String(decoded64String);

            if (randomSeed != "")
            {
                decoded = RandomizeUtil.ReorderBytes(decoded, randomSeed);
                
            }

            if (useCompression)
            {
                try
                {
                    return CompressionUtil.Decompress(decoded);
                }
                catch (Exception e)
                {
                    throw new TransformationException("An exception occurred while decompressing content.", e);
                }
            }
            else
            {
                return decoded;
            }
        }

    }
}
