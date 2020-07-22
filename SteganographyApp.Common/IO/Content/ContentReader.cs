using System;
using System.IO;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Providers;

namespace SteganographyApp.Common.IO.Content
{

    /// <summary>
    /// Stream encapsulation class that reads and encodes data
    /// from the input file.
    /// </summary>
    public sealed class ContentReader : AbstractContentIO
    {

        /// <summary>
        /// Creates a new instance of the ContentReader.
        /// </summary>
        /// <param name="args">The values parsed from the command line values.</param>
        public ContentReader(IInputArguments args) : base(args) { }

        /// <summary>
        /// Reads in the next unread chunk of data from the input file, encodes it,
        /// and returns the encoded value.
        /// <para>The byte array to encode will be trimmed if the number of bytes remaining in the file is less
        /// than the ChunkByteSize</para>
        /// </summary>
        /// <returns>A binary string representation of the next availabe ChunkByteSize from the input file.</returns>
        public string ReadContentChunkFromFile()
        {
            if (stream == null)
            {
                stream = Injector.Provide<IFileProvider>().OpenFileForRead(args.FileToEncode);
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

            return Injector.Provide<IDataEncoderUtil>().Encode(buffer, args.Password, args.UseCompression, args.DummyCount, args.RandomSeed);
        }

    }
}
