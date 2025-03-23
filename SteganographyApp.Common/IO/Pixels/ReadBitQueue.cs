namespace SteganographyApp.Common.IO.Pixels;

/// <summary>
/// A queue like structure to provide a sequential set of bits from an input binary string.
/// </summary>
/// <param name="binaryString">The binary string from which a character will be pulled from.</param>
internal sealed class ReadBitQueue(string binaryString)
{
    private readonly string binaryString = binaryString;
    private int position = 0;

    /// <summary>
    /// Returns the binary character at the queues current position. If there are no more
    /// bits left to return the the <paramref name="defaultTo"/> value will be returned.
    /// </summary>
    /// <param name="defaultTo">The default value to return if there are no more
    /// bits left in the queue to read.</param>
    /// <returns>The next available bit in the queue or the <paramref name="defaultTo"/>
    /// value if no more bits are available.</returns>
    public char Next(char defaultTo)
    {
        if (!HasNext())
        {
            return defaultTo;
        }

        return binaryString[position++];
    }

    /// <summary>
    /// Checks if the queue has more bits that can be returned via the
    /// <see cref="Next(char)"/> method call.
    /// </summary>
    /// <returns>True if there are more bits remaining.</returns>
    public bool HasNext() => position < binaryString.Length;
}