namespace SteganographyApp.Common.Logging;

using System;
using SteganographyApp.Common.Injection;

/// <summary>
/// A logger class that allows a logger instance to be lazily initialized.
/// </summary>
/// <typeparam name="T">The type of the class that will be consuming the underlying ILogger instance.</typeparam>
public sealed class LazyLogger<T> : ILogger
{
    private ILogger? instance;

    /// <inheritdoc/>
    public void Trace(string message, params object[] arguments) => GetInstance().Trace(message, arguments);

    /// <inheritdoc/>
    public void Trace(string message, Func<object[]> provider) => GetInstance().Trace(message, provider);

    /// <inheritdoc/>
    public void Debug(string message, params object[] arguments) => GetInstance().Debug(message, arguments);

    /// <inheritdoc/>
    public void Debug(string message, Func<object[]> provider) => GetInstance().Debug(message, provider);

    /// <inheritdoc/>
    public void Error(string message, params object[] arguments) => GetInstance().Error(message, arguments);

    /// <inheritdoc/>
    public void Error(string message, Func<object[]> provider) => GetInstance().Error(message, provider);

    private ILogger GetInstance() => instance ??= ServiceContainer.GetLogger<T>();
}