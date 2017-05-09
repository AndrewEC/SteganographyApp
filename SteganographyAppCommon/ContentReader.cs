using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyAppCommon
{
    public class ContentReader : IDisposable
    {

        private Stream source;
        private readonly InputArguments args;
        public static readonly int ChunkByteSize = 32_768;

        public int RequiredNumberOfReads { get; private set; }

        public ContentReader(InputArguments args)
        {
            this.args = args;
            RequiredNumberOfReads = (int)Math.Ceiling(new FileInfo(args.FileToEncode).Length / (double)ChunkByteSize);
        }

        public string ReadNextChunk()
        {
            if (source == null)
            {
                source = File.OpenRead(args.FileToEncode);
            }
            byte[] buffer = new byte[ChunkByteSize];
            int read = source.Read(buffer, 0, buffer.Length);
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

        public void Dispose()
        {
            if(source != null)
            {
                source.Dispose();
            }
        }

    }
}
