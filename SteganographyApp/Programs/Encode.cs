namespace SteganographyApp.Programs;

using System;
using System.Text.Json;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Arguments.Validation;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using SteganographyApp.Common.Logging;

#pragma warning disable SA1600, SA1402

[ProgramDescriptor(
    "Encode an input file and hide it within a series of cover images.",
    "dotnet SteganographyApp.dll encode -c *.png,*.webp -f input.zip -p testing -r 123 -d 300 -a 30 -l Debug")]
internal sealed class EncodeArguments : CryptFields
{
    [Argument("--file", "-f", helpText: "The path to the file to encode and write to the cover images.")]
    [Required]
    [IsFile]
    public string InputFile { get; set; } = string.Empty;

    [Argument(
        "--chunkByteSize",
        "-cs",
        helpText: "The number of bytes to read and encode from the input file during "
            + "each iteration.")]
    [InRange(1, int.MaxValue)]
    public int ChunkByteSize { get; set; } = 524_288;

    [Argument(
        "--compress",
        "-co",
        helpText: "If provided will compress the contents of the file after encryption.")]
    public bool EnableCompression { get; set; } = false;

    [Argument(
        "--twoBits",
        "-tb",
        helpText: "If true will store data in the least and second-least significant "
            + "bit rather than just the least significant.")]
    public bool TwoBits { get; set; } = false;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        var arguments = new CommonArguments
        {
            CoverImages = CoverImages,
            Password = Password,
            FileToEncode = InputFile,
            RandomSeed = RandomSeed,
            ChunkByteSize = ChunkByteSize,
            DummyCount = DummyCount,
            AdditionalPasswordHashIterations = AdditionalPasswordHashIterations,
            UseCompression = EnableCompression,
            BitsToUse = TwoBits ? 2 : 1,
        };
        ServiceContainer.GetLogger<EncodeArguments>()
            .Debug("Using input arguments: [{0}]", () => [JsonSerializer.Serialize(arguments)]);
        return arguments;
    }
}

internal sealed class EncodeCommand : Command<EncodeArguments>
{
    private readonly ILogger log = new LazyLogger<EncodeCommand>();

    public override string GetName() => "encode";

    public override void Execute(EncodeArguments args)
    {
        var arguments = args.ToCommonArguments();
        Console.WriteLine("Encoding File: {0}", arguments.FileToEncode);

        var utilities = new EncodingUtilities(arguments);

        Encode(utilities, arguments);

        Cleanup(utilities, arguments);
    }

    private void Encode(EncodingUtilities utilities, IInputArguments arguments)
    {
        ICalculator calculator = ServiceContainer.GetService<ICalculator>();
        using (var stream = utilities.ImageStore.OpenStream(StreamMode.Write))
        {
            log.Debug("Encoding file: [{0}]", arguments.FileToEncode);
            int startingPixel = calculator.CalculateRequiredBitsForContentTable(arguments.FileToEncode, arguments.ChunkByteSize);
            log.Debug("Content chunk table requires [{0}] bits of space to store.", startingPixel);
            stream.SeekToPixel(startingPixel);

            int requiredNumberOfWrites = calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);
            log.Debug("File requires [{0}] iterations to encode.", requiredNumberOfWrites);
            var progressTracker = ServiceContainer.CreateProgressTracker(requiredNumberOfWrites, "Encoding file contents", "All input file contents have been encoded.")
                .Display();

            using (var reader = ServiceContainer.CreateContentReader(arguments))
            {
                string? contentChunk = string.Empty;
                while (true)
                {
                    log.Debug("===== ===== ===== Begin Encoding Iteration ===== ===== =====");
                    contentChunk = reader.ReadContentChunkFromFile();
                    if (contentChunk == null)
                    {
                        break;
                    }

                    log.Debug("Processing chunk of [{0}] bits.", contentChunk.Length);
                    stream.WriteContentChunkToImage(contentChunk);
                    progressTracker.UpdateAndDisplayProgress();
                    log.Debug("===== ===== ===== End Encoding Iteration ===== ===== =====");
                }
            }
        }
    }

    private void Cleanup(EncodingUtilities utilities, IInputArguments arguments)
    {
        Console.WriteLine("Writing content chunk table.");
        using (var writer = new ChunkTableWriter(utilities.ImageStore))
        {
            writer.WriteContentChunkTable(utilities.TableTracker.GetContentTable());
        }

        Console.WriteLine("Encoding process complete.");
        log.Trace("Encoding process complete.");
        utilities.ImageTracker.PrintImagesUtilized();
    }
}

internal sealed class EncodingUtilities
{
    public EncodingUtilities(IInputArguments args)
    {
        ImageStore = ServiceContainer.CreateImageStore(args);
        TableTracker = new TableChunkTracker(ImageStore);
        ImageTracker = new ImageTracker(args, ImageStore);
    }

    public ImageStore ImageStore { get; }

    public TableChunkTracker TableTracker { get; }

    public ImageTracker ImageTracker { get; }
}

#pragma warning restore SA1600, SA1402