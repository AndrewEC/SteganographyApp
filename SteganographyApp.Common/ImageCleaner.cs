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
/// Utility class to automate the process of cleaning data from a series of cover images.
/// <para>This "cleans" the image by writing random 1s and 0s to the least and second
/// least significant bits of each RGB channel of each pixel of each image.</para>
/// </summary>
/// <param name="arguments">The arguments to configure how many bits per colour channel
/// to overwrite and which images are to be cleaned.</param>
/// <param name="imageStore">The image store instance initialized with the same arguments
/// parameter as provided in this constructor.</param>
public sealed class ImageCleaner(IInputArguments arguments, IImageStore imageStore)
{
    private const int MaxTableEntries = 5;
    private const int MinTableEntries = 1;
    private const int MaxEntryLength = 10_000;
    private const int MinEntryLength = 100;

    private readonly IInputArguments arguments = arguments;
    private readonly IImageStore imageStore = imageStore;

    private readonly ILogger log = new LazyLogger<ImageCleaner>();

    /// <summary>
    /// Cleans all the cover images specified in the cover images array of the input arguments.
    /// </summary>
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
    /// <returns>0 if this is not the last image or the number of bits needed to subtract from the total
    /// bits the image can store to ensure the last pixel in the image is not writtent to.</returns>
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