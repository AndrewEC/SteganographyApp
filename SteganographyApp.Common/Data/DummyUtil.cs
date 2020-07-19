using System;
using System.Linq;

namespace SteganographyApp.Common.Data
{

    public static class DummyUtil
    {

        private static readonly int MAX_LENGTH_PER_DUMMY = 1000;
        private static readonly int MIN_LENGTH_PER_DUMMY = 50;

        /// <summary>
        /// Inserts the specified number of dummy entries into the current
        /// binary string.
        /// </summary>
        /// <param name="numDummies">The number of dummy entries to insert into the
        /// binary string.</param>
        /// <param name="binary">The original binary string to be modified with the
        /// dummy entries.</param>
        /// <returns>Returns the binary string with the new dummy entries.</returns>
        public static string InsertDummies(int numDummies, string binary)
        {
            int[] lengths = GenerateLengthsForDummies(numDummies);

            // cubed root
            int length = (int)Math.Ceiling(Math.Pow(binary.Length, (double)1 / 3)) + 1;
            var generator = new IndexGenerator(length, length);

            // storing the positions instead of calculating on the fly will make decoding easier
            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(binary.Length - 1))
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
        private static int[] GenerateLengthsForDummies(int numDummies)
        {
            var lengthGenerator = new IndexGenerator(numDummies, numDummies);
            return Enumerable.Range(0, numDummies)
                .Select(i => lengthGenerator.Next(MAX_LENGTH_PER_DUMMY - MIN_LENGTH_PER_DUMMY) + MIN_LENGTH_PER_DUMMY)
                .ToArray();
        }

        /// <summary>
        /// Generates a random binary string of the length specified by the length parameter
        /// using the provided IndexGenerator instance.
        /// </summary>
        /// <param name="generator">The index generator instance to determine whether
        /// each character in the dummy string should be a 1 or a 0.</param>
        /// <returns>Returns a random binary string of a length equal to
        private static string GenerateDummy(IndexGenerator generator, int length)
        {
            string dummy = "";
            for(int i = 0; i < length; i++)
            {
                dummy += (generator.Next(2) == 0) ? "0" : "1";
            }
            return dummy;
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
        public static string RemoveDummies(int numDummies, string binary)
        {

            // calculate the length of the dummies originally added to the string
            int[] lengths = GenerateLengthsForDummies(numDummies);
            Array.Reverse(lengths);
            int totalLength = lengths.Sum();

            // determine the length of the binary string before the dummies were added
            int actualLength = binary.Length - totalLength;

            // cubed root
            int length = (int)Math.Ceiling(Math.Pow(actualLength, (double)1 / 3)) + 1;
            var generator = new IndexGenerator(length, length);

            int[] positions = Enumerable.Range(0, numDummies)
                .Select(i => generator.Next(actualLength - 1))
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