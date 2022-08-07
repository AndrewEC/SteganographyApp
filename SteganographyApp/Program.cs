namespace SteganographyApp
{
    using System;

    using SteganographyApp.Common.Arguments.Commands;
    using SteganographyApp.Decode;
    using SteganographyApp.Encode;

    public class Program
    {
        public static void Main(string[] args)
        {
#pragma warning disable SA1009
            Exception? exception = CliProgram.Create(
                Command.Group(
                    Command.Lazy<CleanCoverImagesCommand>(),
                    Command.From<EncodeArguments>("encode", args => Encoder.CreateAndEncode(args.ToCommonArguments())),
                    Command.From<DecodeArguments>("decode", args => Decoder.CreateAndDecode(args.ToCommonArguments())),
                    Command.Lazy<ConvertImagesCommand>(),
                    Command.Group(
                        "calculate",
                        Command.Lazy<CalculateEncryptedSizeCommand>(),
                        Command.Lazy<CalculateStorageSpaceCommand>()
                    )
                )
            ).TryExecute(args);
#pragma warning restore SA1009

            if (exception != null)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}