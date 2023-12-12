namespace SteganographyApp.Common;

using System;
using System.Text;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.Logging;

/// <summary>
/// Utility class to automate the process of cleaning data from a series of cover images.
/// <para>This "cleans" the image by writing random 1s and 0s to the least and second
/// least significant bits of each RGB channel of each pixel of each image.</para>
/// </summary>
/// <param name="arguments">The arguments to configure how many bits per colour channel
/// to overwrite and which images are to be cleaned.</param>
/// <param name="imageStore">The image store instance initialized with the same arguments
/// parameter as provided in this constructor.</param>
public sealed class ImageCleaner(IInputArguments arguments, ImageStore imageStore)
{
    private readonly IInputArguments arguments = arguments;
    private readonly ImageStore imageStore = imageStore;

    private readonly ILogger log = new LazyLogger<ImageCleaner>();

    /// <summary>
    /// Cleans all the cover images specified in the cover images array of the input arguments.
    /// </summary>
    public void CleanImages()
    {
        using (var cleaner = imageStore.CreateIOWrapper())
        {
            for (int i = 0; i < arguments.CoverImages.Length; i++)
            {
                IBasicImageInfo? currentImage = imageStore.CurrentImage;
                if (currentImage == null)
                {
                    return;
                }
                string randomBinary = GenerateBinaryString(currentImage, ComputeModifier(currentImage));
                log.Trace("Generated random binary string of: [{0}]", randomBinary);
                cleaner.WriteContentChunkToImage(randomBinary);
            }

            cleaner.EncodeComplete();
        }
    }

    private bool IsLastImage(IBasicImageInfo image) => arguments.CoverImages.IndexOf(image.Path) == arguments.CoverImages.Length - 1;

    private int ComputeModifier(IBasicImageInfo image)
    {
        if (!IsLastImage(image))
        {
            return 0;
        }
        return -1 * Calculator.MinimumBitsPerPixel * arguments.BitsToUse;
    }

    private string GenerateBinaryString(IBasicImageInfo image, int modifier)
    {
        long bitCount = Calculator.CalculateStorageSpaceOfImage(image.Width, image.Height, arguments.BitsToUse) + modifier;

        log.Debug("Generating binary string with a length of [{0}] for image [{1}]", bitCount, image.Path);

        var random = new Random();
        var builder = new StringBuilder();
        for (long i = 0; i < bitCount; i++)
        {
            builder.Append(random.Next(10) % 2 == 0 ? '0' : '1');
        }
        return builder.ToString();
    }
}