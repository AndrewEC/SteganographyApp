namespace SteganographyApp.Common.Logging;

using System;

/// <summary>
/// The interface to the standard logger.
/// </summary>
public interface ILogger
{
    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/Trace/*' />
    void Trace(string message, params object[] arguments);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/TraceProvider/*' />
    void Trace(string message, Func<object[]> provider);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/Debug/*' />
    void Debug(string message, params object[] arguments);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/DebugProvider/*' />
    void Debug(string message, Func<object[]> provider);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/Error/*' />
    void Error(string message, params object[] arguments);

    /// <include file='../docs.xml' path='docs/members[@name="Logger"]/ErrorProvider/*' />
    void Error(string message, Func<object[]> provider);
}
