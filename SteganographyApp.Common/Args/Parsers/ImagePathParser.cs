namespace SteganographyApp.Common.Arguments
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Static utility class to help parse out a list of paths to the images provided in the
    /// users arguments.
    /// </summary>
    public static class ImagePathParser
    {
        private static readonly ImmutableDictionary<string, string> ShorthandMappings = new Dictionary<string, string>()
        {
            { "PNG_IMAGES", @"[r]<^[\W\w]+\.png$><.>" },
            { "JPG_IMAGES", @"[r]<^[\W\w]+\.(jpg|jpeg)$><.>" },
            { "WEBP_IMAGES", @"[r]<^[\W\w]+\.webp$><.>" },
        }
        .ToImmutableDictionary();

        /// <summary>
        /// Parses a list of image paths from the original input string. This will also verify the paths actually
        /// exist and point to a file and not a directory.
        /// </summary>
        /// <param name="value">The string to be parsed in the list of image paths. Can either be a comma delimited list of
        /// paths, a regular expression, or a shorthand mapping like PNG_IMAGES, JPG_IMAGES, or WEBP_IMAGES.</param>
        /// <returns>An array of string representing the paths to an image file.</returns>
        public static ImmutableArray<string> ParseImages(string value)
        {
            var images = RetrieveImagePaths(value);
            ValidateImagePaths(images);
            return images;
        }

        private static ImmutableArray<string> RetrieveImagePaths(string value)
        {
            value = ShorthandMappings.GetValueOrDefault(value, value);
            string[] imagePaths;
            if (ImageRegexParser.IsValidRegex(value))
            {
                imagePaths = ImageRegexParser.ImagePathsFromRegex(value);
            }
            else if (value.Contains(","))
            {
                imagePaths = value.Split(',');
            }
            else
            {
                imagePaths = new string[] { value };
            }
            return ImmutableArray.Create(imagePaths.Select(imagePath => imagePath.Trim()).ToArray());
        }

        private static void ValidateImagePaths(ImmutableArray<string> imagePaths)
        {
            if (imagePaths.Length == 0)
            {
                throw new ArgumentValueException($"No images were found using the provided regular expression.");
            }

            var fileProxy = Injector.Provide<IFileIOProxy>();
            foreach (string path in imagePaths)
            {
                if (!fileProxy.IsExistingFile(path))
                {
                    throw new ArgumentValueException($"The file specified could not be found or is not a file: {path}");
                }
            }
        }
    }
}