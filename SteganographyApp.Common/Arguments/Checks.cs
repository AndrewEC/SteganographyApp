using System;

namespace SteganographyApp.Common.Arguments
{

    public static class Checks
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

        /// <summary>
        /// Checks if the provided action value existst within the array specified by
        /// the parameters values.
        /// </summary>
        /// <param name="action">The action type to validate exists in the values array.</param>
        /// <param name="values">The array of action types to check against.</param>
        public static bool IsOneOf(ActionEnum action, params ActionEnum[] values)
        {
            return Array.IndexOf(values, action) != -1;
        }

        /// <summary>
        /// Checks if the action value is either CalculateStorageSpace or its alias CSS.
        /// </summary>
        /// <param name="action">The parsed action value to check.</param>
        public static bool IsCalculateStorageSpaceAction(ActionEnum action)
        {
            return action == ActionEnum.CalculateStorageSpace || action == ActionEnum.CSS;
        }

        /// <summary>
        /// Checks if the action value is either CalculateEncryptedSize or its alias CES.
        /// </summary>
        /// <param name="action">The parsed action value to check.</param>
        public static bool IsCalculateEncryptedSpaceAction(ActionEnum action)
        {
            return action == ActionEnum.CalculateEncryptedSize || action == ActionEnum.CES;
        }

    }
}