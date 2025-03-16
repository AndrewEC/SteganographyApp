namespace SteganographyApp.Common.IO.Content;

using System;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Stream encapsulation class that reads and encodes data
/// from the input file.
/// </summary>
public sealed class ContentReader : AbstractContentIO
{
#pragma warning disable CS1591, SA1600
    public ContentReader(
        IInputArguments args,
        IDataEncoderUtil dataEncoderUtil,
        IFileIOProxy fileIOProxy)
    : base(args, dataEncoderUtil, fileIOProxy) { }
#pragma warning restore CS1591, SA1600

    /// <summary>
    /// Reads in the next unread chunk of data from the input file, encodes it,
    /// and returns the encoded value.
    /// <para>The byte array to encode will be trimmed if the number of bytes remaining in the file is less
    /// than the ChunkByteSize.</para>
    /// </summary>
    /// <returns>A binary string representation of the next availabe ChunkByteSize from the input file.</returns>
    public string? ReadContentChunkFromFile() => RunIfNotDisposedWithResult(() =>
    {
        byte[] buffer = new byte[Arguments.ChunkByteSize];
        int read = Stream.Read(buffer, 0, buffer.Length);
        if (read == 0)
        {
            return null;
        }
        else if (read < Arguments.ChunkByteSize)
        {
            byte[] actual = new byte[read];
            Array.Copy(buffer, actual, read);
            buffer = actual;
        }

        return DataEncoderUtil.Encode(
            buffer,
            Arguments.Password,
            Arguments.UseCompression,
            Arguments.DummyCount,
            Arguments.RandomSeed,
            Arguments.AdditionalPasswordHashIterations);
    });

    /// <summary>
    /// Creates a new stream configured to read in the file that is to be encoded.
    /// </summary>
    /// <returns>A stream instance configured for reading from the FileToEncode.</returns>
    protected override IReadWriteStream InitializeStream()
        => FileIOProxy.OpenFileForRead(Arguments.FileToEncode);
}
