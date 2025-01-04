namespace SteganographyApp.Common.Logging;

using System;

using SteganographyApp.Common.Injection;

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
[Injectable(typeof(ILoggerFactory))]
public class LoggerFactory : ILoggerFactory
{
    private const string TypeNameTemplate = "{0}.{1}";

    /// <summary>
    /// Creates an ILogger instance for the specified type. The type should specify the class that will
    /// house and make use of the ILogger instance.
    /// </summary>
    /// <param name="type">The class type that will house and make use of the ILogger instance.</param>
    /// <returns>A new ILogger instance configured for the specified type.</returns>
    public ILogger LoggerFor(Type type)
    {
        string name = string.Format(TypeNameTemplate, type.Namespace, type.Name);
        return new Logger(name);
    }
}