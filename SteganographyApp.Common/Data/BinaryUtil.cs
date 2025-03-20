namespace SteganographyApp.Common.Data;

using System;
using System.Linq;
using System.Text;

/// <summary>
/// Contract for interacting with the concrete BinaryUtil implementation.
/// </summary>
public interface IBinaryUtil
{
    /// <summary>
    /// Converts a byte array into a binary string representation. This will convert each byte into a padded 8 bit string
    /// and concatenate all values into a single string.
    /// </summary>
    /// <param name="bytes">The array of bytes to be converted into a binary string.</param>
    /// <returns>A binary representation of the input byte array.</returns>
    string ToBinaryString(byte[] bytes);

    /// <summary>
    /// Converts a byte array into a binary string representation. This expects each byte in the array to
    /// have a decimal value of 0 or 1. Each by will thus be added to the binary string using their
    /// direct decimal representation.
    /// </summary>
    /// <param name="bytes">The array of bytes to be converted into a binary string.</param>
    /// <returns>A binary representation of the input byte array.</returns>
    string ToBinaryStringDirect(byte[] bytes);

    /// <summary>
    /// Converts a given binary string into an array of bytes. This break the binary string down and take 8 characters, 8 bits
    /// at a time and convert that set of 8 bits to a byte value before aggregating the values in the byte array.
    /// This method expecs the binary string to have a length equally divisible by 8.
    /// </summary>
    /// <param name="binary">The binary string to be converted to a byte array.</param>
    /// <returns>A byte array representation of the original input binary string.</returns>
    byte[] ToBytes(string binary);

    /// <summary>
    /// Converts a given binary string into an array of bytes. This will take each bit in the binary string and convert that bit
    /// directly into a byte. This means the resulting byte array will only have values of 0 or 1.
    /// </summary>
    /// <param name="binary">The binary string to be converted to a byte array.</param>
    /// <returns>A byte array representation of the roiginal input binary string.</returns>
    byte[] ToBytesDirect(string binary);
}

/// <summary>
/// Injectable Utility class for encoding a binary string to base64 and from base64 to binary.
/// </summary>
public sealed class BinaryUtil : IBinaryUtil
{
    private const int BitsPerByte = 8;

    /// <inheritdoc/>
    public string ToBinaryString(byte[] bytes)
    {
        int numberOfBits = bytes.Length * BitsPerByte;
        var builder = new StringBuilder(numberOfBits);
        foreach (byte bit in bytes)
        {
            builder.Append(To8BitBinaryString(bit));
        }

        return builder.ToString();
    }

    /// <inheritdoc/>
    public string ToBinaryStringDirect(byte[] bytes)
    {
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b);
        }

        return builder.ToString();
    }

    /// <inheritdoc/>
    public byte[] ToBytes(string binary)
    {
        int byteCount = binary.Length / BitsPerByte;
        byte[] binaryBytes = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
        {
            binaryBytes[i] = Convert.ToByte(binary.Substring(BitsPerByte * i, BitsPerByte), 2);
        }

        return binaryBytes;
    }

    /// <inheritdoc/>
    public byte[] ToBytesDirect(string binary) => binary.Select(c => (byte)char.GetNumericValue(c)).ToArray();

    private static string To8BitBinaryString(byte input) => Convert.ToString(input, 2).PadLeft(BitsPerByte, '0');
}