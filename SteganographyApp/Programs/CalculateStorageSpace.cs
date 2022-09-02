// <auto-generated/>
namespace SteganographyApp
{
    using System;
    using System.Collections.Immutable;
    using System.Numerics;

    using SixLabors.ImageSharp;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;

    [ProgramDescriptor("Calculate the amount of storage space a set of cover images will provide.")]
    internal sealed class CalculateStorageSpaceArguments : IArgumentConverter
    {
        [Argument("--coverImages", "-c", position: 1, helpText: "The images from which the amount of storage space available will be derived.")]
        public ImmutableArray<string> CoverImages = new ImmutableArray<string>();

        public IInputArguments ToCommonArguments()
        {
            return new CommonArguments()
            {
                CoverImages = CoverImages
            };
        }
    }

    internal sealed class CalculateStorageSpaceCommand : BaseCommand<CalculateStorageSpaceArguments>
    {

        public override string GetName() => "storage-space";

        /// <summary>
        /// Calculates the total available storage space of all the specified images.
        /// </summary>
        public override void Execute(CalculateStorageSpaceArguments args)
        {
            var arguments = args.ToCommonArguments();

            Console.WriteLine("Calculating storage space in {0} images.", arguments.CoverImages.Length);
            try
            {
                var availableSpace = CalculateNumberOfPixelsForImages(arguments.CoverImages) * Calculator.BitsPerPixel;

                Console.WriteLine("\nImages are able to store:");
                PrintSize(availableSpace);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured while calculating storage space: {0}", e.Message);
            }
        }

        /// <summary>
        /// Calculate the total number of pixels within all images.
        /// </summary>
        /// <param name="coverImages">The array of string paths to the images to check.</param>
        private static BigInteger CalculateNumberOfPixelsForImages(ImmutableArray<string> coverImages)
        {
            var progressTracker = ProgressTracker.CreateAndDisplay(coverImages.Length, "Calculating image storage space", "Completed calculating image storage space.");
            var count = new BigInteger(0);
            foreach (string imagePath in coverImages)
            {
                try
                {
                    using (var image = Image.Load(imagePath))
                    {
                        count += image.Width * image.Height;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not get the width and height for image: {imagePath}", e);
                }
                progressTracker.UpdateAndDisplayProgress();
            }
            return count;
        }

        /// <summary>
        /// Prints out the binary size of an encypted file or storage space in bits, bytes,
        /// megabytes and gigabytes.
        /// </summary>
        /// <param name="size">The size in bits to print out.</param>
        private static void PrintSize(BigInteger size)
        {
            Console.WriteLine("\t{0} bits", size);
            Console.WriteLine("\t{0} bytes", size / 8);
            Console.WriteLine("\t{0} KB", size / 8 / 1024);
            if ((size / 8) / 1024 / 1024 < 1)
            {
                Console.WriteLine("\t< 1 MB");
            }
            else
            {
                Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
            }
        }
    }

}