namespace SteganographyApp.Common.IO
{
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Stream encapsulating class that decodes binary data and writes it
    /// to an output file.
    /// </summary>
    public sealed class ContentWriter : AbstractContentIO
    {
        /// <summary>
        /// Instantiates a new ContentWrite instance and sets the
        /// args field value.
        /// </summary>
        /// <param name="args">The user provided input arguments.</param>
        public ContentWriter(IInputArguments args) : base(args)
        {
            var fileIOProxy = Injector.Provide<IFileIOProxy>();
            if (fileIOProxy.IsExistingFile(Args.DecodedOutputFile))
            {
                fileIOProxy.Delete(Args.DecodedOutputFile);
            }
            Stream = fileIOProxy.OpenFileForWrite(Args.DecodedOutputFile);
        }

        /// <summary>
        /// Takes in an encrypted binary string, decyrypts it using the DataEncoderUtil
        /// and writes the resulting bytes to the output file.
        /// </summary>
        /// <param name="binary">The encrypted binary string read from the storage images.</param>
        public void WriteContentChunkToFile(string binary)
        {
            byte[] decoded = Injector.Provide<IDataEncoderUtil>().Decode(binary, Args.Password, Args.UseCompression, Args.DummyCount, Args.RandomSeed);
            Stream.Write(decoded, 0, decoded.Length);
            Stream.Flush();
        }
    }
}
