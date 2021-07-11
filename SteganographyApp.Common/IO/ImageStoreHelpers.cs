namespace SteganographyApp.Common.IO
{
    using System;

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
}