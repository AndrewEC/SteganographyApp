namespace SteganographyApp.Common.Data;

using System;
using SteganographyApp.Common.Logging;

/// <summary>
/// The contract for interacting with the RandomizeUtil instance.
/// </summary>
public interface IRandomizeUtil
{
    /// <summary>
    /// Randomizes the already encrypted array of bytes.
    /// </summary>
    /// <param name="value">The bytes to be randomized.</param>
    /// <param name="randomSeed">The user provided random seed that will be used to initialize the random number generator.</param>
    /// <returns>A randomized binary string.</returns>
    byte[] Randomize(byte[] value, string randomSeed);

    /// <summary>
    /// Reverses the effect of the Randomize method when writing to file.
    /// </summary>
    /// <param name="value">The encrypted and randomized bytes to be re-ordered.</param>
    /// <param name="randomSeed">The user provided randoom seed that will be used to initialize the random number generator.</param>
    /// <returns>A non-randomized array of bytes matching the original input file.</returns>
    byte[] Reorder(byte[] value, string randomSeed);
}

/// <summary>
/// The injectable utility class to randomize and re-order a binary string during the encode
/// and decode process.
/// </summary>
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