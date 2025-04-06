namespace SteganographyApp.Common.Logging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// The contract for interacting with the LoggerFactory instance.
/// </summary>
public interface ILoggerFactory
{
    /// <summary>
    /// Creates an ILogger instance for the specified type. The type should specify the class that will
    /// house and make use of the ILogger instance.
    /// </summary>
    /// <param name="type">The class type that will house and make use of the ILogger instance.</param>
    /// <returns>A new ILogger instance configured for the specified type.</returns>
    ILogger LoggerFor(Type type);
}

/// <summary>
/// The concrete logger factory responsible for supplying ILogger instances.
/// </summary>
public class LoggerFactory : ILoggerFactory
{
    private static readonly CompositeFormat TypeNameFormat = CompositeFormat.Parse("{0}.{1}");

    private readonly Dictionary<string, ILogger> loggerCache = [];

    /// <summary>
    /// Creates an ILogger instance for the specified type. The type should specify the class that will
    /// house and make use of the ILogger instance.
    /// </summary>
    /// <param name="type">The class type that will house and make use of the ILogger instance.</param>
    /// <returns>A new ILogger instance configured for the specified type.</returns>
    public ILogger LoggerFor(Type type)
    {
        string name = string.Format(CultureInfo.InvariantCulture, TypeNameFormat, type.Namespace, type.Name);
        if (loggerCache.TryGetValue(name, out ILogger? value))
        {
            return value;
        }

        var logger = new Logger(name);
        loggerCache[name] = logger;
        return logger;
    }
}