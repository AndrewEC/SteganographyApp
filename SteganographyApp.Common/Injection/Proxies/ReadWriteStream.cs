namespace SteganographyApp.Common.Injection.Proxies;

using System;
using System.IO;

/// <summary>
/// Provides an interface wrapper for some of the basic
/// read/write stream operations.
/// </summary>
public interface IReadWriteStream : IDisposable
{
    /// <summary>
    /// Reads a number of bytes from the current file stream.
    /// </summary>
    /// <param name="array">The array of bytes for values to be read into.</param>
    /// <param name="offset">The byte offset to start reading from.</param>
    /// <param name="count">The number of bytes to read from the file.</param>
    /// <returns>The number of bytes read from the file stream.</returns>
    int Read(byte[] array, int offset, int count);

    /// <summary>
    /// Writes a number of bytes to the current file stream.
    /// </summary>
    /// <param name="array">The array of bytes to write to the stream.</param>
    /// <param name="offset">The byte offset to start writing from.</param>
    /// <param name="count">The number of bytes to write to the file.</param>
    void Write(byte[] array, int offset, int count);

    /// <summary>
    /// Calls flush on the current file stream.
    /// </summary>
    void Flush();
}

/// <summary>
/// A concrete implementation of the read write stream. Used to help proxy calls to a file stream
/// so they can be better mocked in unit tests.
/// </summary>
public class ReadWriteStream : AbstractDisposable, IReadWriteStream
{
    private readonly FileStream stream;

    private ReadWriteStream(FileStream stream)
    {
        this.stream = stream;
    }

    /// <summary>
    /// Creates a ReadWriteStream instance which allows for reading of the contents of a file.
    /// </summary>
    /// <param name="pathToFile">The relative or absolute path to the file to read from.</param>
    /// <returns>A ReadWriteStream instance configured to read from the specified file.</returns>
    public static ReadWriteStream CreateStreamForRead(string pathToFile)
        => new(File.OpenRead(pathToFile));

    /// <summary>
    /// Creates a ReadWriteStream instance which allows for writing to a specified destination.
    /// </summary>
    /// <param name="pathToFile">The relative or absolute path to the destination to write to.</param>
    /// <returns>A readWriteStream instance configured to write to the specified file.</returns>
    public static ReadWriteStream CreateStreamForWrite(string pathToFile)
        => new(File.Open(pathToFile, FileMode.OpenOrCreate));

    /// <inheritdoc/>
    public int Read(byte[] array, int offset, int count)
        => RunIfNotDisposedWithResult(() => stream.Read(array, offset, count));

    /// <inheritdoc/>
    public void Write(byte[] array, int offset, int count)
        => RunIfNotDisposed(() => stream.Write(array, offset, count));

    /// <inheritdoc/>
    public void Flush() => RunIfNotDisposed(stream.Flush);

    /// <summary>
    /// Disposes of the currently opened stream represented by this instance.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() => stream.Dispose());
}