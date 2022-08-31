namespace SteganographyApp.Common.Data
{
    using System;
    using System.Linq;
    using System.Text;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// The contract for interacting with the DummyUtil instance.
    /// </summary>
    public interface IDummyUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/InsertDummies/*' />
        string InsertDummies(int numDummies, string binary, string randomSeed);

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/RemoveDummies/*' />
        string RemoveDummies(int numDummies, string binary, string randomSeed);
    }

    /// <summary>
    /// Utility class for inserting and removing dummy entries in a binary string.
    /// </summary>
    [Injectable(typeof(IDummyUtil))]
    public sealed class DummyUtil : IDummyUtil
    {
        private const int MaxLengthPerDummy = 500;
        private const int MinLengthPerDummy = 100;
        private const int HashIterationLimit = 1000;

        private ILogger log = new LazyLogger<DummyUtil>();

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/InsertDummies/*' />
        public string InsertDummies(int numDummies, string binary, string randomSeed)
        {
            string seed = CreateRandomSeed(randomSeed);

            log.Debug("Inserting [{0}] dummies using seed [{1}]", numDummies, seed);
            log.Debug("Bit count before inserting dummies: [{0}]", binary.Length);

            var generator = IndexGenerator.FromString(seed);

            // generate an array in which each element represents the length that an inserted dummy entry will have.
            int[] lengths = GenerateLengthsOfDummies(randomSeed, numDummies, generator);

            // generate an array in which each element represents the index the dummy entry will be inserted at.
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binary.Length))
                .ToArray();

            var builder = new StringBuilder(binary, binary.Length + lengths.Sum());
            for (int i = 0; i < positions.Length; i++)
            {
                var nextDummy = GenerateDummyEntry(generator, lengths[i]);
                var nextDummyPosition = positions[i];
                log.Trace("Inserting dummy at position [{0}] with value [{1}]", nextDummyPosition, nextDummy);
                builder.Insert(positions[i], nextDummy);
            }
            binary = builder.ToString();

            log.Debug("Bit count after inserting dummies: [{0}]", binary.Length);
            return binary;
        }

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/RemoveDummies/*' />
        public string RemoveDummies(int numDummies, string binary, string randomSeed)
        {
            string seed = CreateRandomSeed(randomSeed);

            log.Debug("Removing [{0}] dummies using seed [{1}]", numDummies, seed);
            log.Debug("Bit count before removing dummies: [{0}]", binary.Length);

            var generator = IndexGenerator.FromString(seed);

            // calculate the length of the dummies originally added to the string
            int[] lengths = GenerateLengthsOfDummies(randomSeed, numDummies, generator);
            Array.Reverse(lengths);
            int totalLength = lengths.Sum();

            // Calculate the length of the binary string before the dummies were added so we can
            // determine the original position where the dummy entries were inserted.
            int binaryLengthWithoutDummies = binary.Length - totalLength;

            // generate the positions in which the dummy entries were inserted into the original string
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binaryLengthWithoutDummies))
                .Reverse()
                .ToArray();

            try
            {
                var builder = new StringBuilder(binary);
                for (int i = 0; i < positions.Length; i++)
                {
                    builder.Remove(positions[i], lengths[i]);
                }
                binary = builder.ToString();
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new TransformationException("Unable to remove all dummy entries from chunk.", e);
            }

            log.Debug("Bit count after removing dummies: [{0}]", binary.Length);
            return binary;
        }

        private string CreateRandomSeed(string randomSeed)
        {
            int iterations = (int)Math.Max(1, GlobalCounter.Instance.Count % HashIterationLimit);
            var randomKey = Injector.Provide<IEncryptionUtil>().GenerateKey(randomSeed + iterations, iterations);
            return Convert.ToBase64String(randomKey);
        }

        private int[] GenerateLengthsOfDummies(string randomSeed, int numDummies, IndexGenerator generator) => Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(MaxLengthPerDummy - MinLengthPerDummy) + MinLengthPerDummy)
                .ToArray();

        private string GenerateDummyEntry(IndexGenerator generator, int length)
        {
            var dummy = new StringBuilder(length, length);
            for (int i = 0; i < length; i++)
            {
                int next = generator.Next(10);
                dummy.Append((next % 2 == 0) ? '0' : '1');
            }
            return dummy.ToString();
        }
    }
}