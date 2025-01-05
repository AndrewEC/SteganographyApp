namespace SteganographyApp.Common.IO;

/// <summary>
/// Specifies the type of IO operations the opened stream needs
/// to support.
/// </summary>
public enum StreamMode
{
    /// <summary>
    /// Specifies that the stream should only allow
    /// read operations to occur.
    /// </summary>
    Read,

    /// <summary>
    /// Specifies that the stream should only allow
    /// write operations to occur.
    /// </summary>
    Write,
}