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
    public class DummyUtil : IDummyUtil
    {
        private static readonly int MaxLengthPerDummy = 500;
        private static readonly int MinLengthPerDummy = 100;

        private ILogger log;

        /// <summary>
        /// Post construct method for initializing the logger.
        /// </summary>
        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<DummyUtil>();
        }

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/InsertDummies/*' />
        public string InsertDummies(int numDummies, string binary, string randomSeed)
        {
            log.Debug("Inserting [{0}] dummies using seed [{1}] and global count [{2}]", numDummies, randomSeed, GlobalCounter.Instance.Count);
            log.Debug("Bit count before inserting dummies: [{0}]", binary.Length);
            int amountToIncrement = SumBinaryString(binary);

            int[] lengths = GenerateLengthsOfDummies(numDummies);

            var generator = IndexGenerator.FromString(CreateRandomSeed(randomSeed));

            // we need to pre-calculate the positions that these dummy entries will be inserted at in order
            // to make decoding possible.
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binary.Length))
                .ToArray();

            for (int i = 0; i < positions.Length; i++)
            {
                binary = binary.Insert(positions[i], GenerateDummyEntry(generator, lengths[i]));
            }

            GlobalCounter.Instance.Increment(amountToIncrement);

            log.Debug("Bit count after inserting dummies: [{0}]", binary.Length);
            return binary;
        }

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/RemoveDummies/*' />
        public string RemoveDummies(int numDummies, string binary, string randomSeed)
        {
            log.Debug("Removing [{0}] dummies using seed [{1}] and global count [{2}]", numDummies, randomSeed, GlobalCounter.Instance.Count);
            log.Debug("Bit count before removing dummies: [{0}]", binary.Length);

            // calculate the length of the dummies originally added to the string
            int[] lengths = GenerateLengthsOfDummies(numDummies);
            Array.Reverse(lengths);
            int totalLength = lengths.Sum();

            // Calculate the length of the binary string before the dummies were added so we can
            // determine the original position where the dummy entries were inserted.
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
            return binary.ToCharArray().Where(c => c == '1').Count();
        }

        private string CreateRandomSeed(string randomSeed)
        {
            return $"{randomSeed}{GlobalCounter.Instance.Count}";
        }

        private int[] GenerateLengthsOfDummies(int numDummies)
        {
            var lengthGenerator = new IndexGenerator(numDummies, numDummies);
            return Enumerable.Range(0, numDummies)
                .Select(i => lengthGenerator.Next(MaxLengthPerDummy - MinLengthPerDummy) + MinLengthPerDummy)
                .ToArray();
        }

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