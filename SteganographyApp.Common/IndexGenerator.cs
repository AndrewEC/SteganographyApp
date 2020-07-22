using System;

namespace SteganographyApp.Common.Data
{

    /// <summary>
    /// A reliable number generator that will repeatably generate random sets
    /// of numbers as long as the input seed is the always the same.
    /// </summary>
    public sealed class IndexGenerator
    {

        /// <summary>
        /// The history of x values up to a maximum of 3 values.
        /// Used to calculate xn.
        /// The newly calculated yn will become the first entry
        /// in the history of x values.
        /// </summary>
        private Int32[] x;

        /// <summary>
        /// The history of y values up to a maximum of 3 values.
        /// Used to calculate yn. The newly calculated yn will become
        /// the first entry in the history of y values.
        /// </summary>
        private Int32[] y;

        //Coefficients
        private readonly Int32 a1 = 0;
        private readonly Int32 a2 = 63308;
        private readonly Int32 a3 = -183326;

        private readonly Int32 b1 = 86098;
        private readonly Int32 b2 = 0;
        private readonly Int32 b3 = -539608;

        //Bases
        private readonly Int32 m1 = 2147483647;
        private readonly Int32 m2 = 2145483479;

        /// <summary>
        /// Takes in a seed number and initializes the history of x and y values using the specified seed
        /// and generates a new history of random number based on the number of generations specified.
        /// </summary>
        /// <param name="seed">The value to initialize the x and y history to. If seed is a negative number the
        /// absolute value of the number will be used.</param>
        /// <param name="generations">The number of random number to generate. At least 3 should be used
        /// to clear all initial entries from the x and y history.</param>
        public IndexGenerator(Int32 seed, Int32 generations = 3)
        {
            if (seed < 0)
            {
                seed = Math.Abs(seed);
            }

            x = new Int32[] { seed, seed, seed };
            y = new Int32[] { seed, seed, seed };

            int count = 0;
            while (count < generations)
            {
                Next(Int32.MaxValue);
                count++;
            }
        }

        /// <summary>
        /// Generates a new random number between 0 and max inclusive.
        /// </summary>
        /// <param name="max">The largest values this method can return.</param>
        /// <returns>A 32 bit integer between 0 and the maximum value inclusive.</returns>
        public Int32 Next(Int32 max)
        {
            if (max % 2 == 0)
            {
                max -= 1;
            }

            Int32 xn = (a1 * x[2] + a2 * x[1] + a3 * x[0]) % m1;
            Int32 yn = (b1 * y[2] + b2 * y[1] + b3 * y[0]) % m2;

            Swap(x, xn);
            Swap(y, yn);

            Int32 zn = (xn - yn) % max;
            return Math.Abs(zn - 1);
        }

        /// <summary>
        /// Generates a new random seed from a string.
        /// </summary>
        /// <param name="seed">The seed or hash as a string.</param>
        /// <returns>An IndexGenerator instance with a calculated numeric seed pre-iterated to a
        ///  number of times based on the provided seed string byte size.</returns>
        public static IndexGenerator FromString(string seed)
        {
            Int32 total = 0;
            foreach (char c in seed)
            {
                total += c * c;
            }
            return new IndexGenerator(total, seed.Length);
        }

        /// <summary>
        /// Adds the new value to the beginning of the array and moves all elements to the next element,
        /// losing the last value in the array.
        /// </summary>
        /// <param name="arr">The array of values to modify.</param>
        /// <param name="value">The value to place at the beginning of the array.</param>
        private void Swap(Int32[] arr, Int32 value)
        {
            arr[2] = arr[1];
            arr[1] = arr[0];
            arr[0] = value;
        }
    }
}