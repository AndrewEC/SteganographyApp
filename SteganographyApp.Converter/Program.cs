using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Converter
{
    class Program
    {

        private static readonly string PngMimeType = "image/png";

        static void Main(string[] args)
        {
            Console.WriteLine("\nSteganography Converter\n");
            if (Array.IndexOf(args, "--help") != -1 || Array.IndexOf(args, "-h") != -1)
            {
                PrintHelp();
                return;
            }

            var parser = new ArgumentParser();
            if(!parser.TryParse(args, out InputArguments arguments, PostValidation))
            {
                parser.PrintCommonErrorMessage();
                return;
            }
            
            ConvertImages(arguments);
        }

        /// <summary>
        /// Performs some validation once all the user inputted values have been parsed and individually
        /// validated.
        /// </summary>
        private static string PostValidation(InputArguments inputs)
        {
            if (inputs.EncodeOrDecode != EncodeDecodeAction.Convert)
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
        private static void ConvertImages(InputArguments args)
        {
            string[] images = args.CoverImages.Where(FilterImage).ToArray();
            Console.WriteLine("Converting {0} images.", images.Length);
            var tracker = new ProgressTracker(images.Length, "Converting images", "Finished converting all images");
            tracker.Display();

            foreach (string coverImage in images)
            {
                if (Image.DetectFormat(coverImage).DefaultMimeType == PngMimeType)
                {
                    continue;
                }

                var encoder = new PngEncoder();
                encoder.CompressionLevel = args.CompressionLevel;
                using(var image = Image.Load(coverImage))
                {
                    image.Save(AppendPngExtension(coverImage), encoder);
                }

                if (args.DeleteAfterConversion)
                {
                    File.Delete(coverImage);
                }

                tracker.TickAndDisplay();
            }
        }

        /// <summary>
        /// Filters out any images that already have the png format.
        /// </summary>
        private static bool FilterImage(string image)
        {
            return Image.DetectFormat(image).DefaultMimeType != PngMimeType;
        }

        /// <summary>
        /// Takes in the path to the specified image, stripts out the existing file extension
        /// and replaces it with a png extension.
        /// </summary>
        /// <param name="image">The path to the image being converted</param>
        private static string AppendPngExtension(string image)
        {
            int index = image.LastIndexOf('.');
            return string.Format("{0}.png", image.Substring(0, index));
        }

        /// <summary>
        /// Attempts to print the help info retrieved from the help.props file.
        /// </summary>
        private static void PrintHelp()
        {
            var parser = new HelpParser();
            if (!parser.TryParse(out HelpInfo info))
            {
                parser.PrintCommonErrorMessage();
                return;
            }

            Console.WriteLine("Steganography Converter Help\n");

            foreach (string message in info.GetMessagesFor(HelpItemSet.Converter))
            {
                Console.WriteLine("{0}\n", message);
            }
        }
    }
}
