namespace SteganographyApp.Common.IO.Content;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Base class for read and write IO. Implements the basic disposable logic to be shared
/// with the content read and write child classes.
/// </summary>
public abstract class AbstractContentIO : AbstractDisposable
{
#pragma warning disable CS1591, SA1600
    public AbstractContentIO(
        IInputArguments arguments,
        IDataEncoderUtil dataEncoderUtil,
        IFileIOProxy fileIOProxy)
    {
        Arguments = arguments;
        DataEncoderUtil = dataEncoderUtil;
        FileIOProxy = fileIOProxy;
        Stream = InitializeStream();
    }
#pragma warning restore CS1591, SA1600

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
    /// Gets the data encoder util to encode/decode the file contents.
    /// </summary>
    protected IDataEncoderUtil DataEncoderUtil { get; private set; }

    /// <summary>
    /// Gets the file proxy used to open the read or write file stream.
    /// </summary>
    protected IFileIOProxy FileIOProxy { get; private set; }

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