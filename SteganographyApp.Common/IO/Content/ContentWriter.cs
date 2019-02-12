using SteganographyApp.Common.Data;
using System;
using System.IO;

namespace SteganographyApp.Common.IO.Content
{

    /// <summary>
    /// Stream encapsulating class that decodes binary data and writes it
    /// to an output file.
    /// </summary>
    public class ContentWriter : AbstractContentIO
    {

        /// <summary>
        /// Instantiates a new ContentWrite instance and sets the
        /// args field value.
        /// </summary>  
        /// <param name="args"></param>
        public ContentWriter(InputArguments args) : base(args) { }

        /// <summary>
        /// Takes in an encrypted binary string, decyrypts it using the DataEncoderUtil
        /// and writes the resulting bytes to the output file.
        /// </summary>
        /// <param name="binary">The encrypted binary string read from the storage images.</param>
        public void WriteChunk(string binary)
        {
            if(stream == null)
            {
                if (File.Exists(args.DecodedOutputFile))
                {
                    File.Delete(args.DecodedOutputFile);
                }
                stream = File.Open(args.DecodedOutputFile, FileMode.OpenOrCreate);
            }

            byte[] decoded = DataEncoderUtil.Decode(binary, args.Password, args.UseCompression, args.DummyCount);
            if (args.RandomSeed != "")
            {
                decoded = ReorderBytes(decoded);
            }
            stream.Write(decoded, 0, decoded.Length);
            stream.Flush();
        }
    }
}
