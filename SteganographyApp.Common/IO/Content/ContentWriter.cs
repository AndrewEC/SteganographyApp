namespace SteganographyApp.Common.IO;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;

/// <summary>
/// Stream encapsulating class that decodes binary data and writes it
/// to an output file.
/// </summary>
/// <remarks>
/// Instantiates a new ContentWrite instance and sets the
/// args field value.
/// </remarks>
/// <param name="args">The user provided input arguments.</param>
public sealed class ContentWriter(IInputArguments args) : AbstractContentIO(args)
{
    /// <summary>
    /// Takes in an encrypted binary string, decyrypts it using the DataEncoderUtil
    /// and writes the resulting bytes to the output file.
    /// </summary>
    /// <param name="binary">The encrypted binary string read from the storage images.</param>
    public void WriteContentChunkToFile(string binary) => RunIfNotDisposed(() =>
    {
        byte[] decoded = Injector.Provide<IDataEncoderUtil>().Decode(binary, Args.Password, Args.UseCompression, Args.DummyCount, Args.RandomSeed, Args.AdditionalPasswordHashIterations);
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
        var fileIOProxy = Injector.Provide<IFileIOProxy>();
        if (fileIOProxy.IsExistingFile(Args.DecodedOutputFile))
        {
            fileIOProxy.Delete(Args.DecodedOutputFile);
        }
        return fileIOProxy.OpenFileForWrite(Args.DecodedOutputFile);
    }
}