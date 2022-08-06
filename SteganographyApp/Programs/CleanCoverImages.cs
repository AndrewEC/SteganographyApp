namespace SteganographyApp
{
    using System.Collections.Immutable;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;

    [ProgramDescriptor("Cleans the LSBs of each pixel colour of the specified cover images.")]
    internal sealed class CleanArguments : IArgumentConverter
    {
        [Argument("--coverImages", "-c", true, helpText: "The list of images whose pixel colour LSBs need to be cleaned", parser: nameof(ParsePaths))]
        public ImmutableArray<string> CoverImages { get; set; }

        public static object ParsePaths(object? target, string value) => ImagePathParser.ParseImages(value);

        public IInputArguments ToCommonArguments()
        {
            return new CommonArguments
            {
                CoverImages = this.CoverImages
            };
        }
    }

    internal sealed class CleanCoverImagesCommand : BaseCommand<CleanArguments>
    {
        public override void Execute(CleanArguments args)
        {
            Injector.LoggerFor<CleanCoverImagesCommand>().Trace("Cleaning image LSBs");
            var tracker = ProgressTracker.CreateAndDisplay(args.CoverImages.Length, "Cleaning image LSB data", "Finished cleaning all images.");
            var store = new ImageStore(args.ToCommonArguments());
            store.OnNextImageLoaded += (object? sender, NextImageLoadedEventArgs eventArg) =>
            {
                tracker.UpdateAndDisplayProgress();
            };
            store.CleanImageLSBs();
        }

        public override string GetName() => "clean";
    }

}