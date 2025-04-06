namespace SteganographyApp.Programs;

using System;
using System.Collections.Immutable;
using System.Numerics;

using SixLabors.ImageSharp;

using SteganographyApp.Common;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Commands;
using SteganographyApp.Common.Injection;

#pragma warning disable SA1600, SA1402

[ProgramDescriptor("Calculate the amount of storage space a set of cover images will provide.")]
internal sealed class CalculateStorageSpaceArguments : ImageFields
{
    [Argument(
        "--twoBits",
        "-tb",
        helpText: "If true will store data in the least and second-least significant "
            + "bit rather than just the least significant.")]
    public bool TwoBits { get; set; } = false;

    public IInputArguments ToCommonArguments()
    {
        return new CommonArguments()
        {
            CoverImages = CoverImages,
            BitsToUse = TwoBits ? 2 : 1,
        };
    }
}

internal sealed class CalculateStorageSpaceCommand : Command<CalculateStorageSpaceArguments>
{
    public override string GetName() => "storage-space";

    public override void Execute(CalculateStorageSpaceArguments args)
    {
        var arguments = args.ToCommonArguments();

        Console.WriteLine("Calculating storage space in {0} images.", arguments.CoverImages.Length);
        try
        {
            var availableSpace = CalculateNumberOfPixelsForImages(arguments.CoverImages, arguments);

            Console.WriteLine("Images are able to store:");
            PrintSize(availableSpace);
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception occured while calculating storage space: {0}", e.Message);
        }
    }

    private static BigInteger CalculateNumberOfPixelsForImages(ImmutableArray<string> coverImages, IInputArguments arguments)
    {
        ICalculator calculator = ServiceContainer.GetService<ICalculator>();
        var progressTracker = ServiceContainer.CreateProgressTracker(coverImages.Length, "Calculating image storage space", "Completed calculating image storage space.")
            .Display();
        var count = new BigInteger(0);
        foreach (string imagePath in coverImages)
        {
            try
            {
                using (var image = Image.Load(imagePath))
                {
                    count += calculator.CalculateStorageSpaceOfImage(image.Width, image.Height, arguments.BitsToUse);
                }
            }
            catch (Exception e)
            {
#pragma warning disable CA2201
                throw new Exception($"Could not get the width and height for image: {imagePath}", e);
#pragma warning restore CA2201
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
        if (size / 8 / 1024 / 1024 < 1)
        {
            Console.WriteLine("\t< 1 MB");
        }
        else
        {
            Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
        }
    }
}

#pragma warning disable SA1600, SA1402