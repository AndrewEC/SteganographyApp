namespace SteganographyApp
{
    using System;

    using SteganographyApp.Common.Arguments.Commands;
    using SteganographyApp.Encode;
    using SteganographyApp.Decode;

    public class Program
    {
        public static void Main(string[] args)
        {
            Exception? exception = CliProgram.Create(
                Command.Group(
                    Command.Later<CleanCoverImagesCommand>(),
                    Command.From<EncodeArguments>("encode", args => Encoder.CreateAndEncode(args.ToCommonArguments())),
                    Command.From<DecodeArguments>("decode", args => Decoder.CreateAndDecode(args.ToCommonArguments())),
                    Command.Later<ConvertImagesCommand>(),
                    Command.Group(
                        "calculate",
                        Command.Later<CalculateEncryptedSizeCommand>(),
                        Command.Later<CalculateStorageSpaceCommand>()
                    )
                )
            ).TryExecute(args);

            if (exception != null)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}