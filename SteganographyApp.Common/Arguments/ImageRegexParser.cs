namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Static utility class to identity and invoke a regular expression to help identify
    /// coverage images to use from a given directory.
    /// </summary>
    internal static class ImageRegexParser
    {
        private static readonly string ImageRegexExpression = @"^\[r\]\<(.+)\>\<(.+)\>$";
        private static readonly Regex ImageRegex = new Regex(ImageRegexExpression);

        /// <summary>
        /// Checks if the input value is a valid regex based input.
        /// </summary>
        /// <param name="value">The input value to check against.</param>
        /// <returns>True if a directory and regex have been given by the user in a valid format otherwise false.</returns>
        /// <see cref="ImageRegexParser.ImageRegexExpression">The regular expression used to
        /// check if the input is a valid regex based in put scheme.</see>
        public static bool IsValidRegex(string value) => ImageRegex.Match(value).Success;

        /// <summary>
        /// If a regular expression is detected in the --images parameter this method
        /// will load the images in the specified directory based on matches against
        /// the regular expression.
        /// <para>The regex expression will be retrieved by invoking the <see cref="ParseRegex"/> method.</para>
        /// </summary>
        /// <param name="value">The value of the --images parameter.</param>
        /// <returns>An array of images from the specified directory.</returns>
        /// <exception cref="ArgumentValueException">Thrown if an invalid regular expression is provided or if the
        /// regular expression doesn't match any files in the provided directory.</exception>
        public static string[] ImagePathsFromRegex(string value)
        {
            (string regexExpression, string path) = ParseRegex(value);
            var regex = new Regex(regexExpression);

            string[] files = Injector.Provide<IFileIOProxy>().GetFiles(path);
            string[] images = files.Where(file => regex.Match(file).Success).ToArray();
            Array.Sort(images, string.Compare);

            return images;
        }

        /// <summary>
        /// Parses the regular expression and path from the value of the --images parameter.
        /// Input strings will be in the format [r]&lt;REGEX&gt;&lt;DIRECTORY&gt;.
        /// </summary>
        /// <param name="value">The value of the --images parameter.</param>
        /// <returns>A tuple containing the regex nd directory in that order.</returns>
        /// <exception cref="ArgumentValueException">Thrown if the value for the --images parameter
        /// does not match the expected format.</exception>
        private static (string, string) ParseRegex(string value)
        {
            var match = ImageRegex.Match(value);
            var fileNameRegex = match.Groups[1].Value;
            var directory = match.Groups[2].Value;

            return (fileNameRegex, directory);
        }
    }
}