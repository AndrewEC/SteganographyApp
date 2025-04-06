namespace SteganographyApp;

using System;
using System.Collections.Generic;

using SteganographyApp.Common;
using SteganographyApp.Common.IO;

/// <summary>
/// Helps track the images being used during the encoding process.
/// All images are using during the encoding process if the enableDummies
/// flag has been provided even if they have not been written to.
/// </summary>
internal sealed class ImageTracker
{
    private readonly HashSet<string> imagesUsed = [];
    private readonly int availableImages;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTracker"/> class.
    /// </summary>
    /// <param name="arguments">Arguments from which a count of the number of cover images
    /// available will be pulled.</param>
    /// <param name="store">The ImageStore to attach to the the OnNextImageLoaded event of.</param>
    public ImageTracker(IInputArguments arguments, ImageStore store)
    {
        availableImages = arguments.CoverImages.Length;
        store.OnNextImageLoaded += RecordLoadedImage;
    }

    /// <summary>
    /// Utility method to print out a list the list of images that were used in the
    /// encoding decoding process. Will only print something out it the number of image
    /// uses was less than the number of images parsed in the arguments.
    /// </summary>
    /// <param name="imagesUsed">A list containing the names of the images used in the
    /// encoding/decoding process.</param>
    public void PrintImagesUtilized()
    {
        if (imagesUsed.Count == availableImages)
        {
            return;
        }

        Console.WriteLine("Not all images were written to.");
        Console.WriteLine("Only the images that were written to are required to decode later.");
        Console.WriteLine("The following images were used:");

        foreach (string image in imagesUsed)
        {
            Console.WriteLine("\t{0}", image);
        }
    }

    private void RecordLoadedImage(object? sender, NextImageLoadedArgs args)
    {
        imagesUsed.Add(args.Path);
    }
}