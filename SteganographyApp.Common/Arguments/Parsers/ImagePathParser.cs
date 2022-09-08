namespace SteganographyApp.Common.Arguments
{
    using System.Collections.Immutable;
    using System.IO;

    using Microsoft.Extensions.FileSystemGlobbing;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Static utility class to help parse out a list of paths to the images provided in the
    /// users arguments.
    /// </summary>
    public static class ImagePathParser
    {
        /// <summary>
        /// Parses a list of image paths from the original input string.
        /// </summary>
        /// <param name="value">The string to be parsed in the list of image paths. Can be a comma
        /// delimited list of globs.</param>
        /// <returns>A list of the files identified by the matching glob patterns. This method
        /// minimally ensures that each file exists and is a file and not a directory.</returns>
        public static ImmutableArray<string> ParseImages(string value)
        {
            var images = RetrieveImagePaths(value);
            ValidateImagePaths(images);
            return images;
        }

        private static ImmutableArray<string> RetrieveImagePaths(string value)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(value.Split(","));
            return matcher.GetResultsInFullPath(Directory.GetCurrentDirectory()).ToImmutableArray();
        }

        private static void ValidateImagePaths(ImmutableArray<string> imagePaths)
        {
            if (imagePaths.Length == 0)
            {
                throw new ArgumentValueException($"No images could be found.");
            }

            var fileProxy = Injector.Provide<IFileIOProxy>();
            foreach (string path in imagePaths)
            {
                if (!fileProxy.IsExistingFile(path))
                {
                    throw new ArgumentValueException($"The file specified could not be read: [{path}]");
                }
            }
        }
    }
}