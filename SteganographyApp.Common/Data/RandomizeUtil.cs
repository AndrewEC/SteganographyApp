namespace SteganographyApp.Common.Data
{
    using System;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    public interface IRandomizeUtil
    {
        string RandomizeBinaryString(string binaryString, string randomSeed);

        string ReorderBinaryString(string binaryString, string randomSeed);
    }

    [Injectable(typeof(IRandomizeUtil))]
    public class RandomizeUtil : IRandomizeUtil
    {
        private static readonly int RandomizeIterationsModifier = 7;

        private ILogger log;

        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<RandomizeUtil>();
        }

        /// <summary>
        /// Randomizes the encrypted binary string from the file to encode.
        /// </summary>
        /// <param name="binaryString">The binary string to randomize</param>
        /// <returns>A randomized binary string.</returns>
        public string RandomizeBinaryString(string binaryString, string randomSeed)
        {
            char[] characters = binaryString.ToCharArray();

            var generator = IndexGenerator.FromString(randomSeed);

            int iterations = characters.Length * RandomizeIterationsModifier;

            log.Debug("Randomizing binary string using seed [{0}] over [{1}] iterations", randomSeed, iterations);

            for (int i = 0; i < iterations; i++)
            {
                int first = generator.Next(characters.Length - 1);
                int second = generator.Next(characters.Length - 1);
                if (first != second)
                {
                    char temp = characters[first];
                    characters[first] = characters[second];
                    characters[second] = temp;
                }
            }

            string randomized = new string(characters);
            LogDegreeOfSimilarity(randomized, binaryString);
            return randomized;
        }

        /// <summary>
        /// Reverses the effect of the RandomizeBytes method when writing to file
        /// </summary>
        /// <param name="bytes">The randomized bytes read from the input images</param>
        /// <returns>A non-randomized array of bytes matching the original input file.</returns>
        public string ReorderBinaryString(string binaryString, string randomSeed)
        {
            char[] characters = binaryString.ToCharArray();
            var generator = IndexGenerator.FromString(randomSeed);

            int iterations = characters.Length * RandomizeIterationsModifier;

            log.Debug("Randomizing binary string using seed [{0}] over [{1}] iterations", randomSeed, iterations);

            var pairs = new ValueTuple<int, int>[iterations];
            for (int i = iterations - 1; i >= 0; i--)
            {
                int first = generator.Next(characters.Length - 1);
                int second = generator.Next(characters.Length - 1);
                pairs[i] = (first, second);
            }
            foreach ((int first, int second) in pairs)
            {
                char temp = characters[first];
                characters[first] = characters[second];
                characters[second] = temp;
            }
            return new string(characters);
        }

        private void LogDegreeOfSimilarity(string newString, string originalString)
        {
            log.Debug("After randomizing [{0}] input bits, [{1}] have changed leading to a [{2}]% similarity", () =>
            {
                int characterCount = newString.Length;
                int similar = 0;
                for (int i = 0; i < characterCount; i++)
                {
                    if (newString[i] == originalString[i])
                    {
                        similar++;
                    }
                }
                return new object[]
                {
                    characterCount,
                    characterCount - similar,
                    ((double)similar / (double)characterCount) * 100.0,
                };
            });
        }
    }
}