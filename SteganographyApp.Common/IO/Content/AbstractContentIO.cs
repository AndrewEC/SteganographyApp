namespace SteganographyApp.Common.IO.Content;

using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Base class for read and write IO. Implements the basic disposable logic to be shared
/// with the content read and write child classes.
/// </summary>
public abstract class AbstractContentIO : AbstractDisposable
{
    /// <summary>
    /// Initialize the abstract content IO instance with the user provided input arguments.
    /// </summary>
    /// <param name="arguments">The user provided input arguments.</param>
    public AbstractContentIO(IInputArguments arguments)
    {
        Arguments = arguments;
        Stream = InitializeStream();
    }

    /// <summary>
    /// Gets the values parsed from the command line arguments.
    /// </summary>
    protected IInputArguments Arguments { get; }

    /// <summary>
    /// Gets the stream used by the underlying implementation to read
    /// or write data to a specified file.
    /// </summary>
    protected IReadWriteStream Stream { get; private set; }

    /// <summary>
    /// Disposes of the current instance. Any implementation of this method should check if disposing is true and,
    /// if it is not, skip the execution of the remainder of the method.
    /// </summary>
    /// <param name="disposing">Indicates if this method was called from the base Dispose method.</param>
    protected override void Dispose(bool disposing) => RunIfNotDisposed(() =>
    {
        if (!disposing)
        {
            return;
        }
        Stream.Flush();
        Stream.Dispose();
    });

    /// <summary>
    /// Gets the stream instance initialized for reading or writing.
    /// </summary>
    /// <returns>A read write stream instance.</returns>
    protected abstract IReadWriteStream InitializeStream();
}