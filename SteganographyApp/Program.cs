namespace SteganographyApp;

using System;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Parsers;

public class Program
{
    public static void Main(string[] args)
    {
#pragma warning disable SA1009
        CliProgram.Create(
            Commands.Group(
                Commands.Lazy<CleanCoverImagesCommand>(),
                Commands.Lazy<DecodeCommand>(),
                Commands.Lazy<EncodeCommand>(),
                Commands.Group(
                    "manage",
                    "Convert and verify images so they support encoding and decoding data.",
                    Commands.Lazy<ConvertImagesCommand>(),
                    Commands.Lazy<VerifyImagesCommand>()
                ),
                Commands.Group(
                    "calculate",
                    "Calculate the size of an encrypted file or the storage space of an image.",
                    Commands.Lazy<CalculateEncryptedSizeCommand>(),
                    Commands.Lazy<CalculateStorageSpaceCommand>()
                )
            )
        )
        .WithAdditionalParsers(
            AdditionalParserFunctionsProvider.Builder()
                .ForFieldNamed("CoverImages", (target, value) => ImagePathParser.ParseImages(value))
                .ForFieldNamed("Password", CreateSecureParser("Password: "))
                .ForFieldNamed("RandomSeed", CreateSecureParser("Random Seed: "))
                .ForFieldNamed("AdditionalPasswordHashIterations", CreateSecureIntParser("Additional Hashes: "))
                .ForFieldNamed("DummyCount", CreateSecureIntParser("Dummy Count: "))
                .Build()
        )
        .Execute(args);
#pragma warning restore SA1009
    }

    private static Func<object, string, object> CreateSecureParser(string prompt)
        => (target, value) => SecureParser.ReadUserInput(prompt, value);

    private static Func<object, string, object> CreateSecureIntParser(string prompt)
        => (target, value) => Convert.ToInt32(SecureParser.ReadUserInput(prompt, value));
}