namespace SteganographyApp.Common.IO.Pixels;

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
        if (!HasNext())
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