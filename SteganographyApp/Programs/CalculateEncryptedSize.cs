namespace SteganographyApp.Programs;

using System;
using System.Text.Json;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Arguments.Validation;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

#pragma warning disable SA1600, SA1402

[ProgramDescriptor("Calculates the approximate size of an input file if it were to be encrypted.")]
internal sealed class CalculateEncryptedSizeArguments : CryptFields
{
    [Argument(
        "--file",
        "-f",
        helpText: "The path to the file to encode and write to the cover images.")]
    [Required]
    [IsFile]
    public string InputFile { get; set; } = string.Empty;

    [Argument(
        "--chunkByteSize",
        "-cs",
        helpText: "The number of bytes to read and encode from the input file during each iteration.")]
    [InRange(1, int.MaxValue)]
    public int ChunkByteSize { get; set; } = 524_288;

    [Argument("--compress", "-co", helpText: "If provided will compress the contents of the file after encryption.")]
    public bool EnableCompression { get; set; } = false;

    [Argument("--twoBits", "-tb", helpText: "If true will store data in the least and second-least significant bit rather than just the least significant.")]
    public bool TwoBits { get; set; } = false;

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
        ServiceContainer.GetLogger<CalculateEncryptedSizeArguments>()
            .Debug("Using input arguments: [{0}]", () => [JsonSerializer.Serialize(arguments)]);
        return arguments;
    }
}

internal sealed class CalculateEncryptedSizeCommand : Command<CalculateEncryptedSizeArguments>
{
    private readonly ICalculator calculator;

    public CalculateEncryptedSizeCommand()
    {
        calculator = ServiceContainer.GetService<ICalculator>();
    }

    public override string GetName() => "encrypted-size";

    public override void Execute(CalculateEncryptedSizeArguments args)
    {
        var arguments = args.ToCommonArguments();
        Console.WriteLine("Calculating encypted size of file {0}.", arguments.FileToEncode);
        try
        {
            int singleChunkSize = CalculateChunkLength(arguments);
            int numberOfChunks = calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);

            int chunkTableSize = calculator.CalculateRequiredBitsForContentTable(numberOfChunks);
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
        using (var contentReader = ServiceContainer.CreateContentReader(arguments))
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

    private void PrintComparison(double size, int bitsToUse)
    {
        foreach (CommonResolutions resolution in Enum.GetValues(typeof(CommonResolutions)))
        {
            string name = Resolution.ToDisplayName(resolution);
            (int width, int height) = Resolution.Dimensions[resolution];
            Console.WriteLine("\tAt {0}: \t{1}", name, size / calculator.CalculateStorageSpaceOfImage(width, height, bitsToUse));
        }
    }
}

#pragma warning disable SA1600, SA1402