using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyAppCommon
{
    /// <summary>
    /// Utility class to reliably and repeatably generate the same sets of random number
    /// when the same seed and generation value is provided during initialization.
    /// <para>This generator uses a simplified version of the combined multiple recursive
    /// generator by L’Ecuyer</para>
    /// </summary>
    public class RandomIndexGenerator
    {

        private readonly Int32 a1 = 0;
        private readonly Int32 a2 = 63_308;
        private readonly Int32 a3 = -183_326;

        private readonly Int32 b1 = 86_098;
        private readonly Int32 b2 = 0;
        private readonly Int32 b3 = -539_608;

        private readonly Int32 m1 = 2_147_483_647;
        private readonly Int32 m2 = 2_145_483_479;

        public readonly Int32 Seed;

        private Int32[] x = { 1, 1, 1 };
        private Int32[] y = { 1, 1, 1 };

        /// <summary>
        /// The main constructor. Takes in a seed and a generation to initialize the object.
        /// The seed will be used in the first 3 random number calculations at which point all instances
        /// of the value will have been overwritten by previous calculation values.
        /// </summary>
        /// <param name="seed">The initial value to use when calculating the random numbers until enough previous numbers
        /// have been generated to overwrite this value.</param>
        /// <param name="generations">Specifies the number of generations to generate random numbers for. Recommended
        /// to do at least 3 generations so the original seed value will be overwritten in all future calculations.</param>
        public RandomIndexGenerator(Int32 seed = 1, int generations = 3)
        {
            Seed = seed;

            Set(x, seed);
            Set(y, seed);

            if (generations <= 0)
            {
                return;
            }

            int count = 0;
            while (count < generations)
            {
                Next(seed);
                count++;
            }
        }

        /// <summary>
        /// Generates a numeric seed value from the string seed and passes it into the main constructor.
        /// </summary>
        /// <param name="seed">The hash string value that will be converted into a numeric seed.</param>
        /// <param name="generations">Value will simply be passed into the main constructor.</param>
        public RandomIndexGenerator(string seed = "simple-hash", int generations = 3) : this(SeedFromHash(seed), generations) { }

        /// <summary>
        /// Generates a random number between 0 and max inclusive.
        /// </summary>
        /// <returns>A random number between 0 and max inclusive.</returns>
        public Int32 Next(int max)
        {
            if (max % 2 == 0)
            {
                max++;
            }
            int xn = ((a1 * x[0]) + (a2 * x[1]) + (a3 * x[2])) % m1;
            int yn = ((b1 * y[0]) + (b2 * y[1]) + (b3 * x[2])) % m2;

            Swap(x, xn);
            Swap(y, yn);

            int zn = (xn - yn) % m1;
            return zn = (Math.Abs(zn) - 1) % max;
        }

        /// <summary>
        /// Takes an input array of ints and sets all the values of the array to
        /// to the provided seed value.
        /// </summary>
        ///	<param name="arr">The int array of values to set</param>
        /// <param name="seed">The value to set all of the array elements equal to.</param>
        private void Set(Int32[] arr, Int32 seed)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = seed;
            }
        }

        /// <summary>
        /// Drops the last value of the array pushes the ther values back one
        /// space to the next index position and sets the first element to the
        /// value of next.
        /// </summary>
        /// <param name="arr">The array of values to swap and modify.</param>
        /// <param name="next">The value to replace the first element in the array with.</param>
        private void Swap(Int32[] arr, Int32 next)
        {
            arr[2] = arr[1];
            arr[1] = arr[0];
            arr[0] = next;
        }

        /// <summary>
        /// Generates a random int seed from a string hash value.
        /// <para>This method will eventually intantiate a SimpleRandomGenerator instance
        /// with a seed and generation value arbitrarily calculated from the input hash.</para>
        /// </summary>
        /// <returns>A random int seed based on a string hash value.</returns>
        private static Int32 SeedFromHash(string hash)
        {
            Int32 seed = 0;
            foreach (char c in hash)
            {
                seed += (int)c * (int)c;
            }
            seed /= (hash.Length / 2);
            return new RandomIndexGenerator(seed, hash.Length).Next(seed);
        }
    }
}
