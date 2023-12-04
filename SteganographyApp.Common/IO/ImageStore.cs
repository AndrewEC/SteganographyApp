namespace SteganographyApp.Common.IO;

using System;

using SixLabors.ImageSharp.PixelFormats;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Logging;

/// <summary>
/// Class that handles positioning a make shift write stream in the proper position so
/// data can be reliably read and written to the storage images.
/// </summary>
/// <remarks>
/// Creates a new instance of the ImageStore and calculates the RequiredContentChunkTableBitSize
/// value.
/// </remarks>
/// <param name="args">The values parsed from the command line arguments.</param>
public sealed class ImageStore(IInputArguments args)
{
    /// <summary>
    /// Stores the current x and y position of the pixel the image store is about to
    /// read from or write to.
    /// </summary>
    private readonly PixelPosition pixelPosition = new();

    /// <summary>
    /// The values parsed from the command line arguments.
    /// </summary>
    private readonly IInputArguments args = args;

    private readonly ILogger log = new LazyLogger<ImageStore>();

    /// <summary>
    /// The index used to determine which image will be read/written to in the
    /// next read/write operation.
    /// <para>The image name is retrieved by looking up the args.CoverImage value
    /// using this field as the index.</para>
    /// </summary>
    private int currentImageIndex = -1;

    /// <summary>
    /// Event handler that's invoked whenever the WriteContentChunkToImage method completes.
    /// The main argument of the event will indicate the total number of bits written to the
    /// image(s).
    /// </summary>
    public event EventHandler<ChunkWrittenArgs>? OnChunkWritten;

    /// <summary>
    /// Event handler that's invoked whenever the internal Next method is called.
    /// In the Next method another image will be loaded so it can be read/written to.
    /// The path of the file will be the main argument passed to the event handler.
    /// </summary>
    public event EventHandler<NextImageLoadedEventArgs>? OnNextImageLoaded;

    /// <summary>
    /// Gets the currently loaded image. The image is loaded whenever the Next
    /// method is called and the currentImageIndex has been incremented.
    /// </summary>
    public IBasicImageInfo? CurrentImage { get; private set; }

    /// <summary>
    /// Utility method to create an <see cref="ImageStoreIO"/> instance to be used in conjunction with the
    /// read and write methods.
    /// <para>The ImageStoreWrapper instance provides proxies to the ImageStore IO methods as well as
    /// implementing the IDisposable interface to ensure that any currently open and loaded image
    /// will be properly disposed.</para>
    /// </summary>
    /// <returns>A new wrapper for safely using the image store IO methods.</returns>
    public ImageStoreIO CreateIOWrapper() => new(this);

    /// <summary>
    /// Moves the current index back to the index before the specified image
    /// then calls the Next method to advance to the specified image.
    /// </summary>
    /// <param name="coverImageIndex">The index of the image to start reading and writing from.</param>
    /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
    public void SeekToImage(int coverImageIndex)
    {
        if (coverImageIndex < 0 || coverImageIndex >= args.CoverImages.Length)
        {
            throw new ImageProcessingException($"An invalid image index was provided in ResetTo. Expected value between [{0}] and [{args.CoverImages.Length - 1}], instead got [{coverImageIndex}].");
        }
        currentImageIndex = coverImageIndex - 1;
        LoadNextImage();
    }

    /// <summary>
    /// Method to help close off any open images.
    /// </summary>
    /// <param name="saveImageChanges">If true this method will attempt to save any changes
    /// made to the current image.</param>
    internal void CloseOpenImage(bool saveImageChanges = false)
    {
        if (CurrentImage == null)
        {
            return;
        }
        if (saveImageChanges)
        {
            log.Debug("Saving changes to image [{0}]", CurrentImage.Path);
            var encoder = Injector.Provide<IEncoderProvider>().GetEncoder(CurrentImage.Path);
            CurrentImage.Save(CurrentImage.Path, encoder);
        }
        CurrentImage.Dispose();
        CurrentImage = null;
    }

    /// <summary>
    /// Writes a binary string value to the current image, starting
    /// at the last write index, by replacing the LSB of each RGB value in each pixel.
    /// <para>If the current image does not have enough storage space to store the binary
    /// string in full then the Next method will be invoked and it will continue to write
    /// on the next available image.</para>
    /// </summary>
    /// <param name="binary">The encypted binary string to write to the image.</param>
    /// <returns>The number of bits written to the image. This is mostly important when
    /// writing the content chunk table to the start of the leading image.</returns>
    /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
    internal int WriteBinaryString(string binary)
    {
        if (CurrentImage == null)
        {
            return -1;
        }
        log.Debug("Writing [{0}] bits to image [{1}] starting at position [{2}]", binary.Length, CurrentImage.Path, pixelPosition.ToString());
        var queue = new ReadBitQueue(binary);
        var pixelWriter = new PixelWriter(queue, args.BitsToUse);
        while (queue.HasNext())
        {
            Rgba32 source = CurrentImage[pixelPosition.X, pixelPosition.Y];
            Rgba32 updated = pixelWriter.UpdatePixel(source);
            CurrentImage[pixelPosition.X, pixelPosition.Y] = updated;

            if (!pixelPosition.TryMoveToNext())
            {
                LoadNextImage(true);
            }
        }
        OnChunkWritten?.Invoke(this, new ChunkWrittenArgs(binary.Length));
        return binary.Length;
    }

    /// <summary>
    /// Attempts to read the specified number of bits from the current image starting
    /// at the last read/write position.
    /// <para>If there is not enough available space in the current image then it will
    /// load the next cover image and continue to read from the next available image.</para>
    /// </summary>
    /// <param name="bitsToRead">Specifies the number of bits to be read from the current image.</param>
    /// <returns>A binary string whose length is equal to the length specified in the length
    /// parameter.</returns>
    /// <exception cref="ImageProcessingException">Rethrown from the Next method call.</exception>
    internal string ReadBinaryString(int bitsToRead)
    {
        if (CurrentImage == null)
        {
            return string.Empty;
        }
        log.Debug("Reading [{0}] bits from image [{1}] starting from position [{2}]", bitsToRead, CurrentImage, pixelPosition.ToString());
        var binaryStringBuilder = new BinaryStringBuilder(bitsToRead);
        var pixelReader = new PixelReader(binaryStringBuilder, args.BitsToUse);
        while (!binaryStringBuilder.IsFull())
        {
            Rgba32 pixel = CurrentImage[pixelPosition.X, pixelPosition.Y];
            pixelReader.ReadBinaryFromPixel(pixel);
            if (!pixelPosition.TryMoveToNext())
            {
                LoadNextImage();
            }
        }
        return binaryStringBuilder.ToBinaryString();
    }

    /// <summary>
    /// Attempts to save then dispose of the currently opened image, if the currentImage property
    /// is not null, resets the read/write x and y positions to 0, increments the current image index,
    /// loads the new image, and invokes the OnNextImageLoaded event.
    /// </summary>
    /// <param name="saveImageChanges">If true it will attempt to save any changes made to the currently open
    /// image before attempting to increment to the next available image.</param>
    /// <exception cref="ImageProcessingException">If the increment of the current
    /// image index exeeds the available number of images an exception will be thrown.</exception>
    internal void LoadNextImage(bool saveImageChanges = false)
    {
        log.Trace("Loading next image.");
        CloseOpenImage(saveImageChanges);

        currentImageIndex++;
        if (currentImageIndex == args.CoverImages.Length)
        {
            throw new ImageProcessingException("Cannot load next image because there are no remaining cover images left to load.");
        }

        CurrentImage = Injector.Provide<IImageProxy>().LoadImage(args.CoverImages[currentImageIndex]);
        pixelPosition.TrackImage(CurrentImage);
        log.Debug("Loaded image [{0}]", CurrentImage.Path);

        OnNextImageLoaded?.Invoke(this, new NextImageLoadedEventArgs(CurrentImage.Path, currentImageIndex));
    }

    /// <summary>
    /// Skip over a number of pixels on the currently loaded image equal to one third
    /// of the number specified as the bitsToSkip arguments.
    /// </summary>
    /// <param name="bitsToSkip">The number of bits to skip over for the current over.</param>
    /// <exception cref="ImageProcessingException">If the number of bits to skip puts the current position past
    /// the last bit of the currently loaded image then a processing exception will be thrown.</exception>
    internal void SeekToPixel(int bitsToSkip)
    {
        if (CurrentImage == null)
        {
            return;
        }
        log.Debug("Seeking past [{0}] bits in image [{1}]", bitsToSkip, CurrentImage);
        pixelPosition.Reset();

        int pixelIndex = (int)Math.Ceiling((double)bitsToSkip / (Calculator.MinimumBitsPerPixel * args.BitsToUse));
        for (int i = 0; i < pixelIndex; i++)
        {
            if (!pixelPosition.TryMoveToNext())
            {
                LoadNextImage();
            }
        }
    }
}