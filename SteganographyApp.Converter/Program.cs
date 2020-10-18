using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Converter
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography Converter\n");
            if (Checks.WasHelpRequested(args))
            {
                PrintHelp();
                return;
            }

            var parser = new ArgumentParser();
            if(!parser.TryParse(args, out IInputArguments arguments, PostValidation))
            {
                parser.PrintCommonErrorMessage();
                return;
            }
            
            ConvertImagesToPng(arguments);
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string PostValidation(IInputArguments inputs)
        {
            if (inputs.EncodeOrDecode != ActionEnum.Convert)
            {
                return "The converter utility only supports the Convert action.";
            }
            if (Checks.IsNullOrEmpty(inputs.CoverImages))
            {
                return "At least one image must be provided to convert.";
            }
            return null;
        }

        /// <summary>
        /// Converts all of the images to a PNG format and will optionally delete
        /// the original images after convertion if the delete option was specified
        /// by the user.
        /// </summary>
        private static void ConvertImagesToPng(IInputArguments args)
        {
            int imageCount = args.CoverImages.Length;
            if (imageCount == 0)
            {
                Console.WriteLine("All images found are png images and will not be converted.");
                return;
            }

            Console.WriteLine("Converting {0} images.", imageCount);
            var tracker = ProgressTracker.CreateAndDisplay(imageCount, "Converting images",
                "Finished converting all images");

            var failures = new List<string>();

            var encoder = new PngEncoder();
            encoder.CompressionLevel = args.CompressionLevel;
            foreach (string coverImage in args.CoverImages)
            {
                string result = TrySaveImage(coverImage, encoder);
                if (!Checks.IsNullOrEmpty(result))
                {
                    failures.Add($"{coverImage}: {result}");
                    tracker.UpdateAndDisplayProgress();
                    continue;
                }
                else
                {
                    if (args.DeleteAfterConversion)
                    {
                        File.Delete(coverImage);
                    }
                }

                tracker.UpdateAndDisplayProgress();
            }

            PrintFailures(failures);
        }

        private static string TrySaveImage(string coverImage, PngEncoder encoder)
        {
            try
            {
                using (var image = Image.Load(coverImage))
                {
                    image.Save(ReplaceFileExtension(coverImage), encoder);
                }
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
        /// Takes in the path to the specified image, stripts out the existing file extension
        /// and replaces it with a png extension.
        /// </summary>
        /// <param name="image">The path to the image being converted</param>
        private static string ReplaceFileExtension(string image)
        {
            int index = image.LastIndexOf('.');
            return $"{image.Substring(0, index)}.png";
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
