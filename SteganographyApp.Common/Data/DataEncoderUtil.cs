using Rijndael256;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

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
        public static string Encode(byte[] bytes, string password, bool useCompression, int dummyCount)
        {
            if (useCompression)
            {
                bytes = Compress(bytes);
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

            byte[] converted = Convert.FromBase64String(base64);

            var builder = new StringBuilder();
            foreach (byte bit in converted)
            {
                builder.Append(Convert.ToString(bit, 2).PadLeft(8, '0'));
            }

            if(dummyCount == 0)
            {
                return builder.ToString();
            }
            else
            {
                return DummyUtil.InsertDummies(dummyCount, builder.ToString());
            }
        }

        /// <summary>
        /// Takes an encrypted binary string and returns a byte array that is the original bytes
        /// that made up the original input file.
        /// </summary>
        /// <param name="input">The encrypted binary string.</param>
        /// <param name="password">The password used to decrypt the base64 string. If no password is provided
        /// then no decryption will be done to the string.</param>
        /// <param name="useCompression">Tells the encoder whether or not to uncompress the encoded
        /// binary string.</param>
        /// <returns>A byte array containing the original decoded bytes of the file inputted during
        /// encoding.</returns>
        /// <exception cref="TransformationException">Thrown if an error
        /// occured while decrypting the base64 string.</exception>
        public static byte[] Decode(string input, string password, bool useCompression, int dummyCount)
        {
            if(dummyCount > 0)
            {
                input = DummyUtil.RemoveDummies(dummyCount, input);
            }

            byte[] bits = new byte[input.Length / 8];
            for (int i = 0; i < bits.Length; i++)
            {
                var rawValue = input.Substring(i * 8, 8);
                bits[i] = Convert.ToByte(rawValue, 2);
            }

            var decoded64String = Convert.ToBase64String(bits);
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
            if (useCompression)
            {
                return Decompress(decoded);
            }
            else
            {
                return decoded;
            }
        }

        /// <summary>
        /// Copies all the data from the source stream into the destination stream
        /// </summary>
        /// <param name="src">The source stream where the data is coming from.</param>
        /// <param name="dest">The destination stream the data is being written to.</param>
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[2048];
            int read = 0;
            while ((read = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, read);
            }
        }

        /// <summary>
        /// Compresses the raw file bytes using standard gzip compression.
        /// </summary>
        /// <param name="fileBytes">The array of bytes read from the input file.</param>
        /// <returns>The gzip compressed array of bytes.</returns>
        private static byte[] Compress(byte[] fileBytes)
        {
            using (var msi = new MemoryStream(fileBytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the bytes read and decoded from the cover image(s) using standard
        /// gzip compression.
        /// </summary>
        /// <param name="readBytes">The array of bytes read and decoded from the
        /// cover images.</param>
        /// <returns>A byte array after being decompressed using standard gzip compression.</returns>
        private static byte[] Decompress(byte[] readBytes)
        {
            using (var msi = new MemoryStream(readBytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }
    }
}
