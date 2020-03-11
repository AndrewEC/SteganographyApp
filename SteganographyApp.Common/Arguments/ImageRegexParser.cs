using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteganographyApp.Common.Arguments
{

    static class ImageRegexParser
    {

        /// <summary>
        /// If a regular expression is detected in the --images parameter this method
        /// will load the images in the specified directory based on matches against
        /// the regular expression.
        /// <para>The regex expression will be retrieved by invoking the <see cref="ParseRegex"/> method.</para>
        /// </summary>
        /// <param name="value">The value of the --images parameter</param>
        /// <returns>An array of images from the specified directory.</returns>
        /// <exception cref="ArgumentValueException">Thrown if an invalid regular expression is provided or if the
        /// regular expression doesn't match any files in the provided directory.</exception>
        public static string[] ImagesFromRegex(string value)
        {
            (string regex, string path) = ParseRegex(value);

            string[] files = Directory.GetFiles(path);
            string[] images = files.Where(file => Regex.Match(file, regex).Success).ToArray();
            Array.Sort(images, (first, second) => string.Compare(first, second));
            
            if (images.Length == 0)
            {
                throw new ArgumentValueException($"The provided regex expression returned 0 usable files in the directory {path}");
            }
            return images;
        }

        /// <summary>
        /// Parses the regular expression and path from the value of the --images parameter.
        /// Input strings will be in the format [r]&lt;REGEX&gt;&lt;DIRECTORY&gt;
        /// </summary>
        /// <param name="value">The value of the --images parameter</param>
        /// <returns>A tuple containing the regex nd directory in that order.</returns>
        /// <exception cref="ArgumentValueException">Thrown if the value for the --images parameter
        /// does not match the expected format.</exception>
        private static (string, string) ParseRegex(string value)
        {
            value = value.Replace("[r]", "");

            if (value[value.Length - 1] != '>' || value[0] != '<')
            {
                throw new ArgumentValueException("The value supplied for the --images key is invalid. Expected the format [r]<regex><directory>");
            }

            string[] parts = value.Split('>');
            if (parts.Length != 3 || parts[0] == "<" || parts[1] == "<")
            {
                throw new ArgumentValueException("The value supplied for the --images key is invalid. Expected the format [r]<regex><directory>");
            }

            string regex = parts[0].Replace("<", "");
            string path = parts[1].Replace("<", "");
            return (regex, path);
        }

    }

}