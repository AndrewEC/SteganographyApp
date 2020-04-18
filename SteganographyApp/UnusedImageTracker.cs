using System;
using System.Collections.Generic;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;

namespace SteganographyApp
{

    sealed class ImageTracker
    {

        private readonly HashSet<string> imagesUsed = new HashSet<string>();
        private readonly int AvailableImages;

        private ImageTracker(int availableImages)
        {
            AvailableImages = availableImages;
        }

        public static ImageTracker CreateTrackerFrom(IInputArguments arguments, ImageStore store)
        {
            var tracker = new ImageTracker(arguments.CoverImages.Length);
            store.OnNextImageLoaded += tracker.RecordLoadedImage;
            return tracker;
        }

        private void RecordLoadedImage(object sender, NextImageLoadedEventArgs args)
        {
            imagesUsed.Add(args.ImageName);
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
            if (imagesUsed.Count == AvailableImages)
            {
                return;
            }
            Console.WriteLine("Not all images were written to.");
            Console.WriteLine("While these images don't contain encoded data they will be needed to properly decode.");
            Console.WriteLine("The following files were used:");
            foreach (string image in imagesUsed)
            {
                Console.WriteLine("\t{0}", image);
            }
        }

    }

}