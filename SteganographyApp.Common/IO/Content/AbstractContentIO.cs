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
    /// Disposes of the currently open stream.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() =>
    {
        Stream.Flush();
        Stream.Dispose();
    });

    /// <summary>
    /// Gets the stream instance initialized for reading or writing.
    /// </summary>
    /// <returns>A read write stream instance.</returns>
    protected abstract IReadWriteStream InitializeStream();
}