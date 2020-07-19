using System;

namespace SteganographyApp.Common.Data
{

    static class RandomizeUtil
    {

        private static readonly int RandomizeGenerationsModifier = 10;

        /// <summary>
        /// Randomizes the encrypted binary string from the file to encode.
        /// </summary>
        /// <param name="binaryString">The binary string to randomize</param>
        /// <returns>A randomized binary string.</returns>
        public static string RandomizeBinaryString(string binaryString, string randomSeed)
        {
            char[] characters = binaryString.ToCharArray();

            var generator = IndexGenerator.FromString(randomSeed);

            int generations = characters.Length * RandomizeGenerationsModifier;
            for (int i = 0; i < generations; i++)
            {
                int first = generator.Next(characters.Length - 1);
                int second = generator.Next(characters.Length - 1);
                if(first != second)
                {
                    char temp = characters[first];
                    characters[first] = characters[second];
                    characters[second] = temp;
                }
            }
            return new string(characters);
        }

        /// <summary>
        /// Reverses the effect of the RandomizeBytes method when writing to file
        /// </summary>
        /// <param name="bytes">The randomized bytes read from the input images</param>
        /// <returns>A non-randomized array of bytes matching the original input file.</returns>
        public static string ReorderBinaryString(string binaryString, string randomSeed)
        {
            char[] characters = binaryString.ToCharArray();
            var generator = IndexGenerator.FromString(randomSeed);

            int generations = characters.Length * RandomizeGenerationsModifier;
            var pairs = new ValueTuple<int, int>[generations];
            for(int i = generations - 1; i >= 0; i--)
            {
                int first = generator.Next(characters.Length - 1);
                int second = generator.Next(characters.Length - 1);
                pairs[i] = (first, second);
            }
            foreach((int first, int second) in pairs)
            {
                char temp = characters[first];
                characters[first] = characters[second];
                characters[second] = temp;
            }
            return new string(characters);
        }

    }

}