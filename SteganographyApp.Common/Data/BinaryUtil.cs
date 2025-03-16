namespace SteganographyApp.Common.Data;

using System;
using System.Linq;
using System.Text;

/// <summary>
/// Contract for interacting with the concrete BinaryUtil implementation.
/// </summary>
public interface IBinaryUtil
{
    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBinaryString/*' />
    string ToBinaryString(byte[] bytes);

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBinaryStringDirect/*' />
    string ToBinaryStringDirect(byte[] bytes);

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBytes/*' />
    byte[] ToBytes(string binary);

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBytesDirect/*' />
    byte[] ToBytesDirect(string binary);
}

/// <summary>
/// Injectable Utility class for encoding a binary string to base64 and from base64 to binary.
/// </summary>
public sealed class BinaryUtil : IBinaryUtil
{
    private const int BitsPerByte = 8;

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBinaryString/*' />
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

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBinaryStringDirect/*' />
    public string ToBinaryStringDirect(byte[] bytes)
    {
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b);
        }

        return builder.ToString();
    }

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBytes/*' />
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

    /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBytesDirect/*' />
    public byte[] ToBytesDirect(string binary) => binary.Select(c => (byte)char.GetNumericValue(c)).ToArray();

    private static string To8BitBinaryString(byte input) => Convert.ToString(input, 2).PadLeft(BitsPerByte, '0');
}