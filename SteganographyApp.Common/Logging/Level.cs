namespace SteganographyApp.Common.Logging;

/// <summary>
/// The standard log level enumeration indicating the importance of the log message.
/// Used to control when a log message will be ultimately logged or not.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// The most detailed log level. Logs at this level should typically be used to describe
    /// application flow and which step the application is on.
    /// </summary>
    Trace,

    /// <summary>
    /// The middling log level. Logs at this levle should typically be used to log calculated
    /// runtime variables to troubleshoot issues and identity areas of improvement.
    /// </summary>
    Debug,

    /// <summary>
    /// This highes, least verbose, log level. Logs at this level should be used for error messages.
    /// </summary>
    Error,

    /// <summary>
    /// Log level indicating the nothing should be logged.
    /// </summary>
    None,
}