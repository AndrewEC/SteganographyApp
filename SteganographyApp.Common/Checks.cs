using System;

namespace SteganographyApp.Common
{

    public class Checks
    {

        /// <summary>
        /// Checks whether a string is null or contains no characters.
        /// </summary>
        /// <param name="value">The string value to perform the null and
        /// empty checks on.</param>
        public static bool IsNullOrEmpty(string value)
        {
            return value == null || value.Length == 0;
        }

        /// <summary>
        /// Checks whether an array is null or contains no elements.
        /// </summary>
        /// <param name="value">The array to perform the null and empty
        /// checks against.</param>
        public static bool IsNullOrEmpty<T>(T[] value)
        {
            return value == null || value.Length == 0;
        }

        public static bool IsOneOf(EncodeDecodeAction action, params EncodeDecodeAction[] values)
        {
            return Array.IndexOf(values, action) != -1;
        }
    }
}