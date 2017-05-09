using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyAppCommon
{

    /// <summary>
    /// Stream encapsulating class that decodes binary data and writes it
    /// to an output file.
    /// </summary>
    public class ContentWriter : IDisposable
    {

        /// <summary>
        /// A stream responsible for writing decoded bytes to the destination file.
        /// </summary>
        private BinaryWriter destination;

        /// <summary>
        /// The values parsed from the command line arguments.
        /// </summary>
        private readonly InputArguments args;

        /// <summary>
        /// Instantiates a new ContentWrite instance and sets the
        /// args field value.
        /// </summary>
        /// <param name="args"></param>
        public ContentWriter(InputArguments args)
        {
            this.args = args;
        }

        /// <summary>
        /// Takes in an encrypted binary string, decyrypts it using the DataEncoderUtil
        /// and writes the resulting bytes to the output file.
        /// </summary>
        /// <param name="binary">The encrypted binary string read from the storage images.</param>
        public void WriteChunk(string binary)
        {
            if(destination == null)
            {
                if (File.Exists(args.DecodedOutputFile))
                {
                    File.Delete(args.DecodedOutputFile);
                }
                destination = new BinaryWriter(File.Open(args.DecodedOutputFile, FileMode.OpenOrCreate));
            }

            byte[] decoded = DataEncoderUtil.Decode(binary, args.Password, args.UseCompression);
            destination.Write(decoded);
            destination.Flush();
        }

        /// <summary>
        /// Flushes and closes the destination stream if it has been instantiated.
        /// </summary>
        public void Dispose()
        {
            if(destination != null)
            {
                destination.Flush();
                destination.Dispose();
            }
        }
    }
}
