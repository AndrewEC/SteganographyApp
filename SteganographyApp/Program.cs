namespace SteganographyApp
{
    using System;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;

    public class Program
    {
        public static void Main(string[] args)
        {
#pragma warning disable SA1009
            CliProgram.Create(
                Command.Group(
                    Command.Lazy<CleanCoverImagesCommand>(),
                    Command.Lazy<DecodeCommand>(),
                    Command.Lazy<EncodeCommand>(),
                    Command.Lazy<ConvertImagesCommand>(),
                    Command.Lazy<VerifyImagesCommand>(),
                    Command.Group(
                        "calculate",
                        Command.Lazy<CalculateEncryptedSizeCommand>(),
                        Command.Lazy<CalculateStorageSpaceCommand>()
                    )
                )
            )
            .WithParsers(
                AdditionalParsers.Builder()
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

        private static Func<object, string, object> CreateSecureParser(string prompt) => (target, value) => SecureParser.ReadUserInput(prompt, value);
        private static Func<object, string, object> CreateSecureIntParser(string prompt) => (target, value) => Convert.ToInt32(SecureParser.ReadUserInput(prompt, value));
    }
}