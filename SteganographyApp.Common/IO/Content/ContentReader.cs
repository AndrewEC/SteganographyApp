using System;
using System.IO;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.IO.Content
{

    /// <summary>
    /// Stream encapsulation class that reads and encodes data
    /// from the input file.
    /// </summary>
    public sealed class ContentReader : AbstractContentIO
    {

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
        public ContentReader(IInputArguments args) : base(args)
        {
            RequiredNumberOfReads = (int)Math.Ceiling(new FileInfo(args.FileToEncode).Length / (double)args.ChunkByteSize);
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
            byte[] buffer = new byte[args.ChunkByteSize];
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                return null;
            }
            else if (read < args.ChunkByteSize)
            {
                byte[] actual = new byte[read];
                Array.Copy(buffer, actual, read);
                buffer = actual;
            }

            if (args.RandomSeed != "")
            {
                buffer = RandomizeBytes(buffer);
            }

            return DataEncoderUtil.Encode(buffer, args.Password, args.UseCompression, args.DummyCount);
        }

    }
}
