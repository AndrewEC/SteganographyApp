namespace SteganographyApp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;

    [ProgramDescriptor("Convert a set of images to either a webp or png format.")]
    internal sealed class ConvertArguments : IArgumentConverter
    {
        [Argument("--coverImages", "-c", true, helpText: "The images to be converted into a png or webp format.", parser: nameof(ParsePaths))]
        public ImmutableArray<string> coverImages;

        [Argument("--imageFormat", "-i", helpText: "The format the images should be converted to.")]
        public ImageFormat imageFormat = ImageFormat.Png;

        [Argument("--deleteOriginals", "-d", helpText: "Specify whether the original image files should be deleted after conversion.")]
        public bool deleteOriginals = false;

        public static object ParsePaths(object? target, string value) => ImagePathParser.ParseImages(value);

        public IInputArguments ToCommonArguments()
        {
            return new CommonArguments
            {
                CoverImages = coverImages,
                ImageFormat = imageFormat,
                DeleteAfterConversion = deleteOriginals
            };
        }
    }

    internal sealed class ConvertImagesCommand : BaseCommand<ConvertArguments>
    {
        private const string WebpMimeType = "image/webp";
        private const string PngMimeType = "image/png";

        public override string GetName() => "convert";

        public override void Execute(ConvertArguments args)
        {
            var arguments = args.ToCommonArguments();
            string[] lossyImages = arguments.CoverImages.Where(image => FilterOutUnconvertableImages(image, arguments.ImageFormat)).ToArray();
            if (lossyImages.Length == 0)
            {
                Console.WriteLine("All images found are already convert and will not be converted again.");
                return;
            }

            Console.WriteLine("Converting [{0}] images.", lossyImages.Length);
            var tracker = ProgressTracker.CreateAndDisplay(lossyImages.Length, "Converting images", "Finished converting all images");

            var failures = new List<string>();

            var encoder = Injector.Provide<IEncoderProvider>().GetEncoder(arguments.ImageFormat);
            foreach (string coverImage in lossyImages)
            {
                string? result = TrySaveImage(coverImage, encoder, arguments);
                if (!string.IsNullOrEmpty(result))
                {
                    failures.Add($"{coverImage}: {result}");
                    tracker.UpdateAndDisplayProgress();
                    continue;
                }
                if (arguments.DeleteAfterConversion)
                {
                    File.Delete(coverImage);
                }

                tracker.UpdateAndDisplayProgress();
            }

            PrintFailures(failures);
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string? PostValidation(IInputArguments inputs) => Checks.IsNullOrEmpty(inputs.CoverImages) ?
            "At least one image must be provided to convert."
            : null;

        private static string? TrySaveImage(string coverImage, IImageEncoder encoder, IInputArguments arguments)
        {
            try
            {
                string updatedPath = ReplaceFileExtension(coverImage, arguments);
                using (var image = Image.Load(coverImage))
                {
                    image.Save(updatedPath, encoder);
                }
                Console.WriteLine($"Saved image [{coverImage}] to [{updatedPath}]");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return null;
        }

        private static void PrintFailures(List<string> failures)
        {
            if (failures.Count == 0)
            {
                return;
            }
            Console.WriteLine("\nOne or more of the specified images could not be converted.");
            Console.WriteLine("Failed to convert: ");
            foreach (string failure in failures)
            {
                Console.WriteLine($"\t{failure}");
            }
        }

        /// <summary>
        /// Filters out any images that are already in the desired format.
        /// </summary>
        private static bool FilterOutUnconvertableImages(string image, ImageFormat desiredImageFormat)
        {
            string imageType = Image.DetectFormat(image).DefaultMimeType;
            return !((desiredImageFormat == ImageFormat.Png && imageType == PngMimeType) || (desiredImageFormat == ImageFormat.Webp && imageType == WebpMimeType));
        }

        /// <summary>
        /// Takes in the path to the specified image, strips out the existing file extension and replaces it with the extension appropriate
        /// for the output image format.
        /// </summary>
        /// <param name="image">The path to the image being converted</param>
        private static string ReplaceFileExtension(string image, IInputArguments arguments)
        {
            int index = image.LastIndexOf('.');
            string extension = arguments.ImageFormat.ToString().ToLower();
            return $"{image.Substring(0, index)}.{extension}";
        }
    }
}