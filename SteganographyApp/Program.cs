namespace SteganographyApp;

using System;

using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Programs;

#pragma warning disable SA1600, SA1402, SA1009

public class Program
{
    public static void Main(string[] args)
    {
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
        .Execute(args);
    }
}

#pragma warning restore SA1600, SA1402, SA1009