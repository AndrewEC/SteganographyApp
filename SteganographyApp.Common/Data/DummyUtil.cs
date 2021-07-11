namespace SteganographyApp.Common.Data
{
    using System;
    using System.Linq;
    using System.Text;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    public interface IDummyUtil
    {
        string InsertDummies(int numDummies, string binary, string randomSeed);

        string RemoveDummies(int numDummies, string binary, string randomSeed);
    }

    [Injectable(typeof(IDummyUtil))]
    public class DummyUtil : IDummyUtil
    {
        private static readonly int MaxLengthPerDummy = 500;
        private static readonly int MinLengthPerDummy = 100;

        private ILogger log;

        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<DummyUtil>();
        }

        /// <summary>
        /// Inserts the specified number of dummy entries into the current
        /// binary string.
        /// </summary>
        /// <param name="numDummies">The number of dummy entries to insert into the
        /// binary string.</param>
        /// <param name="binary">The original binary string to be modified with the
        /// dummy entries.</param>
        /// <returns>Returns the binary string with the new dummy entries.</returns>
        public string InsertDummies(int numDummies, string binary, string randomSeed)
        {
            log.Debug("Inserting [{0}] dummies using seed [{1}] and global count [{2}]", numDummies, randomSeed, GlobalCounter.Instance.Count);
            log.Debug("Bit count before inserting dummies: [{0}]", binary.Length);
            int amountToIncrement = SumBinaryString(binary);

            int[] lengths = GenerateLengthsForDummies(numDummies);

            var generator = IndexGenerator.FromString(CreateRandomSeed(randomSeed));

            // storing the positions instead of calculating on the fly will make decoding easier
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binary.Length))
                .ToArray();

            for (int i = 0; i < positions.Length; i++)
            {
                binary = binary.Insert(positions[i], GenerateDummy(generator, lengths[i]));
            }

            GlobalCounter.Instance.Increment(amountToIncrement);

            log.Debug("Bit count after inserting dummies: [{0}]", binary.Length);
            return binary;
        }

        /// <summary>
        /// Attempts to remove dummy entries from the string equal to the number
        /// of entries specified in the numDummies parameter.
        /// </summary>
        /// <param name="numDummies">The number of dummy entries to remove from
        /// the binary string.</param>
        /// <param name="binary">The binary string to remove the dummy entries from.</param>
        /// <returns>If numDummies == 0 then it will return the original binary string
        /// otherwise will return the binary string with the dummy entries removed.</returns>
        /// <exception cref="TransformationException">Thrown if an our of range exception is caught
        /// while trying to remove the dummy entries from the chunk.</exception>
        public string RemoveDummies(int numDummies, string binary, string randomSeed)
        {
            log.Debug("Removing [{0}] dummies using seed [{1}] and global count [{2}]", numDummies, randomSeed, GlobalCounter.Instance.Count);
            log.Debug("Bit count before removing dummies: [{0}]", binary.Length);

            // calculate the length of the dummies originally added to the string
            int[] lengths = GenerateLengthsForDummies(numDummies);
            Array.Reverse(lengths);
            int totalLength = lengths.Sum();

            // determine the length of the binary string before the dummies were added
            int binaryLengthWithoutDummies = binary.Length - totalLength;

            var generator = IndexGenerator.FromString(CreateRandomSeed(randomSeed));

            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binaryLengthWithoutDummies))
                .Reverse()
                .ToArray();

            try
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    binary = binary.Remove(positions[i], lengths[i]);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new TransformationException("Unable to remove all dummy entries from chunk.", e);
            }

            GlobalCounter.Instance.Increment(SumBinaryString(binary));

            log.Debug("Bit count after removing dummies: [{0}]", binary.Length);
            return binary;
        }

        private int SumBinaryString(string binary)
        {
            int count = 0;
            foreach (char c in binary.ToCharArray())
            {
                if (c == '1')
                {
                    count++;
                }
            }
            return count;
        }

        private string CreateRandomSeed(string randomSeed)
        {
            return $"{randomSeed}{GlobalCounter.Instance.Count}";
        }

        /// <summary>
        /// Generates an array of the length of each dummy entry to be inserted. This
        /// doesn't make use of the value stored in the global counter as the value
        /// of the counter can not be determined when decoding the
        /// contents of the file later.
        /// </summary>
        /// <param name="numDummies">The number of random numbers to generate. Will also
        /// determine the length of the array being returned.</param>
        /// <returns>A new array of random numbers of the specified length.</returns>
        private int[] GenerateLengthsForDummies(int numDummies)
        {
            var lengthGenerator = new IndexGenerator(numDummies, numDummies);
            return Enumerable.Range(0, numDummies)
                .Select(i => lengthGenerator.Next(MaxLengthPerDummy - MinLengthPerDummy) + MinLengthPerDummy)
                .ToArray();
        }

        /// <summary>
        /// Generates a random binary string of the length specified by the length parameter
        /// using the provided IndexGenerator instance.
        /// </summary>
        /// <param name="generator">The index generator instance to determine whether
        /// each character in the dummy string should be a 1 or a 0.</param>
        /// <returns>Returns a random binary string of a length equal to
        private string GenerateDummy(IndexGenerator generator, int length)
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