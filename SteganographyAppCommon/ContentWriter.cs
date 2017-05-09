using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyAppCommon
{
    public class ContentWriter : IDisposable
    {

        private BinaryWriter destination;
        private InputArguments args;

        public ContentWriter(InputArguments args)
        {
            this.args = args;
        }

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
