namespace SteganographyApp.Common.IO
{
    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Handles the current pixel position for the currently loaded image in the ImageStore.
    /// </summary>
    internal class PixelPosition
    {
        /// <summary>
        /// Gets the current pixel X position.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Gets the current pixel Y position.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Gets or sets the image currently being tracked and updated. This width and height of this
        /// image is used to determine if there is a pixel to move to when CanMoveToNext is invoked.
        /// </summary>
        private IBasicImageInfo? TrackedImage { get; set; }

        /// <summary>
        /// Sets the image whose pixel position is currently being tracked and starts
        /// tracking from the first pixel position (0, 0).
        /// </summary>
        /// <param name="imageInfo">The image to track.</param>
        public void TrackImage(IBasicImageInfo imageInfo)
        {
            TrackedImage = imageInfo;
            Reset();
        }

        /// <summary>
        /// Attempts to move to the next available position.
        /// </summary>
        /// <returns>False if there is no further pixel to move to, otherwise true.</returns>
        public bool TryMoveToNext()
        {
            if (!CanMoveToNext())
            {
                return false;
            }

            X = X + 1;
            if (X == TrackedImage!.Width)
            {
                X = 0;
                Y = Y + 1;
            }

            return true;
        }

        /// <summary>
        /// Resets the X and Y positions to 0.
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Stringifies the pixel position in the format (X: {0}, Y: {1}).
        /// </summary>
        /// <returns>The position of the pixel in the format (X: {0}, Y: {1}).</returns>
        public override string ToString() => string.Format("(X: {0}, Y: {1})", X, Y);

        private bool CanMoveToNext() => !(X + 1 == TrackedImage!.Width && Y + 1 == TrackedImage.Height);
    }
}