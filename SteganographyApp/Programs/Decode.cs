// <auto-generated/>
namespace SteganographyApp
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Logging;

    [ProgramDescriptor(
        "Decode data from the specified cover images to the output file.",
        "dotnet SteganographyApp.dll decode -c *.png,*.webp -o output.zip -p testing -r 123 -d 300 -a 30 -l Debug"
    )]
    internal sealed class DecodeArguments : IArgumentConverter
    {
        [Argument(
            "--coverImages",
            "-c",
            true,
            helpText: "The images where the input file will be decoded from.\n"
                + " This parameter can be a comma delimited list of globs with the current directory as the root directory from which files will be matched.",
            example: "*.png,*.webp"
        )]
        public ImmutableArray<string> CoverImages = new ImmutableArray<string>();

        [Argument("--password", "-p", helpText: "The optional password used to decrypt the input file contents.\n Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
        public string Password = string.Empty;

        [Argument("--out", "-o", true, helpText: "The path to the file where the decoded contents will be written to.")]
        public string OutputFile = string.Empty;

        [Argument("--randomSeed", "-r", helpText: "The optional value to determine how the contents cover image contents will be decoded while before writing them to the output file.\n Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
        public string RandomSeed = string.Empty;

        [Argument("--dummyCount", "-d", helpText: "The number of dummy entries to be removed after randomization and before decryption. Recommended value between 100 and 1,000.\n Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
        public int DummyCount = 0;

        [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
        public LogLevel LogLevel = LogLevel.None;

        [Argument("--additionalHashes", "-a", helpText: "The number of additional times to has the password. Has no effect if no password is provided.\n Providing a question mark (?) as input allows this parameter to be entered in an interactive mode where the input will be captured but not displayed.")]
        public int AdditionalPasswordHashIterations = 0;

        [Argument("--compress", "-co", helpText: "If provided will decompress the contents of the file before decryption.")]
        public bool EnableCompression = false;

        [Argument("--twoBits", "-tb", helpText: "If true will store data in the least and second-least significant bit rather than just the least significant.")]
        public bool TwoBits = false;

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
            Injector.LoggerFor<DecodeArguments>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(arguments) });
            return arguments;
        }
    }

    internal sealed class DecodeCommand : Command<DecodeArguments>
    {
        private readonly ILogger log = new LazyLogger<DecodeCommand>();

        public override void Execute(DecodeArguments args)
        {
            var arguments = args.ToCommonArguments();
            Console.WriteLine("Decoding to File: {0}", arguments.DecodedOutputFile);
            log.Debug("Decoding to file: [{0}]", arguments.DecodedOutputFile);

            Decode(arguments);

            log.Trace("Decoding process completed.");
            Console.WriteLine("Decoding process complete.");
        }

        public override string GetName() => "Decode";

        private void Decode(IInputArguments arguments)
        {
            Console.WriteLine("Reading content chunk table.");
            var store = new ImageStore(arguments);
            using (var chunkTableReader = new ChunkTableReader(store, arguments))
            {
                using (var wrapper = store.CreateIOWrapper())
                {
                    var contentChunkTable = chunkTableReader.ReadContentChunkTable();
                    var tracker = ProgressTracker.CreateAndDisplay(contentChunkTable.Length, "Decoding file contents", "All input file contents have been decoded, completing last write to output file.");
                    log.Debug("Content chunk table contains [{0}] entries.", contentChunkTable.Length);

                    using (var writer = new ContentWriter(arguments))
                    {
                        foreach (int chunkLength in contentChunkTable)
                        {
                            log.Debug("===== ===== ===== Begin Decoding Iteration ===== ===== =====");
                            log.Debug("Processing chunk of [{0}] bits.", chunkLength);
                            string binary = wrapper.ReadContentChunkFromImage(chunkLength);
                            writer.WriteContentChunkToFile(binary);
                            tracker.UpdateAndDisplayProgress();
                            log.Debug("===== ===== ===== End Decoding Iteration ===== ===== =====");
                        }
                    }
                }
            }
        }
    }
}