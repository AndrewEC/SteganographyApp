// <auto-generated/>
#nullable enable
namespace SteganographyApp;


using System;
using System.Collections.Immutable;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.Logging;

[ProgramDescriptor("Cleans the LSBs of each pixel colour of the specified cover images.")]
internal sealed class CleanArguments : IArgumentConverter
{
    [Argument(
        "CoverImages",
        position: 1,
        helpText: "The list of images whose pixel colour LSBs need to be cleaned."
            + " This parameter can be a comma delimited list of globs with the current directory as the root directory from which files will be matched.",
        example: "*.png,*.webp"
    )]
    public ImmutableArray<string> CoverImages = [];

    [Argument("--twoBits", "-tb", helpText: "Tells the app to clean data in the least and second-least significant bit rather than just the least significant.")]
    public bool TwoBits = false;

    [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
    public LogLevel LogLevel = LogLevel.None;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        return new CommonArguments
        {
            CoverImages = this.CoverImages,
            BitsToUse = TwoBits ? 2 : 1,
        };
    }
}

internal sealed class CleanCoverImagesCommand : Command<CleanArguments>
{
    public override void Execute(CleanArguments args)
    {
        Injector.LoggerFor<CleanCoverImagesCommand>().Trace("Cleaning image LSBs");

        Console.WriteLine("Cleaning data from the following images:");
        foreach (string path in args.CoverImages)
        {
            Console.WriteLine($"\t{path}");
        }

        IInputArguments arguments = args.ToCommonArguments();
        var tracker = ProgressTracker.CreateAndDisplay(args.CoverImages.Length, "Cleaning image LSB data", "Finished cleaning all images.");
        var store = new ImageStore(arguments);
        store.OnNextImageLoaded += (object? sender, NextImageLoadedEventArgs eventArg) =>
        {
            tracker.UpdateAndDisplayProgress();
        };
        new ImageCleaner(arguments, store).CleanImages();
    }

    public override string GetName() => "clean";
}