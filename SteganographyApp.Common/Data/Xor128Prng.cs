namespace SteganographyApp.Common.Data;

using System;
using System.Linq;

/// <summary>
/// Based on the existing implementation by David Blackman and Sebastiano Vigna:
/// https://prng.di.unimi.it/xoroshiro128plus.c
/// A predictable random number generator that will repeatably generate the same
/// random sets of numbers as long as the input seed is the always the same.
/// </summary>
public sealed class Xor128Prng
{
    private readonly ulong[] s = new ulong[2];

    /// <summary>
    /// Initializes a random number generator with a starting seed.
    /// </summary>
    /// <param name="seed">The seed to set the initial state of the generator to.</param>
    /// <param name="initialIterations">The number of random numbers to generate
    /// and drop.</param>
    public Xor128Prng(int seed, int initialIterations)
    {
        s[0] = (ulong)seed;
        s[1] = (ulong)seed;
        for (int i = 0; i < initialIterations; i++)
        {
            Next();
        }
    }

    /// <summary>
    /// Creates a new instance of the random number generator.
    /// </summary>
    /// <param name="seed">This will be converted into a int and used to
    /// determine the initial state of the generator.</param>
    /// <returns>A new initialized instance of the random number generator.</returns>
    public static Xor128Prng FromString(string seed)
    {
        int sum = Enumerable.Range(0, seed.Length).Select(i => seed[i] * i).Sum();
        return new Xor128Prng(sum, seed.Length);
    }

    /// <summary>
    /// Generates a pseudo random integer between <see cref="int.MinValue"/>
    /// and <see cref="int.MaxValue"/>.
    /// </summary>
    /// <returns>A pseudo random integer between <see cref="int.MinValue"/>
    /// and <see cref="int.MaxValue"/>.</returns>
    public int Next()
    {
        ulong s0 = s[0];
        ulong s1 = s[1];
        ulong result = s0 + s1;

        s1 ^= s0;
        s[0] = Rotl(s0, 24) ^ s1 ^ (s1 << 16); // a, b
        s[1] = Rotl(s1, 37); // c
        return (int)result;
    }

    /// <summary>
    /// Generates an integer between 0, inclusive, and the input exclusive value.
    /// </summary>
    /// <param name="exclusive">The maximum value the generated integer can reach.</param>
    /// <returns>An integer whose value is between 0 and the exclusive value less one.</returns>
    public int Next(int exclusive) => Math.Abs(Next()) % exclusive;

    private static ulong Rotl(ulong x, int k) => (x << k) | (x >> (64 - k));
}