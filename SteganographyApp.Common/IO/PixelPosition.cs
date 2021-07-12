namespace SteganographyApp.Common.IO
{
    /// <summary>
    /// Handles the current pixel position for the currently loaded image in the ImageStore.
    /// </summary>
    internal class PixelPosition
    {
        /// <summary>
        /// Gets or sets the current pixel X position.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the current pixel Y position.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Attempts to move to the next available position.
        /// </summary>
        /// <param name="maxWidth">The width of the current image being traversed.</param>
        /// <param name="maxHeight">The height of the current image being traversed.</param>
        /// <returns>False if there is no further pixel to move to, otherwise true.</returns>
        public bool TryMoveToNext(int maxWidth, int maxHeight)
        {
            if (!CanMoveToNext(maxWidth, maxHeight))
            {
                return false;
            }

            X = X + 1;
            if (X == maxWidth)
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

        private bool CanMoveToNext(int maxWidth, int maxHeight) => !(X + 1 == maxWidth && Y + 1 == maxHeight);
    }
}