namespace SteganographyApp.Common.IO
{
    using System;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

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
        /// than the ChunkByteSize.</para>
        /// </summary>
        /// <returns>A binary string representation of the next availabe ChunkByteSize from the input file.</returns>
        public string ReadContentChunkFromFile()
        {
            byte[] buffer = new byte[Args.ChunkByteSize];
            int read = Stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                return null;
            }
            else if (read < Args.ChunkByteSize)
            {
                byte[] actual = new byte[read];
                Array.Copy(buffer, actual, read);
                buffer = actual;
            }

            return Injector.Provide<IDataEncoderUtil>().Encode(buffer, Args.Password, Args.UseCompression, Args.DummyCount, Args.RandomSeed);
        }

        /// <summary>
        /// Creates a new stream configured to read in the file that is to be encoded.
        /// </summary>
        /// <returns>A stream instance configured for reading from the FileToEncode.</returns>
        protected override IReadWriteStream InitializeStream()
        {
            return Injector.Provide<IFileIOProxy>().OpenFileForRead(Args.FileToEncode);
        }
    }
}
