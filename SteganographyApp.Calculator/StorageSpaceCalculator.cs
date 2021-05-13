using System;
using System.Collections.Immutable;
using System.Numerics;

using SixLabors.ImageSharp;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;

namespace SteganographyAppCalculator
{

    public static class StorageSpaceCalculator
    {

         /// <summary>
        /// Calculates the total available storage space of all the specified images.
        /// </summary>
        /// <param name="args">The InputArguments instance parsed from the user provided command
        /// line arguments.</param>
        public static void CalculateStorageSpace(IInputArguments args)
        {
            Console.WriteLine("Calculating storage space in {0} images.", args.CoverImages.Length);
            try
            {
                var availableSpace = CalculateNumberOfPixelsForImages(args.CoverImages) * Calculator.BitsPerPixel;

                Console.WriteLine("\nImages are able to store:");
                PrintSize(availableSpace);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured while calculating storage space: {0}", e.Message);
                if (args.PrintStack)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return;
            }
        }

        /// <summary>
        /// Calculate the total number of pixels within all images.
        /// </summary>
        /// <param name="coverImages">The array of string paths to the images to check.</param>
        private static BigInteger CalculateNumberOfPixelsForImages(ImmutableArray<string> coverImages)
        {
            var progressTracker = ProgressTracker.CreateAndDisplay(coverImages.Length,
                    "Calculating image storage space", "Completed calculating image storage space.");
            var count = new BigInteger();
            foreach (string imagePath in coverImages)
            {
                try
                {
                    using (var image = Image.Load(imagePath))
                    {
                        count += (image.Width * image.Height);
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
            if (size / 8 / 1024 / 1024 < 1) {
                Console.WriteLine("\t< 1 MB");
            } else{
                Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
            }
        }

    }

}