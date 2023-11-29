namespace SteganographyApp.Common.IO;

using System.Text;

/// <summary>
/// A queue like structure to provide a sequential set of bits from an input binary string.
/// Internally this keeps track of the position of the last character taken. This will not
/// remove a character from the provided binary string each time a character is taken.
/// </summary>
/// <remarks>
/// Initializes the queue.
/// </remarks>
/// <param name="binaryString">The binary string from which a character will be pulled each time
/// the Next method is invoked.</param>
internal sealed class ReadBitQueue(string binaryString)
{
    private readonly string binaryString = binaryString;
    private int position = 0;

    /// <summary>
    /// Returns a bit from the binary string then increments the current position.
    /// This method does not provide any safeguards to ensure the current position does not
    /// exceed the length of the array.
    /// </summary>
    /// <returns>A character representing a bit at the current position within the binary string.</returns>
    public char Next() => binaryString[position++];

    /// <summary>
    /// Returns a bit from the binary string then increments the current position.
    /// This provides a safety check to ensure the position is not yet greater than the length of the binary
    /// string before attempting to return a character from the string.
    /// If the current position exceeds the length of the string then the defaultTo value will be return.
    /// </summary>
    /// <param name="defaultTo">The default character to return if teh internal position is greater than
    /// the length of the binary string.</param>
    /// <returns>A character representing a bit at the current position within the binary string.
    /// If the current position exceeds the length of the binary string then the defaultTo value will
    /// be returned instead.</returns>
    public char Next(char defaultTo)
    {
        if (position == binaryString.Length)
        {
            return defaultTo;
        }
        return Next();
    }

    /// <summary>
    /// Checks to see if the current position is less than the length of the binary string.
    /// </summary>
    /// <returns>True if the current position is less than the length of the binary string, otherwise false.</returns>
    public bool HasNext() => position < binaryString.Length;
}

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
        if (bitsCurrentlyStored >= capacity)
        {
            return;
        }
        if (bitsCurrentlyStored + bits.Length > capacity)
        {
            int bitsToTake = bits.Length - (capacity - bitsCurrentlyStored);
            string bitsTaken = bits.Substring(0, bitsToTake);
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