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

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/Trace/*' />
    public void Trace(string message, params object[] arguments) => Log(LogLevel.Trace, message, arguments);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/TraceProvider/*' />
    public void Trace(string message, Func<object[]> provider) => Log(LogLevel.Trace, message, provider);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/Debug/*' />
    public void Debug(string message, params object[] arguments) => Log(LogLevel.Debug, message, arguments);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/DebugProvider/*' />
    public void Debug(string message, Func<object[]> provider) => Log(LogLevel.Debug, message, provider);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/Error/*' />
    public void Error(string message, params object[] arguments) => Log(LogLevel.Error, message, arguments);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/ErrorProvider/*' />
    public void Error(string message, Func<object[]> provider) => Log(LogLevel.Error, message, provider);

    private void Log(LogLevel level, string message, params object[] arguments) => RootLogger.Instance.LogToFile(typeName, level, message, arguments);

    private void Log(LogLevel level, string message, Func<object[]> provider) => RootLogger.Instance.LogToFile(typeName, level, message, provider);
}