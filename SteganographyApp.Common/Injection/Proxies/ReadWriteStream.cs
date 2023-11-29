namespace SteganographyApp.Common.Injection;

using System;
using System.IO;

/// <summary>
/// Provides an interface wrapper for some of the basic
/// read/write stream operations.
/// </summary>
public interface IReadWriteStream : IDisposable
{
    /// <include file='../../docs.xml' path='docs/members[@name="ReadWriteStream"]/Read/*' />
    int Read(byte[] array, int offset, int count);

    /// <include file='../../docs.xml' path='docs/members[@name="ReadWriteStream"]/Write/*' />
    void Write(byte[] array, int offset, int count);

    /// <include file='../../docs.xml' path='docs/members[@name="ReadWriteStream"]/Flush/*' />
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
    public static ReadWriteStream CreateStreamForRead(string pathToFile) => new(File.OpenRead(pathToFile));

    /// <summary>
    /// Creates a ReadWriteStream instance which allows for writing to a specified destination.
    /// </summary>
    /// <param name="pathToFile">The relative or absolute path to the destination to write to.</param>
    /// <returns>A readWriteStream instance configured to write to the specified file.</returns>
    public static ReadWriteStream CreateStreamForWrite(string pathToFile) => new(File.Open(pathToFile, FileMode.OpenOrCreate));

    /// <include file='../../docs.xml' path='docs/members[@name="ReadWriteStream"]/Read/*' />
    public int Read(byte[] array, int offset, int count) => RunIfNotDisposedWithResult(() => stream.Read(array, offset, count));

    /// <include file='../../docs.xml' path='docs/members[@name="ReadWriteStream"]/Write/*' />
    public void Write(byte[] array, int offset, int count) => RunIfNotDisposed(() => stream.Write(array, offset, count));

    /// <include file='../../docs.xml' path='docs/members[@name="ReadWriteStream"]/Flush/*' />
    public void Flush() => RunIfNotDisposed(stream.Flush);

    /// <summary>
    /// Disposes of the currently opened stream represented by this instance.
    /// </summary>
    /// <param name="disposing">Specifies if this method is being invoked from the main Dispose method or the finalizer.</param>
    protected override void Dispose(bool disposing) => RunIfNotDisposed(() =>
    {
        if (!disposing)
        {
            return;
        }
        stream.Dispose();
    });
}