namespace SteganographyApp.Common.IO
{
    using System;

    /// <summary>
    /// A general exception to represent a specific error occured
    /// while reading or writing data to the images.
    /// </summary>
    public class ImageProcessingException : Exception
    {
        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessage/*' />
        public ImageProcessingException(string message) : base(message) { }

        /// <include file='../docs.xml' path='docs/members[@name="Exceptions"]/GeneralMessageInner/*' />
        public ImageProcessingException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Event arguments passed into the OnNextImageLoaded event handler whenever the next image
    /// has been loaded in the read, write, or clean process.
    /// </summary>
    public class NextImageLoadedEventArgs
    {
        /// <summary>
        /// Gets or sets the name of the file that was loaded.
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Gets or sets the index of the image that was loaded. This represents the index of the image within the
        /// cover images parsed from the user's input.
        /// </summary>
        public int ImageIndex { get; set; }
    }

    /// <summary>
    /// Event arguments passed into the OnChunkWritten event handler whenever an encoded binary
    /// content has been writtent to an image.
    /// </summary>
    public class ChunkWrittenArgs
    {
        /// <summary>
        /// Gets or sets the length, in bytes, of the chunk that was just written to the cover images.
        /// </summary>
        public int ChunkLength { get; set; }
    }
}