namespace SteganographyApp.Common.Injection.Proxies;

using System.IO;

/// <summary>
/// A proxy interface to interacting with some of the common file IO related functions.
/// </summary>
public interface IFileIOProxy
{
    /// <summary>
    /// Attempts to get the size of the file, specified by input path, in bytes.
    /// </summary>
    /// <param name="pathToFile">The absolute or relative path to the file.</param>
    /// <returns>The size of the file in bytes.</returns>
    long GetFileSizeBytes(string pathToFile);

    /// <summary>
    /// Checks if the file at the specified path exists and is a file and not a directory.
    /// </summary>
    /// <param name="pathToFile">The absolute or relative path to the file.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    bool IsExistingFile(string pathToFile);

    /// <summary>
    /// Opens an IO stream to read from a specified file.
    /// </summary>
    /// <param name="pathToFile">The path to the file to open the read stream to.</param>
    /// <returns>A stream made to read from the specified file.</returns>
    IReadWriteStream OpenFileForRead(string pathToFile);

    /// <summary>
    /// Opens an IO stream to the target destination for write.
    /// </summary>
    /// <param name="pathToFile">The path to the file to write to.</param>
    /// <returns>A stream made to write to the specified destination.</returns>
    IReadWriteStream OpenFileForWrite(string pathToFile);

    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="pathToFile">The relative or absolute path to the file to delete.</param>
    void Delete(string pathToFile);
}

/// <summary>
/// Concrete implementation of the file IO proxy. Used to proxy calls to static IO based methods
/// so that such calls can be better mocked in unit tests.
/// </summary>
public class FileIOProxy : IFileIOProxy
{
    /// <inheritdoc/>
    public long GetFileSizeBytes(string pathToFile) => new FileInfo(pathToFile).Length;

    /// <inheritdoc/>
    public bool IsExistingFile(string pathToFile)
        => File.Exists(pathToFile)
            && !File.GetAttributes(pathToFile).HasFlag(FileAttributes.Directory);

    /// <inheritdoc/>
    public IReadWriteStream OpenFileForRead(string pathToFile)
        => ReadWriteStream.CreateStreamForRead(pathToFile);

    /// <inheritdoc/>
    public IReadWriteStream OpenFileForWrite(string pathToFile)
        => ReadWriteStream.CreateStreamForWrite(pathToFile);

    /// <inheritdoc/>
    public void Delete(string pathToFile) => File.Delete(pathToFile);
}