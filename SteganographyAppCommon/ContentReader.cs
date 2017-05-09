using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyAppCommon
{

    /// <summary>
    /// Stream encapsulation class that reads and encodes data
    /// from the input file.
    /// </summary>
    public class ContentReader : AbstractContentIO
    {

        /// <summary>
        /// Specifies the chunk size. I.e. the number of bytes to read, encode,
        /// and write at any given time.
        /// <para>Value of 131,072</para>
        /// </summary>
        public static readonly int ChunkByteSize = 131_072;

        /// <summary>
        /// Specifies the number of iterations it will take to read the file based on the size
        /// of the file and the size of the file.
        /// <para>Value is readonly.</para>
        /// </summary>
        public int RequiredNumberOfReads { get; private set; }

        /// <summary>
        /// Creates a new instance of the ContentReader and calculates the
        /// RequiredNumberOfReads property value.
        /// </summary>
        /// <param name="args">The values parsed from the command line values.</param>
        public ContentReader(InputArguments args) : base(args)
        {
            RequiredNumberOfReads = (int)Math.Ceiling(new FileInfo(args.FileToEncode).Length / (double)ChunkByteSize);
        }

        /// <summary>
        /// Reads in the next unread chunk of data from the input file, encodes it,
        /// and returns the encoded value.
        /// <para>The byte array to encode will be trimmed if the number of bytes remaining in the file is less
        /// than the ChunkByteSize</para>
        /// </summary>
        /// <returns>A binary string representation of the next availabe ChunkByteSize from the input file.</returns>
        public string ReadNextChunk()
        {
            if (stream == null)
            {
                stream = File.OpenRead(args.FileToEncode);
            }
            byte[] buffer = new byte[ChunkByteSize];
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                return null;
            }
            else if (read < ChunkByteSize)
            {
                byte[] actual = new byte[read];
                Array.Copy(buffer, actual, read);
                buffer = actual;
            }
            string data = DataEncoderUtil.Encode(buffer, args.Password, args.UseCompression);
            return data;
        }

    }
}
