namespace SteganographyApp.Common.IO
{
    /// <summary>
    /// Handles the current pixel position for the currently loaded image in the ImageStore.
    /// </summary>
    internal class PixelPosition
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

        public override string ToString() => string.Format("(X: {0}, Y: {1})", X, Y);

        private bool CanMoveToNext(int maxWidth, int maxHeight) => !(X + 1 == maxWidth && Y + 1 == maxHeight);
    }
}