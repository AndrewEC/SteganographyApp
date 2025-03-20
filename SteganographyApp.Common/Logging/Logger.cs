namespace SteganographyApp.Common.Logging;

using System;

/// <summary>
/// The concrete ILogger implementation that provides some proxy methods to help fill out values
/// that will subsequently be passed to the RootLogger and written to the log file.
/// </summary>
/// <remarks>
/// Initialize a logger instance for the specified type.
/// </remarks>
/// <param name="typeName">The name of the object type that will be invoking thsi ILogger instance.</param>
internal sealed class Logger(string typeName) : ILogger
{
    private readonly string typeName = typeName;

    /// <inheritdoc/>
    public void Trace(string message, params object[] arguments) => Log(LogLevel.Trace, message, arguments);

    /// <inheritdoc/>
    public void Trace(string message, Func<object[]> provider) => Log(LogLevel.Trace, message, provider);

    /// <inheritdoc/>
    public void Debug(string message, params object[] arguments) => Log(LogLevel.Debug, message, arguments);

    /// <inheritdoc/>
    public void Debug(string message, Func<object[]> provider) => Log(LogLevel.Debug, message, provider);

    /// <inheritdoc/>
    public void Error(string message, params object[] arguments) => Log(LogLevel.Error, message, arguments);

    /// <inheritdoc/>
    public void Error(string message, Func<object[]> provider) => Log(LogLevel.Error, message, provider);

    private void Log(LogLevel level, string message, params object[] arguments) => RootLogger.Instance.LogToFile(typeName, level, message, arguments);

    private void Log(LogLevel level, string message, Func<object[]> provider) => RootLogger.Instance.LogToFile(typeName, level, message, provider);
}