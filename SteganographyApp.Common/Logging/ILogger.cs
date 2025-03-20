namespace SteganographyApp.Common.Logging;

using System;

/// <summary>
/// The interface to the standard logger.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a message to the log file with the level of Trace.
    /// </summary>
    /// <param name="message">The template string to log to file.</param>
    /// <param name="arguments">The arguments to be spliced into the template message string.</param>
    void Trace(string message, params object[] arguments);

    /// <summary>
    /// Logs a message to the log file with the level of Trace.
    /// </summary>
    /// <param name="message">The template string to log to file.</param>
    /// <param name="provider">The producer function to provide the arguments to be spliced into the template message string.
    /// This will only be invoked if it is determine that the message, at the current log level, should
    /// be logged at all.</param>
    void Trace(string message, Func<object[]> provider);

    /// <summary>
    /// Logs a message to the log file with the level of Debug.
    /// </summary>
    /// <param name="message">The template string to log to file.</param>
    /// <param name="arguments">The arguments to be spliced into the template message string.</param>
    void Debug(string message, params object[] arguments);

    /// <summary>
    /// Logs a message to the log file with the level of Debug.
    /// </summary>
    /// <param name="message">The template string to log to file.</param>
    /// <param name="provider">The producer function to provide the arguments to be spliced into the template message string.
    /// This will only be invoked if it is determine that the message, at the current log level, should
    /// be logged at all.</param>
    void Debug(string message, Func<object[]> provider);

    /// <summary>
    /// Logs a message to the log file with the level of Error.
    /// </summary>
    /// <param name="message">The template string to log to file.</param>
    /// <param name="arguments">The arguments to be spliced into the template message string.</param>
    void Error(string message, params object[] arguments);

    /// <summary>
    /// Logs a message to the log file with the level of Error.
    /// </summary>
    /// <param name="message">The template string to log to file.</param>
    /// <param name="provider">The producer function to provide the arguments to be spliced into the template message string.
    /// This will only be invoked if it is determine that the message, at the current log level, should
    /// be logged at all.</param>
    void Error(string message, Func<object[]> provider);
}
