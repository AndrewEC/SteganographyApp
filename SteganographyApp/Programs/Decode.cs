namespace SteganographyApp.Programs;

using System;
using System.Collections.Immutable;
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
    "Decode data from the specified cover images to the output file.",
    "dotnet SteganographyApp.dll decode -c *.png,*.webp -o output.zip -p testing -r 123 -d 300 -a 30 -l Debug")]
internal sealed class DecodeArguments : CryptFields
{
    [Argument(
        "--out",
        "-o",
        helpText: "The path to the file where the decoded contents will be written to.")]
    [Required]
    public string OutputFile { get; set; } = string.Empty;

    [Argument(
        "--compress",
        "-co",
        helpText: "If provided will decompress the contents of the file before decryption.")]
    public bool EnableCompression { get; set; } = false;

    [Argument(
        "--twoBits",
        "-tb",
        helpText: "If true will store data in the least and second-least significant bit "
            + "rather than just the least significant.")]
    public bool TwoBits { get; set; } = false;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        var arguments = new CommonArguments
        {
            CoverImages = CoverImages,
            Password = Password,
            DecodedOutputFile = OutputFile,
            RandomSeed = RandomSeed,
            DummyCount = DummyCount,
            AdditionalPasswordHashIterations = AdditionalPasswordHashIterations,
            UseCompression = EnableCompression,
            BitsToUse = TwoBits ? 2 : 1,
        };
        ServiceContainer.GetLogger<DecodeArguments>()
            .Debug("Using input arguments: [{0}]", () => [JsonSerializer.Serialize(arguments)]);
        return arguments;
    }
}

internal sealed class DecodeCommand : Command<DecodeArguments>
{
    private readonly LazyLogger<DecodeCommand> log = new();

    public override void Execute(DecodeArguments args)
    {
        var arguments = args.ToCommonArguments();
        Console.WriteLine("Decoding to File: {0}", arguments.DecodedOutputFile);
        log.Debug("Decoding to file: [{0}]", arguments.DecodedOutputFile);

        Decode(arguments);

        log.Trace("Decoding process completed.");
        Console.WriteLine("Decoding process complete.");
    }

    public override string GetName() => "decode";

    private void Decode(IInputArguments arguments)
    {
        Console.WriteLine("Reading content chunk table.");
        using (var stream = ServiceContainer.CreateImageStore(arguments).OpenStream(StreamMode.Read))
        {
            ImmutableArray<int> contentChunkTable = ChunkTableReader.ReadContentChunkTable(stream);
            var tracker = ServiceContainer.CreateProgressTracker(contentChunkTable.Length, "Decoding file contents", "All input file contents have been decoded, completing last write to output file.")
                .Display();
            log.Debug("Content chunk table contains [{0}] entries.", contentChunkTable.Length);

            using (var writer = ServiceContainer.CreateContentWriter(arguments))
            {
                foreach (int chunkLength in contentChunkTable)
                {
                    log.Debug("===== ===== ===== Begin Decoding Iteration ===== ===== =====");
                    log.Debug("Processing chunk of [{0}] bits.", chunkLength);
                    string binary = stream.ReadContentChunkFromImage(chunkLength);
                    writer.WriteContentChunkToFile(binary);
                    tracker.UpdateAndDisplayProgress();
                    log.Debug("===== ===== ===== End Decoding Iteration ===== ===== =====");
                }
            }
        }
    }
}

#pragma warning restore SA1600, SA1402