namespace SteganographyApp.Common.Data
{
    using System;
    using System.Text;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Contract for interacting with the concrete BinaryUtil implementation.
    /// </summary>
    public interface IBinaryUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBase64String/*' />
        string ToBase64String(string binary);

        /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBinaryString/*' />
        string ToBinaryString(string base64String);
    }

    /// <summary>
    /// Injectable Utility class for encoding a binary string to base64 and from base64 to binary.
    /// </summary>
    [Injectable(typeof(IBinaryUtil))]
    public class BinaryUtil : IBinaryUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBase64String/*' />
        public string ToBase64String(string binary)
        {
            byte[] bytes = new byte[binary.Length / 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                var rawValue = binary.Substring(i * 8, 8);
                bytes[i] = Convert.ToByte(rawValue, 2);
            }
            return Convert.ToBase64String(bytes);
        }

        /// <include file='../docs.xml' path='docs/members[@name="BinaryUtil"]/ToBinaryString/*' />
        public string ToBinaryString(string base64String)
        {
            byte[] converted = Convert.FromBase64String(base64String);
            int capacity = converted.Length * 8;
            var builder = new StringBuilder(capacity);
            foreach (byte bit in converted)
            {
                builder.Append(Convert.ToString(bit, 2).PadLeft(8, '0'));
            }
            return builder.ToString();
        }
    }
}