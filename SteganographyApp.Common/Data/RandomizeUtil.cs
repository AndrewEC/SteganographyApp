namespace SteganographyApp.Common.Data;

using System;
using SteganographyApp.Common.Logging;

/// <summary>
/// Provides functionality to randomize and reorder a byte array based on an
/// input seed.
/// </summary>
public interface IRandomizeUtil
{
    /// <summary>
    /// Randomizes, in place, the elements of the input value array using a seeded
    /// pseudo-random number generator. This will produce consistent results across
    /// multiple instances of this application.
    /// </summary>
    /// <param name="value">The bytes to be randomized.</param>
    /// <param name="randomSeed">The seed to initialize the pseudo-random number generator.</param>
    /// <returns>A randomize byte array.</returns>
    byte[] Randomize(byte[] value, string randomSeed);

    /// <summary>
    /// Reverses the effect of the <see cref="Randomize(byte[], string)"/> method.
    /// </summary>
    /// <param name="value">The bytes to be reordered.</param>
    /// <param name="randomSeed">Used to initialize the pseudo-random number generator.
    /// In order for this method to proper reorder the byte array this seed must
    /// match the seed used in the initial <see cref="Randomize(byte[], string)"/> call.</param>
    /// <returns>A reordered array of bytes.</returns>
    byte[] Reorder(byte[] value, string randomSeed);
}

/// <inheritdoc/>
public sealed class RandomizeUtil : IRandomizeUtil
{
    private const int IterationMultiplier = 5;
    private readonly ILogger log = new LazyLogger<RandomizeUtil>();

    /// <inheritdoc/>
    public byte[] Randomize(byte[] value, string randomSeed)
    {
        var generator = Xor128Prng.FromString(randomSeed);

        int iterations = value.Length * IterationMultiplier;

        log.Debug("Randomizing byte array using seed [{0}] over [{1}] iterations", randomSeed, iterations);

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

    /// <inheritdoc/>
    public byte[] Reorder(byte[] value, string randomSeed)
    {
        var generator = Xor128Prng.FromString(randomSeed);

        int iterations = value.Length * IterationMultiplier;

        log.Debug("Reordering byte array using seed [{0}] over [{1}] iterations", randomSeed, iterations);

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
}