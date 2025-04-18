namespace SteganographyApp.Common;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using SteganographyApp.Common.Logging;

/// <summary>
/// Utility to automate the process of cleaning data from a series of cover images.
/// <para>This "cleans" the image by writing random 1s and 0s to the least and second
/// least significant bits of each RGB channel of each pixel of each image.</para>
/// </summary>
public interface IImageCleaner
{
    /// <summary>
    /// Cleans all the cover images specified in the cover images array of the input arguments.
    /// </summary>
    void CleanImages();
}

/// <inheritdoc/>
public sealed class ImageCleaner(
    IInputArguments arguments,
    IImageStore imageStore,
    ICalculator calculator) : IImageCleaner
{
    private const int MaxTableEntries = 5;
    private const int MinTableEntries = 1;
    private const int MaxEntryLength = 10_000;
    private const int MinEntryLength = 100;

    private readonly IInputArguments arguments = arguments;
    private readonly IImageStore imageStore = imageStore;

    private readonly LazyLogger<ImageCleaner> log = new();

    /// <inheritdoc/>
    public void CleanImages()
    {
        FillImagesWithRandomData();
        WriteDummyTable();
    }

    private void FillImagesWithRandomData()
    {
        using (var stream = imageStore.OpenStream(StreamMode.Write))
        {
            for (int i = 0; i < arguments.CoverImages.Length; i++)
            {
                IBasicImageInfo? currentImage = imageStore.CurrentImage;
                if (currentImage == null)
                {
                    return;
                }

                string randomBinary = GenerateBinaryString(currentImage);
                log.Trace("Generated random binary string of: [{0}]", randomBinary);
                stream.WriteContentChunkToImage(randomBinary);
            }
        }
    }

    private void WriteDummyTable()
    {
        var random = new Random();

        int tableEntries = (int)random.NextInt64(MinTableEntries, MaxTableEntries);
        ImmutableArray<int> entries = Enumerable.Range(0, tableEntries)
            .Select(i => (int)random.NextInt64(MinEntryLength, MaxEntryLength))
            .ToImmutableArray();

        using (var writer = new ChunkTableWriter(imageStore))
        {
            writer.WriteContentChunkTable(entries);
        }
    }

    private bool IsLastImage(IBasicImageInfo image)
        => arguments.CoverImages.IndexOf(image.Path) == arguments.CoverImages.Length - 1;

    /// <summary>
    /// The modifier is computed to ensure that the last pixel of the last image is not written to.
    /// This is to prevent a potential issue in which if all the pixels of the last image are written
    /// to the stream will try to advance to the next image which will produce an error because there
    /// are no more remaining images to advance to.
    /// </summary>
    /// <param name="image">The image that is currently being written to. The path of this image
    /// will be compared to the last path specified in the <see cref="IInputArguments.CoverImages"/>.
    /// </param>
    /// <returns>1 if the input image is the last available cover image to clean. Otherwise, 0.</returns>
    private int ComputeModifier(IBasicImageInfo image)
    {
        if (!IsLastImage(image))
        {
            return 0;
        }

        return -1 * Calculator.MinimumBitsPerPixel * arguments.BitsToUse;
    }

    private string GenerateBinaryString(IBasicImageInfo image)
    {
        int modifier = ComputeModifier(image);
        long bitCount = calculator.CalculateStorageSpaceOfImage(image.Width, image.Height, arguments.BitsToUse) + modifier;

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