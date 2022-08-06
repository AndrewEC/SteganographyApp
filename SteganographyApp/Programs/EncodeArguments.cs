namespace SteganographyApp
{
    using System.Collections.Immutable;
    using System.Text.Json;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    [ProgramDescriptor("Encode a file to the specified cover images.")]
    public sealed class EncodeArguments : IArgumentConverter
    {
        [Argument("--coverImages", "-c", true, helpText: "The images where the input file will be encoded and written to.", parser: nameof(ParsePaths))]
        public ImmutableArray<string> coverImages;

        [Argument("--password", "-p", helpText: "The optional password used to encrypt the input file contents.")]
        public string password = string.Empty;

        [Argument("--file", "-f", true, helpText: "The path to the file to encode and write to the cover images.", parser: nameof(ParseFilePath))]
        public string inputFile = string.Empty;

        [Argument("--randomSeed", "-r", helpText: "The optional value to determine how the contents of the input file will be randomized before writing them.")]
        public string randomSeed = string.Empty;

        [Argument("--insertDummies", "-i", helpText: "Choose whether dummy bytes should be inserted into the file contents before being randomized.")]
        public bool insertDummies = false;

        [Argument("--chunkByteSize", "-cs", helpText: "The number of bytes to read and encode from the input file during each iteration.")]
        public int chunkByteSize = 131_072;

        [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
        public LogLevel logLevel = LogLevel.None;

        public static object ParsePaths(object? target, string value) => ImagePathParser.ParseImages(value);

        public static object ParseFilePath(object? target, string value) => ParserFunctions.ParseFilePath(value);

        public IInputArguments ToCommonArguments()
        {
            RootLogger.Instance.EnableLoggingAtLevel(logLevel);
            var arguments = new CommonArguments
            {
                CoverImages = coverImages,
                Password = password,
                FileToEncode = inputFile,
                RandomSeed = randomSeed,
                InsertDummies = insertDummies,
                ChunkByteSize = chunkByteSize,
                DummyCount = ParserFunctions.ParseDummyCount(insertDummies, coverImages, randomSeed)
            };
            Injector.LoggerFor<EncodeArguments>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(arguments) });
            return arguments;
        }
    }
}