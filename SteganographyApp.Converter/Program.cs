namespace SteganographyApp.Converter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

    public class Program
    {
        private const string WebpMimeType = "image/webp";
        private const string PngMimeType = "image/png";

        public static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography Converter\n");
            if (Checks.WasHelpRequested(args))
            {
                PrintHelp();
                return;
            }

            var parser = new ArgumentParser();
            if (!parser.TryParse(args, out IInputArguments? arguments, PostValidation))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            ConvertImagesToPng(arguments!);
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string? PostValidation(IInputArguments inputs) => Checks.IsNullOrEmpty(inputs.CoverImages) ?
            "At least one image must be provided to convert."
            : null;

        /// <summary>
        /// Converts all of the images to a PNG format and will optionally delete
        /// the original images after convertion if the delete option was specified
        /// by the user.
        /// </summary>
        private static void ConvertImagesToPng(IInputArguments arguments)
        {
            string[] lossyImages = arguments.CoverImages.Where(image => FilterOutUnconvertableImages(image, arguments.ImageFormat)).ToArray();
            if (lossyImages.Length == 0)
            {
                Console.WriteLine("All images found are png images and will not be converted.");
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
        /// Filters out any images that already have the png format.
        /// </summary>
        private static bool FilterOutUnconvertableImages(string image, ImageFormat desiredImageFormat)
        {
            string imageType = Image.DetectFormat(image).DefaultMimeType;
            return !((desiredImageFormat == ImageFormat.Png && imageType == PngMimeType) || (desiredImageFormat == ImageFormat.Webp && imageType == WebpMimeType));
        }

        /// <summary>
        /// Takes in the path to the specified image, stripts out the existing file extension
        /// and replaces it with a png extension.
        /// </summary>
        /// <param name="image">The path to the image being converted</param>
        private static string ReplaceFileExtension(string image, IInputArguments arguments)
        {
            int index = image.LastIndexOf('.');
            string extension = arguments.ImageFormat.ToString().ToLower();
            return $"{image.Substring(0, index)}.{extension}";
        }

        /// <summary>
        /// Attempts to print the help info retrieved from the help.props file.
        /// </summary>
        private static void PrintHelp()
        {
            var parser = new HelpParser();
            if (!parser.TryParseHelpFile(out HelpInfo info))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            Console.WriteLine("Steganography Converter Help\n");

            foreach (string message in info.GetHelpMessagesFor(HelpItemSet.Converter))
            {
                Console.WriteLine("{0}\n", message);
            }
        }
    }
}
