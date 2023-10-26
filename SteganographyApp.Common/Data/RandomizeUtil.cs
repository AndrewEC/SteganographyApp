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
        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/Randomize/*' />
        byte[] Randomize(byte[] value, string randomSeed, int dummyCount, int iterationMultiplier);

        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/Reorder/*' />
        byte[] Reorder(byte[] value, string randomSeed, int dummyCount, int iterationMultiplier);
    }

    /// <summary>
    /// The injectable utility class to randomize and re-order a binary string during the encode
    /// and decode process.
    /// </summary>
    [Injectable(typeof(IRandomizeUtil))]
    public sealed class RandomizeUtil : IRandomizeUtil
    {
        private const int MaxHashIterations = 622_000;
        private const int MinHashIterations = 422_000;

        private readonly ILogger log = new LazyLogger<RandomizeUtil>();

        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/Randomize/*' />
        public byte[] Randomize(byte[] value, string randomSeed, int dummyCount, int iterationMultiplier)
        {
            string seed = FormRandomSeed(randomSeed, dummyCount);
            var generator = Xor128Prng.FromString(seed);

            int iterations = value.Length * iterationMultiplier;

            log.Debug("Randomizing byte array using seed [{0}] over [{1}] iterations", seed, iterations);

            for (int i = 0; i < iterations; i++)
            {
                int first = generator.Next(value.Length);
                int second = generator.Next(value.Length);
                if (first != second)
                {
                    (value[second], value[first]) = (value[first], value[second]);
                }
            }

            return value;
        }

        /// <include file='../docs.xml' path='docs/members[@name="RandomizeUtil"]/Reorder/*' />
        public byte[] Reorder(byte[] value, string randomSeed, int dummyCount, int iterationMultiplier)
        {
            string seed = FormRandomSeed(randomSeed, dummyCount);
            var generator = Xor128Prng.FromString(seed);

            int iterations = value.Length * iterationMultiplier;

            log.Debug("Reordering byte array using seed [{0}] over [{1}] iterations", seed, iterations);

            var pairs = new ValueTuple<int, int>[iterations];
            for (int i = iterations - 1; i >= 0; i--)
            {
                int first = generator.Next(value.Length);
                int second = generator.Next(value.Length);
                pairs[i] = (first, second);
            }
            foreach ((int first, int second) in pairs)
            {
                (value[second], value[first]) = (value[first], value[second]);
            }
            return value;
        }

        private string FormRandomSeed(string randomSeed, int dummyCount)
        {
            int iterations = (dummyCount == 0) ? MaxHashIterations : (int)(((dummyCount + GlobalCounter.Instance.Count) % (MaxHashIterations - MinHashIterations)) + MinHashIterations);
            var randomKey = Injector.Provide<IEncryptionUtil>().GenerateKey(randomSeed + iterations, iterations);
            return Convert.ToBase64String(randomKey);
        }
    }
}