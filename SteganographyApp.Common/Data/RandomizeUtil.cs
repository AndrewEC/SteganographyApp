using System;

namespace SteganographyApp.Common.Data
{

    static class RandomizeUtil
    {

        /// <summary>
        /// Randomizes the bytes being read from the input file
        /// </summary>
        /// <param name="bytes">The original file bytes</param>
        /// <returns>A randomized array of bytes</returns>
        public static byte[] RandomizeBytes(byte[] bytes, string randomSeed)
        {
            var generator = IndexGenerator.FromString(randomSeed);

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
        public static byte[] ReorderBytes(byte[] bytes, string randomSeed)
        {
            var generator = IndexGenerator.FromString(randomSeed);

            int generations = bytes.Length * 2;
            var pairs = new ValueTuple<int, int>[generations];
            for(int i = generations - 1; i >= 0; i--)
            {
                int first = generator.Next(bytes.Length - 1);
                int second = generator.Next(bytes.Length - 1);
                pairs[i] = (first, second);
            }
            foreach((int first, int second) in pairs)
            {
                byte temp = bytes[first];
                bytes[first] = bytes[second];
                bytes[second] = temp;
            }
            return bytes;
        }

    }

}