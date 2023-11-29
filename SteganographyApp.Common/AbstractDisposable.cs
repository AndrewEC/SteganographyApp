namespace SteganographyApp.Common;

using System;

/// <summary>
/// Deletegate to be invoked if the current AbstractDisposable instance has
/// not yet been disposed.
/// </summary>
public delegate void UnmanagedResourceFunction();

/// <summary>
/// Delegate to be invoked, and its result returned, if the current AbstractDisposable
/// instance has not yet been disposed.
/// </summary>
/// <typeparam name="T">The type of the value being returned by this function.</typeparam>
/// <returns>The result of type T.</returns>
public delegate T UnmanagedResourceFunctionWithResult<T>();

/// <summary>
/// Utility class to help on two fronts. To safely dispose of a disposable object and to
/// safely perform operations involving unmanaged resources. A single instance of this class
/// cannot be reused across multiple other instances.
/// </summary>
public abstract class AbstractDisposable : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the Dispose method has completed execution.
    /// </summary>
    protected bool WasDisposed { get; private set; } = false;

    /// <summary>
    /// Disposes of the current instance. This can only be executed once as once the first execution completes it will set the
    /// WasDisposed property to ensure that a second execution cannot complete.
    /// </summary>
    public void Dispose()
    {
        if (WasDisposed)
        {
            return;
        }
        try
        {
            Dispose(true);
        }
        catch (Exception)
        {
        }
        GC.SuppressFinalize(this);
        WasDisposed = true;
    }

    /// <summary>
    /// Executes the input function if the Dispose method has not yet been invoked. If the Dispose method has already completed
    /// execution then this will throw a diposed exception.
    /// </summary>
    /// <param name="function">The function to be executed if the Dispose method has not yet completed execution.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the Dispose method has already completed execution.</exception>
    protected void RunIfNotDisposed(UnmanagedResourceFunction function)
    {
        if (WasDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
        function();
    }

    /// <summary>
    /// Executes the input function and returns the result if the Dispose method has not yet completed execution. If the
    /// Dispose method has already completed execution then this will throw a disposed exception.
    /// </summary>
    /// <typeparam name="T">The return type of the input function being executed.</typeparam>
    /// <param name="function">The function to execute and return the result of.</param>
    /// <returns>The result of the execution of the input function.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the Dispose method has already completed execution.</exception>
    protected T RunIfNotDisposedWithResult<T>(UnmanagedResourceFunctionWithResult<T> function)
    {
        if (WasDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
        return function();
    }

    /// <summary>
    /// Disposes of the current instance. Any implementation of this method should check if disposing is true and,
    /// if it is not, skip the execution of the remainder of the method.
    /// </summary>
    /// <param name="disposing">Indicates if this method was called from the base Dispose method.</param>
    protected abstract void Dispose(bool disposing);
}