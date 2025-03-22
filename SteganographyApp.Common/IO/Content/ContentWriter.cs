namespace SteganographyApp.Common.IO.Content;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Stream encapsulating class that decodes binary data and writes it
/// to an output file.
/// </summary>
public sealed class ContentWriter(
    IInputArguments arguments,
    IDataEncoderUtil dataEncoderUtil,
    IFileIOProxy fileIOProxy) : AbstractContentIO(arguments, dataEncoderUtil, fileIOProxy)
{
    /// <summary>
    /// Takes in an encrypted binary string, decyrypts it using the DataEncoderUtil
    /// and writes the resulting bytes to the output file.
    /// </summary>
    /// <param name="binary">The encrypted binary string read from the storage images.</param>
    public void WriteContentChunkToFile(string binary) => RunIfNotDisposed(() =>
    {
        byte[] decoded = DataEncoderUtil.Decode(
            binary,
            Arguments.Password,
            Arguments.UseCompression,
            Arguments.DummyCount,
            Arguments.RandomSeed,
            Arguments.AdditionalPasswordHashIterations);

        Stream.Write(decoded, 0, decoded.Length);
        Stream.Flush();
    });

    /// <summary>
    /// Creates a new stream configured to write to the target file location as speicified in the
    /// DecodedOutputFile argument.
    /// </summary>
    /// <returns>A read write stream configured to write to the DecodedOutputFile.</returns>
    protected override IReadWriteStream InitializeStream()
    {
        if (FileIOProxy.IsExistingFile(Arguments.DecodedOutputFile))
        {
            FileIOProxy.Delete(Arguments.DecodedOutputFile);
        }

        return FileIOProxy.OpenFileForWrite(Arguments.DecodedOutputFile);
    }
}