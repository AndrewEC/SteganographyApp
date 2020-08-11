using System.IO;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.IO.Content
{

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
        /// <param name="args"></param>
        public ContentWriter(IInputArguments args) : base(args) { }

        /// <summary>
        /// Takes in an encrypted binary string, decyrypts it using the DataEncoderUtil
        /// and writes the resulting bytes to the output file.
        /// </summary>
        /// <param name="binary">The encrypted binary string read from the storage images.</param>
        public void WriteContentChunkToFile(string binary)
        {
            if(stream == null)
            {
                var fileProvider = Injector.Provide<IFileProvider>();
                if (fileProvider.IsExistingFile(args.DecodedOutputFile))
                {
                    fileProvider.Delete(args.DecodedOutputFile);
                }
                stream = fileProvider.OpenFileForWrite(args.DecodedOutputFile);
            }

            byte[] decoded = Injector.Provide<IDataEncoderUtil>().Decode(binary, args.Password, args.UseCompression, args.DummyCount, args.RandomSeed);
            stream.Write(decoded, 0, decoded.Length);
            stream.Flush();
        }
    }
}
