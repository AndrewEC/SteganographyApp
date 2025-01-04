namespace SteganographyApp.Common.IO.Pixels;

using System.Text;

/// <summary>
/// A structure for taking in a series of bits and aggregating the bits into a continuous string.
/// </summary>
/// <remarks>
/// Initializes the binary string builder with the specified maximum capacity.
/// </remarks>
/// <param name="capacity">The total number of bits the binary string builder can house.
/// Any bit one attempts to add beyond the capacity will be silently rejected.</param>
internal sealed class BinaryStringBuilder(int capacity)
{
    private readonly int capacity = capacity;
    private readonly StringBuilder binary = new();
    private int bitsCurrentlyStored = 0;

    /// <summary>
    /// Adds a set of bits to the currently aggregated set of bits. If the number of input
    /// bits and the number of bits already aggregated exceed the capacity of the aggregator
    /// then either a subset of the inputs bits will be taken or none at all.
    /// </summary>
    /// <param name="bits">The bits to add to the queue.</param>
    public void Put(string bits)
    {
        if (IsFull())
        {
            return;
        }
        if (bitsCurrentlyStored + bits.Length > capacity)
        {
            int remainingCapacity = capacity - bitsCurrentlyStored;
            string bitsTaken = bits.Substring(0, remainingCapacity);
            binary.Append(bitsTaken);
            bitsCurrentlyStored += bitsTaken.Length;
        }
        else
        {
            binary.Append(bits);
            bitsCurrentlyStored += bits.Length;
        }
    }

    /// <summary>
    /// Checks if the number of bits in the aggregated binary string is equal to the capacity of the aggregator.
    /// </summary>
    /// <returns>True if the number of currently aggregated bits is greater than or equal to the capacity
    /// of the aggregator, otherwise false.</returns>
    public bool IsFull() => bitsCurrentlyStored >= capacity;

    /// <summary>
    /// Gets the aggregated series of bits as a single continuous string.
    /// </summary>
    /// <returns>Returns a continuous binary string.</returns>
    public string ToBinaryString() => binary.ToString();
}