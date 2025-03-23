namespace SteganographyApp.Common.Data;

using System;

/// <summary>
/// Indicates there was likely an error decrypting previously encrypted
/// data in the <see cref="IDataEncoderUtil"/>.
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
