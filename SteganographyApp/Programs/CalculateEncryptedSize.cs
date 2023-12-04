// <auto-generated/>
namespace SteganographyApp;

using System;
using System.Text.Json;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Arguments.Validation;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.Logging;

[ProgramDescriptor("Calculates the approximate size of an input file if it were to be encrypted.")]
internal sealed class CalculateEncryptedSizeArguments : IArgumentConverter
{
    [Argument("--password", "-p", helpText: "The optional password used to encrypt the input file contents. Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
    public string Password = string.Empty;

    [Argument("--file", "-f", true, helpText: "The path to the file to encode and write to the cover images.")]
    [IsFile()]
    public string InputFile = string.Empty;

    [Argument("--randomSeed", "-r", helpText: "The optional value to determine how the contents of the input file will be randomized before writing them. Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
    public string RandomSeed = string.Empty;

    [Argument("--dummyCount", "-d", helpText: "The number of dummy entries that should be inserted after compression and before randomization. Recommended value between 100 and 1,000. Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
    [InRange(0, int.MaxValue)]
    public int DummyCount = 0;

    [Argument("--chunkByteSize", "-cs", helpText: "The number of bytes to read and encode from the input file during each iteration.")]
    [InRange(1, int.MaxValue)]
    public int ChunkByteSize = 524_288;

    [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
    public LogLevel LogLevel = LogLevel.None;

    [Argument("--additionalHashes", "-a", helpText: "The number of additional times to has the password. Has no effect if no password is provided. Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
    [InRange(0, int.MaxValue)]
    public int AdditionalPasswordHashIterations = 0;

    [Argument("--compress", "-co", helpText: "If provided will compress the contents of the file after encryption.")]
    public bool EnableCompression = false;

    [Argument("--twoBits", "-tb", helpText: "If true will store data in the least and second-least significant bit rather than just the least significant.")]
    public bool TwoBits = false;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        var arguments = new CommonArguments
        {
            Password = Password,
            FileToEncode = InputFile,
            RandomSeed = RandomSeed,
            ChunkByteSize = ChunkByteSize,
            DummyCount = DummyCount,
            AdditionalPasswordHashIterations = AdditionalPasswordHashIterations,
            UseCompression = EnableCompression,
            BitsToUse = TwoBits ? 2 : 1,
        };
        Injector.LoggerFor<CalculateEncryptedSizeArguments>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(arguments) });
        return arguments;
    }
}

internal sealed class CalculateEncryptedSizeCommand : Command<CalculateEncryptedSizeArguments>
{
    public override string GetName() => "encrypted-size";

    public override void Execute(CalculateEncryptedSizeArguments args)
    {
        var arguments = args.ToCommonArguments();
        Console.WriteLine("Calculating encypted size of file {0}.", arguments.FileToEncode);
        try
        {
            int singleChunkSize = CalculateChunkLength(arguments);
            int numberOfChunks = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);

            int chunkTableSize = Calculator.CalculateRequiredBitsForContentTable(numberOfChunks);
            double size = ((double)singleChunkSize * (double)numberOfChunks) + (double)chunkTableSize;

            Console.WriteLine("Encrypted file size is:");
            PrintSize(size);

            Console.WriteLine("# of images required to store this file at common resolutions:");
            PrintComparison(size, arguments.BitsToUse);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occured while encoding file: {0}", e.Message);
        }
    }

    private static int CalculateChunkLength(IInputArguments arguments)
    {
        using (var contentReader = new ContentReader(arguments))
        {
            return contentReader.ReadContentChunkFromFile()!.Length;
        }
    }

    private static void PrintSize(double size)
    {
        Console.WriteLine("\t{0} bits", size);
        Console.WriteLine("\t{0} bytes", size / 8);
        Console.WriteLine("\t{0} KB", size / 8 / 1024);
        Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
    }

    private static void PrintComparison(double size, int bitsToUsePerPixel)
    {
        Console.WriteLine("\tAt 360p: \t{0}", size / (CommonResolutionStorageSpace.P360 * bitsToUsePerPixel));
        Console.WriteLine("\tAt 480p: \t{0}", size / (CommonResolutionStorageSpace.P480 * bitsToUsePerPixel));
        Console.WriteLine("\tAt 720p: \t{0}", size / (CommonResolutionStorageSpace.P720 * bitsToUsePerPixel));
        Console.WriteLine("\tAt 1080p: \t{0}", size / (CommonResolutionStorageSpace.P1080 * bitsToUsePerPixel));
        Console.WriteLine("\tAt 1440p: \t{0}", size / (CommonResolutionStorageSpace.P1440 * bitsToUsePerPixel));
        Console.WriteLine("\tAt 4K (2160p): \t{0}", size / (CommonResolutionStorageSpace.P2160 * bitsToUsePerPixel));
    }
}