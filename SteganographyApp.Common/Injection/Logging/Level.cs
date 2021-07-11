namespace SteganographyApp.Common.Logging
{
    /// <summary>
    /// The standard log level enumeration indicating the importance of the log message.
    /// Used to control when a log message will be ultimately logged or not.
    /// </summary>
    public enum LogLevel
    {
        Trace,
        Debug,
        Error,
        None,
    }
}