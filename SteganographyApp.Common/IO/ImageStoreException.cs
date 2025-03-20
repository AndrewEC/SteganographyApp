namespace SteganographyApp.Common.IO;

using System;

/// <summary>
/// A general exception to represent a specific error occured
/// while reading or writing data to the images.
/// </summary>
public sealed class ImageStoreException : Exception
{
    /// <summary>
    /// Initializes the exception with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ImageStoreException(string message)
    : base(message) { }

    /// <summary>
    /// Initializes the exception with a message and an inner cause.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The root cause of this exception.</param>
    public ImageStoreException(string message, Exception inner)
    : base(message, inner) { }
}