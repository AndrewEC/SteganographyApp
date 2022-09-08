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
        [Argument("CoverImages", position: 1, helpText: "The images from which the amount of storage space available will be derived.\n"
            + " This parameter can be a comma delimited list of globs with the current directory as the root directory from which files will be matched.")]
        public ImmutableArray<string> CoverImages = new ImmutableArray<string>();

        [Argument("--twoBits", "-tb", helpText: "If true will store data in the least and second-least significant bit rather than just the least significant.")]
        public bool TwoBits = false;

        public IInputArguments ToCommonArguments()
        {
            return new CommonArguments()
            {
                CoverImages = CoverImages,
                BitsToUse = TwoBits ? 2 : 1,
            };
        }
    }

    internal sealed class CalculateStorageSpaceCommand : BaseCommand<CalculateStorageSpaceArguments>
    {
        public override string GetName() => "storage-space";

        public override void Execute(CalculateStorageSpaceArguments args)
        {
            var arguments = args.ToCommonArguments();

            Console.WriteLine("Calculating storage space in {0} images.", arguments.CoverImages.Length);
            try
            {
                var availableSpace = CalculateNumberOfPixelsForImages(arguments.CoverImages) * (Calculator.BitsPerPixel * arguments.BitsToUse);

                Console.WriteLine("\nImages are able to store:");
                PrintSize(availableSpace);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured while calculating storage space: {0}", e.Message);
            }
        }

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