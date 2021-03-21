using System;

namespace SteganographyApp.Common.IO
{

    /// <summary>
    /// A general exception to represent a specific error occured
    /// while reading or writing data to the images.
    /// </summary>
    public class ImageProcessingException : Exception
    {
        public ImageProcessingException(string message) : base(message) { }
        public ImageProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Event arguments passed into the OnNextImageLoaded event handler whenever the next image
    /// has been loaded in the read, write, or clean process.
    /// </summary>
    public class NextImageLoadedEventArgs
    {
        public string ImageName { get; set; }
        public int ImageIndex { get; set; }
    }

    /// <summary>
    /// Event arguments passed into the OnChunkWritten event handler whenever an encoded binary
    /// content has been writtent to an image
    /// </summary>
    public class ChunkWrittenArgs
    {
        public int ChunkLength { get; set; }
    }

    /// <summary>
    /// Handles the current pixel position for the currently loaded image in the ImageStore.
    /// </summary>
    class PixelPosition
    {
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Attempts to move to the next available position;
        /// </summary>
        public bool TryMoveToNext(int maxWidth, int maxHeight)
        {
            if (!CanMoveToNext(maxWidth, maxHeight))
            {
                return false;
            }

            X = X + 1;
            if (X == maxWidth) {
                X = 0;
                Y = Y + 1;
            }
            return true;
        }

        private bool CanMoveToNext(int maxWidth, int maxHeight)
        {
            return !(X + 1 == maxWidth && Y + 1 == maxHeight);
        }

        /// <summary>
        /// Resets the X and Y positions to 0.
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;
        }
    }

}