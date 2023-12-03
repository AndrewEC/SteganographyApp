namespace SteganographyApp.Common.Arguments;

using System;

/// <summary>
/// An exception thrown indicating something failed while attempting to parse the user provided arguments.
/// </summary>
public class ParseException : Exception
{
    /// <summary>
    /// Initialize the exception with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ParseException(string message) : base(message) { }

    /// <summary>
    /// Initialize the exception with a message and an underlying cause.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="cause">The underlying cause.</param>
    public ParseException(string message, Exception cause) : base(message, cause) { }
}