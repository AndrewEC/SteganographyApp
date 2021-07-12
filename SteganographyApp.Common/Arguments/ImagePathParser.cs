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
    internal static class ImagePathParser
    {
        private static readonly ImmutableDictionary<string, string> ShorthandMappings = new Dictionary<string, string>()
        {
            { "PNG_IMAGES", @"[r]<^[\W\w]+\.png$><.>" },
            { "JPG_IMAGES", @"[r]<^[\W\w]+\.(jpg|jpeg)$><.>" },
        }
        .ToImmutableDictionary();

        /// <summary>
        /// Takes in a string of comma delimited image names and returns an array of strings.
        /// Will also parse for a regex expression if an expression has been specified with the [r]
        /// prefix.
        /// </summary>
        /// <param name="arguments">The input arguments instance to make modifications to.</param>
        /// <param name="value">A string representation of a number, or a single, image where encoded
        /// data will be writted to or where decoded data will be read from.</param>
        public static void ParseImages(InputArguments arguments, string value)
        {
            var images = RetrieveImagePaths(value);
            ValidateImagePaths(images);
            arguments.CoverImages = images;
        }

        private static ImmutableArray<string> RetrieveImagePaths(string value)
        {
            value = ShorthandMappings.GetValueOrDefault(value, value);
            string[] imagePaths = null;
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