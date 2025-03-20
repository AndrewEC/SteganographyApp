namespace SteganographyApp.Programs;

using System;
using System.Collections.Immutable;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Validation;
using SteganographyApp.Common.Logging;
using SteganographyApp.Common.Parsers;

#pragma warning disable SA1600, SA1402

internal abstract class LogFields
{
    [Argument(
        "--logLevel",
        "-l",
        helpText: "The log level to determine which logs will feed into the log file.")]
    public LogLevel LogLevel { get; set; } = LogLevel.None;
}

internal abstract class ImageFields : LogFields
{
    [Argument(
        "--coverImages",
        "-c",
        helpText: "The list of lossless images to operator on.",
        example: "*.png,*.webp",
        parser: nameof(ParseCoverImages))]
    public ImmutableArray<string> CoverImages { get; set; } = [];

    public static object ParseCoverImages(object target, string value)
        => ImagePathParser.ParseImages(value);
}

internal abstract class CryptFields : ImageFields
{
    [Argument(
        "--additionalHashes",
        "-a",
        helpText: "The number of additional times to hash the password. Has no effect "
            + "if no password is provided. Providing a question mark (?) as input "
            + "allows this parameter to be entered in an interactive mode where the "
            + "input will be captured but not displayed.")]
    [InRange(0, int.MaxValue)]
    public int AdditionalPasswordHashIterations { get; set; } = 0;

    [Argument(
        "--password",
        "-p",
        helpText: "The optional password used to encrypt the input file contents. "
            + "Providing a question mark (?) as input allows this parameter to be "
            + "entered in an interactive mode where the input will be captured but "
            + "not displayed.",
        parser: nameof(ParsePassword))]
    public string Password { get; set; } = string.Empty;

    [Argument(
        "--randomSeed",
        "-r",
        helpText: "The optional value to determine how the contents of the input file "
            + "will be randomized before writing them. Providing a question mark (?) as "
            + "input allows this parameter to be entered in an interactive mode where the "
            + "input will be captured but not displayed.",
        parser: nameof(ParseRandomSeed))]
    public string RandomSeed { get; set; } = string.Empty;

    [Argument(
        "--dummyCount",
        "-d",
        helpText: "The number of dummy entries that should be inserted after compression "
            + "and before randomization. Recommended value between 100 and 1,000. Providing "
            + "a question mark (?) as input allows this parameter to be entered in an "
            + "interactive mode where the input will be captured but not displayed.")]
    [InRange(0, int.MaxValue)]
    public int DummyCount { get; set; } = 0;

    public static object ParseRandomSeed(object target, string value)
        => SecureParser.ReadUserInput("Random Seed: ", value);

    public static object ParsePassword(object target, string value)
        => SecureParser.ReadUserInput("Password: ", value);

    public static object ParseAdditionalPasswordHashIterations(object target, string value)
        => Convert.ToInt32(SecureParser.ReadUserInput("Additional Hash Iterations: ", value));

    public static object ParseDummyCount(object target, string value)
        => Convert.ToInt32(SecureParser.ReadUserInput("Dummy Count: ", value));
}

#pragma warning restore SA1600, SA1402