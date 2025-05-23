namespace SteganographyApp.Common.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SteganographyApp.Common.Logging;

/// <summary>
/// The contract for interacting with the DummyUtil instance.
/// </summary>
public interface IDummyUtil
{
    /// <summary>
    /// Inserts the specified number of dummy entries into the current byte array.
    /// </summary>
    /// <param name="numDummies">The number of dummy entries to insert into the byte array.</param>
    /// <param name="value">The original byte array to be modified with the dummy entries.</param>
    /// <param name="randomSeed">A random seed used to seed the random number generator used when generating the dummy entries.</param>
    /// <returns>Returns a new byte array with the inserted dummy values.</returns>
    byte[] InsertDummies(int numDummies, byte[] value, string randomSeed);

    /// <summary>
    /// Attempts to remove dummy entries from the byte array equal to the number of entries specified in the numDummies parameter.
    /// </summary>
    /// <param name="numDummies">The number of dummy entries to remove from the byte array.</param>
    /// <param name="value">The byte array to remove the dummy entries from.</param>
    /// <param name="randomSeed">A random seed used to seed the random number generator used when generating the dummy entries.</param>
    /// <returns>If numDummies == 0 then it will return the original byte array otherwise will return the byte array with
    /// the dummy entries removed.</returns>
    /// <exception cref="TransformationException">Thrown if an out of range exception is caught while trying to remove the dummy entries from the chunk.</exception>
    byte[] RemoveDummies(int numDummies, byte[] value, string randomSeed);
}

/// <summary>
/// Utility class for inserting and removing dummy entries in the original byte array.
/// </summary>
public sealed class DummyUtil : IDummyUtil
{
    private const int MaxLengthPerDummy = 2500;
    private const int MinLengthPerDummy = 25;

    private readonly LazyLogger<DummyUtil> log = new();

    /// <inheritdoc/>
    public byte[] InsertDummies(int numDummies, byte[] value, string randomSeed)
    {
        var generator = Xor128Prng.FromString(randomSeed);

        int actualNumDummies = ComputeActualNumberOfDummies(generator, numDummies);

        log.Debug("Inserting [{0}] dummies using seed [{1}]", actualNumDummies, randomSeed);
        log.Debug("Byte count before inserting dummies: [{0}]", value.Length);

        // Generates an array in which each element represents the length that an
        // inserted dummy entry will have.
        int[] lengths = GenerateLengthsOfDummies(actualNumDummies, generator);

        // Generates an array in which each element represents the index the dummy
        // entry will be inserted at.
        int[] positions = GeneratePositions(generator, actualNumDummies, value.Length);

        byte[] endValue = new byte[value.Length + lengths.Sum()];
        for (int i = 0; i < value.Length; i++)
        {
            endValue[i] = value[i];
        }

        for (int i = 0; i < positions.Length; i++)
        {
            log.Trace("Inserting dummy at position [{0}] with length [{1}]", positions[i], lengths[i]);
            InsertRandomBytes(endValue, positions[i], lengths[i], generator);
        }

        log.Debug("Byte count after inserting dummies: [{0}]", endValue.Length);
        return endValue;
    }

    /// <inheritdoc/>
    public byte[] RemoveDummies(int numDummies, byte[] value, string randomSeed)
    {
        var generator = Xor128Prng.FromString(randomSeed);

        int actualNumDummies = ComputeActualNumberOfDummies(generator, numDummies);

        log.Debug("Removing [{0}] dummies using seed [{1}]", actualNumDummies, randomSeed);
        log.Debug("Byte count before removing dummies: [{0}]", value.Length);

        // calculate the length of the dummies originally added to the string
        int[] lengths = GenerateLengthsOfDummies(actualNumDummies, generator);
        Array.Reverse(lengths);
        int totalLength = lengths.Sum();

        // Calculate the length of the original byte array before the dummies were added
        // so we can determine the original position where the dummy entries were inserted.
        int lengthWithoutDummies = value.Length - totalLength;

        // generate the positions in which the dummy entries were inserted into the original string
        int[] positions = GeneratePositions(generator, actualNumDummies, lengthWithoutDummies);
        Array.Reverse(positions);

        byte[] result;
        try
        {
            var valueList = new List<byte>(value);
            for (int i = 0; i < positions.Length; i++)
            {
                log.Trace("Removing dummy at position [{0}] with length [{1}].", positions[i], lengths[i]);
                valueList.RemoveRange(positions[i], lengths[i]);
            }

            result = [.. valueList];
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new TransformationException("Unable to remove all dummy entries from chunk.", e);
        }

        log.Debug("Byte count after removing dummies: [{0}]", result.Length);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InsertRandomBytes(byte[] value, int position, int length, Xor128Prng generator)
    {
        // Shift the existing values over to the right.
        for (int i = value.Length - 1 - length; i >= position; i--)
        {
            value[i + length] = value[i];
        }

        // Insert the new values.
        for (int i = 0; i < length; i++)
        {
            value[position + i] = (byte)generator.Next(byte.MaxValue);
        }
    }

    private static int ComputeActualNumberOfDummies(Xor128Prng generator, int numDummies)
        => generator.Next(numDummies - (numDummies / 3)) + (numDummies / 3);

    private static int[] GenerateLengthsOfDummies(int numDummies, Xor128Prng generator)
        => Enumerable.Range(0, numDummies)
            .Select(i => generator.Next(MaxLengthPerDummy - MinLengthPerDummy) + MinLengthPerDummy)
            .ToArray();

    private static int[] GeneratePositions(Xor128Prng generator, int numberOfDummies, int maxPosition)
        => Enumerable.Range(0, numberOfDummies)
            .Select(i => generator.Next(maxPosition))
            .ToArray();
}