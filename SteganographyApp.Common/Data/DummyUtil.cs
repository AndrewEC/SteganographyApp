using System;
using System.Text;
using System.Linq;

namespace SteganographyApp.Common.Data
{

    public interface IDummyUtil
    {
        string InsertDummies(int numDummies, string binary);
        string RemoveDummies(int numDummies, string binary);
    }

    public class DummyUtil : IDummyUtil
    {

        private static readonly int MaxLengthPerDummy = 500;
        private static readonly int MinLengthPerDummy = 100;

        /// <summary>
        /// Inserts the specified number of dummy entries into the current
        /// binary string.
        /// </summary>
        /// <param name="numDummies">The number of dummy entries to insert into the
        /// binary string.</param>
        /// <param name="binary">The original binary string to be modified with the
        /// dummy entries.</param>
        /// <returns>Returns the binary string with the new dummy entries.</returns>
        public string InsertDummies(int numDummies, string binary)
        {
            int[] lengths = GenerateLengthsForDummies(numDummies);

            // cubed root
            int generatorSeed = (int)Math.Ceiling(Math.Pow(binary.Length, (double)1 / 3)) + 1;
            var generator = new IndexGenerator(generatorSeed, generatorSeed);

            // storing the positions instead of calculating on the fly will make decoding easier
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binary.Length))
                .ToArray();

            for (int i = 0; i < positions.Length; i++)
            {
                binary = binary.Insert(positions[i], GenerateDummy(generator, lengths[i]));
            }

            return binary;
        }

        /// <summary>
        /// Generates an array of the specified lengths containing ints consisting
        /// of random values between 1 and 10.
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
            for(int i = 0; i < length; i++)
            {
                int next = generator.Next(10);
                dummy.Append((next % 2 == 0) ? '0' : '1');
            }
            return dummy.ToString();
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
        public string RemoveDummies(int numDummies, string binary)
        {

            // calculate the length of the dummies originally added to the string
            int[] lengths = GenerateLengthsForDummies(numDummies);
            Array.Reverse(lengths);
            int totalLength = lengths.Sum();

            // determine the length of the binary string before the dummies were added
            int binaryLengthWithoutDummies = binary.Length - totalLength;

            // cubed root
            int generatorSeed = (int)Math.Ceiling(Math.Pow(binaryLengthWithoutDummies, (double)1 / 3)) + 1;
            var generator = new IndexGenerator(generatorSeed, generatorSeed);

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
            return binary;
        }
    }

}