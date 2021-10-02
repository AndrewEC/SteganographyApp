namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// Static class containing some simple utility methods.
    /// </summary>
    public static class Checks
    {
        private const string HelpArgument = "--help";
        private const string HelpArgumentShortened = "-h";

        /// <summary>
        /// Checks whether an immutable array is null or contains no elements.
        /// </summary>
        /// <param name="value">The immutable array to perform the null and empty checks against.</param>
        /// <typeparam name="T">The type of the input value array.</typeparam>
        /// <returns>True if the immutable array is null or contains no elements, otherwise false.</returns>
        public static bool IsNullOrEmpty<T>(ImmutableArray<T> value) => value == null || value.Length == 0;

        /// <summary>
        /// Checks if the provided action value existst within the array specified by
        /// the parameters values.
        /// </summary>
        /// <param name="action">The action type to validate exists in the values array.</param>
        /// <param name="values">The array of action types to check against.</param>
        /// <returns>True if the action can be found in the array of values, otherwise false.</returns>
        public static bool IsOneOf(ActionEnum action, params ActionEnum[] values) => Array.IndexOf(values, action) != -1;

        /// <summary>
        /// Checks to see if the help argument or the shortened help argument has been provided
        /// indicating that we should present the user with the help log.
        /// </summary>
        /// <param name="args">The array of unparsed command line arguments.</param>
        /// <returns>True if the full or short help argument flag name can be found in the array of raw argument values.</returns>
        public static bool WasHelpRequested(string[] args) => Array.IndexOf(args, HelpArgument) != -1 || Array.IndexOf(args, HelpArgumentShortened) != -1;
    }
}