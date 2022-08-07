namespace SteganographyApp.Common.Arguments
{
    using System.Collections.Immutable;

    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

    /// <summary>
    /// A static class containing a series of parsers for parsing out the user's provided arguments.
    /// </summary>
    public static class ParserFunctions
    {
        private const int MaxDummyCount = 1000;
        private const int MinDummyCount = 100;

        /// <summary>
        /// Parses out the number of dummy entries that should be inserted, or removed, from the file content.
        /// </summary>
        /// <param name="insertDummies">A boolean indicates whether dummy entries should be inserted. If this is false
        /// the method will return 0.</param>
        /// <param name="coverImages">An array of the images being encoded to or decoded from.</param>
        /// <param name="randomSeed">A random seed used to help randomize the number of dummy entries generated.</param>
        /// <returns>The number of dummy entries.</returns>
        public static int ParseDummyCount(bool insertDummies, ImmutableArray<string> coverImages, string randomSeed)
        {
            if (!insertDummies || Checks.IsNullOrEmpty(coverImages))
            {
                return 0;
            }

            var imageProxy = Injector.Provide<IImageProxy>();

            long dummyCount = 1;
            int[] imageIndexes = new int[] { 0, coverImages.Length - 1 };
            foreach (int imageIndex in imageIndexes)
            {
                using (var image = imageProxy.LoadImage(coverImages[imageIndex]))
                {
                    dummyCount += dummyCount * (image.Width * image.Height);
                }
            }

            string userRandomSeed = string.IsNullOrEmpty(randomSeed) ? string.Empty : randomSeed;
            string seed = userRandomSeed + dummyCount.ToString();
            return IndexGenerator.FromString(seed).Next(MaxDummyCount - MinDummyCount) + MinDummyCount;
        }

        /// <summary>
        /// Verifies that the value parameter points to an existing file.
        /// </summary>
        /// <param name="value">A string pointing to the location of a user specified file.</param>
        /// <returns>The original input value parmeter.</returns>
        public static string ParseFilePath(string value)
        {
            if (!Injector.Provide<IFileIOProxy>().IsExistingFile(value))
            {
                throw new ArgumentValueException($"Input file could not be found or is not a file: {value}");
            }
            return value;
        }
    }
}