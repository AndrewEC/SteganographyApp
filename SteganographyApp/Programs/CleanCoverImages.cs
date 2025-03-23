namespace SteganographyApp;

using System;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;
using SteganographyApp.Programs;

#pragma warning disable SA1600, SA1402

[ProgramDescriptor("Cleans the LSBs of each pixel colour of the specified cover images.")]
internal sealed class CleanArguments : ImageFields
{
    [Argument(
        "--twoBits",
        "-tb",
        helpText: "Tells the app to clean data in the least and second-least "
            + "significant bit rather than just the least significant.")]
    public bool TwoBits { get; set; } = false;

    public IInputArguments ToCommonArguments()
    {
        RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
        return new CommonArguments
        {
            CoverImages = CoverImages,
            BitsToUse = TwoBits ? 2 : 1,
        };
    }
}

internal sealed class CleanCoverImagesCommand : Command<CleanArguments>
{
    public override void Execute(CleanArguments args)
    {
        Console.WriteLine("Cleaning data from the following images:");
        foreach (string path in args.CoverImages)
        {
            Console.WriteLine($"\t{path}");
        }

        IInputArguments arguments = args.ToCommonArguments();
        var tracker = ServiceContainer.CreateProgressTracker(
            args.CoverImages.Length, "Cleaning image LSB data", "Finished cleaning all images.")
            .Display();
        var store = ServiceContainer.CreateImageStore(arguments);
        store.OnNextImageLoaded += (sender, eventArg) =>
        {
            tracker.UpdateAndDisplayProgress();
        };
        ServiceContainer.CreateImageCleaner(arguments, store).CleanImages();
    }

    public override string GetName() => "clean";
}

#pragma warning restore SA1600, SA1402