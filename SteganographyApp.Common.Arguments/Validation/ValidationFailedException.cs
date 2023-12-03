namespace SteganographyApp.Common.Arguments.Validation;

using System;

/// <summary>
/// An exception to indciate that a value for a given field was not valid
/// as the per the rules of the evaluating validation attribute.
/// </summary>
public class ValidationFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationFailedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="cause">The root cause of the validation exception.</param>
    public ValidationFailedException(string message, Exception cause) : base(message, cause) { }
}