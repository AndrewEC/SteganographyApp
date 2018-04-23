using SteganographyApp.Common.Data;
using System;
using System.IO;

namespace SteganographyApp.Common.IO.Content
{
    public abstract class AbstractContentIO : IDisposable
    {

        /// <summary>
        /// The stream used by the underlying implementation to read
        /// or write data to a specified file.
        /// </summary>
        protected FileStream stream;

        /// <summary>
        /// The values parsed from the command line arguments.
        /// </summary>
        protected readonly InputArguments args;

        /// <summary>
        /// Index generator to randomize the order of the binary data being read and wrote
        /// </summary>
        protected readonly IndexGenerator generator;

        public AbstractContentIO(InputArguments args)
        {
            this.args = args;
            if(args.RandomSeed != "")
            {
                generator = IndexGenerator.FromString(args.RandomSeed);
            }
        }

        /// <summary>
        /// Randomizes the bytes being read from the input file
        /// </summary>
        /// <param name="bytes">The original file bytes</param>
        /// <returns>A randomized array of bytes</returns>
        public byte[] RandomizeBytes(byte[] bytes)
        {
            int generations = bytes.Length * 2;
            for (int i = 0; i < generations; i++)
            {
                int first = generator.Next(bytes.Length - 1);
                int second = generator.Next(bytes.Length - 1);
                if(first != second)
                {
                    byte temp = bytes[first];
                    bytes[first] = bytes[second];
                    bytes[second] = temp;
                }
            }
            return bytes;
        }

        /// <summary>
        /// Reverses the effect of the RandomizeBytes method when writing to file
        /// </summary>
        /// <param name="bytes">The randomized bytes read from the input images</param>
        /// <returns>A non-randomized array of bytes matching the original input file.</returns>
        public byte[] ReorderBytes(byte[] bytes)
        {
            int generations = bytes.Length * 2;
            var pairs = new ValueTuple<int, int>[generations];
            for(int i=0; i<generations; i++)
            {
                int first = generator.Next(bytes.Length - 1);
                int second = generator.Next(bytes.Length - 1);
                pairs[i] = (first, second);
            }
            Array.Reverse(pairs);
            foreach((int first, int second) in pairs)
            {
                byte temp = bytes[first];
                bytes[first] = bytes[second];
                bytes[second] = temp;
            }
            return bytes;
        }

        /// <summary>
        /// Flushes the stream if it has been instantiated.
        /// </summary>
        public void Dispose()
        {
            if(stream != null)
            {
                stream.Flush();
                stream.Dispose();
            }
        }

    }
}
