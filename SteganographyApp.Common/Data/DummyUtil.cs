namespace SteganographyApp.Common.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// The contract for interacting with the DummyUtil instance.
    /// </summary>
    public interface IDummyUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/InsertDummies/*' />
        byte[] InsertDummies(int numDummies, byte[] value, string randomSeed);

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/RemoveDummies/*' />
        byte[] RemoveDummies(int numDummies, byte[] value, string randomSeed);
    }

    /// <summary>
    /// Utility class for inserting and removing dummy entries in the original byte array.
    /// </summary>
    [Injectable(typeof(IDummyUtil))]
    public sealed class DummyUtil : IDummyUtil
    {
        private const int MaxLengthPerDummy = 100;
        private const int MinLengthPerDummy = 10;
        private const int HashIterationLimit = 1000;

        private ILogger log = new LazyLogger<DummyUtil>();

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/InsertDummies/*' />
        public byte[] InsertDummies(int numDummies, byte[] value, string randomSeed)
        {
            string seed = CreateRandomSeed(randomSeed);

            log.Debug("Inserting [{0}] dummies using seed [{1}]", numDummies, seed);
            log.Debug("Byte count before inserting dummies: [{0}]", value.Length);

            var generator = IndexGenerator.FromString(seed);

            // generate an array in which each element represents the length that an inserted dummy entry will have.
            int[] lengths = GenerateLengthsOfDummies(randomSeed, numDummies, generator);

            // generate an array in which each element represents the index the dummy entry will be inserted at.
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(value.Length))
                .ToArray();

            var endValue = new List<byte>(value.Length + lengths.Sum());
            endValue.InsertRange(0, value);
            for (int i = 0; i < positions.Length; i++)
            {
                var nextDummy = GenerateDummyBytes(generator, lengths[i]);
                var nextDummyPosition = positions[i];
                log.Trace("Inserting dummy at position [{0}] with with length [{1}]", nextDummyPosition, nextDummy.Length);
                endValue.InsertRange(positions[i], nextDummy);
            }

            var result = endValue.ToArray();
            log.Debug("Byte count after inserting dummies: [{0}]", result.Length);
            return result;
        }

        /// <include file='../docs.xml' path='docs/members[@name="DummyUtil"]/RemoveDummies/*' />
        public byte[] RemoveDummies(int numDummies, byte[] value, string randomSeed)
        {
            string seed = CreateRandomSeed(randomSeed);

            log.Debug("Removing [{0}] dummies using seed [{1}]", numDummies, seed);
            log.Debug("Byte count before removing dummies: [{0}]", value.Length);

            var generator = IndexGenerator.FromString(seed);

            // calculate the length of the dummies originally added to the string
            int[] lengths = GenerateLengthsOfDummies(randomSeed, numDummies, generator);
            Array.Reverse(lengths);
            int totalLength = lengths.Sum();

            // Calculate the length of the byte array before the dummies were added so we can
            // determine the original position where the dummy entries were inserted.
            int lengthWithoutDummies = value.Length - totalLength;

            // generate the positions in which the dummy entries were inserted into the original string
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(lengthWithoutDummies))
                .Reverse()
                .ToArray();

            byte[] result;
            try
            {
                var valueList = new List<byte>(value);
                for (int i = 0; i < positions.Length; i++)
                {
                    valueList.RemoveRange(positions[i], lengths[i]);
                }
                result = valueList.ToArray();
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new TransformationException("Unable to remove all dummy entries from chunk.", e);
            }

            log.Debug("Bit count after removing dummies: [{0}]", result.Length);
            return result;
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

        private byte[] GenerateDummyBytes(IndexGenerator generator, int length) => Enumerable.Range(0, length)
            .Select(i => (byte)generator.Next(byte.MaxValue))
            .ToArray();
    }
}