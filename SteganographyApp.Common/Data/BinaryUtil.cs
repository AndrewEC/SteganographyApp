using System;
using System.Text;

namespace SteganographyApp.Common.Data
{

    public static class BinaryUtil
    {

        /// <summary>
        /// Converts a binary string to a base64 encoded string.
        /// </summary>
        /// <param name="binary">The original string representation of a binary figure to
        /// convert to base64.</param>
        public static string ToBase64String(string binary)
        {
            byte[] bytes = new byte[binary.Length / 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                var rawValue = binary.Substring(i * 8, 8);
                bytes[i] = Convert.ToByte(rawValue, 2);
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts a base64 encoded string to a string of binary.
        /// </summary>
        /// <param name="base64String">The base64 encoded string to convert to
        /// binary.</param>
        public static string ToBinaryString(string base64String)
        {
            byte[] converted = Convert.FromBase64String(base64String);
            var builder = new StringBuilder();
            foreach (byte bit in converted)
            {
                builder.Append(Convert.ToString(bit, 2).PadLeft(8, '0'));
            }
            return builder.ToString();
        }
    }

}