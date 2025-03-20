namespace SteganographyApp.Common.Data;

using System;

/// <summary>
/// A case class inheriting from Exception that specifies an error occured
/// when an IFileCoder instance attempted to transform input data.
/// </summary>
public class TransformationException : Exception
{
    /// <summary>
    /// Initializes the exception with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TransformationException(string message)
    : base(message) { }

    /// <summary>
    /// Initializes the exception with a message and an inner cause.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The root cause of this exception.</param>
    public TransformationException(string message, Exception inner)
    : base(message, inner) { }
}
