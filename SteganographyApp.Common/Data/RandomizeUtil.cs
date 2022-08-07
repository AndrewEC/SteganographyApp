namespace SteganographyApp.Common.Data
{
    using System;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// The contract for interacting with the RandomizeUtil instance.
    /// </summary>
    public interface IRandomizeUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/RandomizeBinaryString/*' />
        string RandomizeBinaryString(string binaryString, string randomSeed);

        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/ReorderBinaryString/*' />
        string ReorderBinaryString(string binaryString, string randomSeed);
    }

    /// <summary>
    /// The injectable utility class to randomize and re-order a binary string during the encode
    /// and decode process.
    /// </summary>
    [Injectable(typeof(IRandomizeUtil))]
    public sealed class RandomizeUtil : IRandomizeUtil
    {
        private ILogger log = new LazyLogger<RandomizeUtil>();

        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/RandomizeBinaryString/*' />
        public string RandomizeBinaryString(string binaryString, string randomSeed)
        {
            char[] characters = binaryString.ToCharArray();

            var generator = IndexGenerator.FromString(randomSeed);

            int iterations = characters.Length;

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

        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/ReorderBinaryString/*' />
        public string ReorderBinaryString(string binaryString, string randomSeed)
        {
            char[] characters = binaryString.ToCharArray();
            var generator = IndexGenerator.FromString(randomSeed);

            int iterations = characters.Length;

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