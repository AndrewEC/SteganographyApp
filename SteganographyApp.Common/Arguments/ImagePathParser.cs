using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SteganographyApp.Common.Arguments
{

    static class ImagePathParser
    {

        private static readonly ImmutableDictionary<string, string> ShorthandMappings = new Dictionary<string, string>()
        {
            { "PNG_IMAGES", @"[r]<^[\W\w]+\.png$><.>" },
            { "JPG_IMAGES", @"[r]<^[\W\w]+\.jpg$><.>" }
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
        /// <exception cref="ArgumentValueException">Thrown if the image
        /// could not be found at the specified path.</exception>
        public static void ParseImages(InputArguments arguments, string value)
        {
            string[] images = RetrieveImagePaths(value);
            ValidateImagePaths(images);
            arguments.CoverImages = images;
        }

        private static string[] RetrieveImagePaths(string value)
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
            return imagePaths.Select(imagePath => imagePath.Trim()).ToArray();
        }

        private static void ValidateImagePaths(string[] imagePaths)
        {
            if (imagePaths.Length == 0)
            {
                throw new ArgumentValueException($"No images were found using the provided regular expression.");
            }

            for (int i = 0; i < imagePaths.Length; i++)
            {
                if (!File.Exists(imagePaths[i]))
                {
                    throw new ArgumentValueException($"Image could not be found at {imagePaths[i]}");
                }
                else if (File.GetAttributes(imagePaths[i]).HasFlag(FileAttributes.Directory))
                {
                    throw new ArgumentValueException($"File found at {imagePaths[i]} was a directory instead of an image.");
                }
            }
        }

    }

}