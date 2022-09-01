namespace SteganographyApp
{
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
                    Command.Group(
                        "calculate",
                        Command.Lazy<CalculateEncryptedSizeCommand>(),
                        Command.Lazy<CalculateStorageSpaceCommand>()
                    )
                )
            )
            .WithParsers(AdditionalParsers.ForFieldName("CoverImages", (target, value) => ImagePathParser.ParseImages(value)))
            .Execute(args);
#pragma warning restore SA1009
        }
    }
}