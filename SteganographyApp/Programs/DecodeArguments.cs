namespace SteganographyApp
{
    using System.Collections.Immutable;
    using System.Text.Json;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    [ProgramDescriptor("Decode data from the specified cover images to the output file.")]
    public sealed class DecodeArguments : IArgumentConverter
    {
        [Argument("--coverImages", "-c", true, helpText: "The images where the input file will be decoded from.", parser: nameof(ParsePaths))]
        public ImmutableArray<string> coverImages;

        [Argument("--password", "-p", helpText: "The optional password used to decrypt the input file contents.")]
        public string password = string.Empty;

        [Argument("--out", "-o", true, helpText: "The path to the file where the decoded contents will be written to.")]
        public string outputFile = string.Empty;

        [Argument("--randomSeed", "-r", helpText: "The optional value to determine how the contents cover image contents will be decoded while before writing them to the output file.")]
        public string randomSeed = string.Empty;

        [Argument("--insertDummies", "-i", helpText: "Choose whether dummy values need to be removed from the encoded cover image contents.")]
        public bool insertDummies = false;

        [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
        public LogLevel logLevel = LogLevel.None;

        public static object ParsePaths(object? target, string value) => ImagePathParser.ParseImages(value);

        public IInputArguments ToCommonArguments()
        {
            RootLogger.Instance.EnableLoggingAtLevel(logLevel);
            var arguments = new CommonArguments
            {
                CoverImages = coverImages,
                Password = password,
                DecodedOutputFile = outputFile,
                RandomSeed = randomSeed,
                InsertDummies = insertDummies,
                DummyCount = ParserFunctions.ParseDummyCount(insertDummies, coverImages, randomSeed)
            };
            Injector.LoggerFor<EncodeArguments>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(arguments) });
            return arguments;
        }
    }
}