using System;
using System.IO;
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
            string[] images = new string[files.Length];
            int valid = 0;
            foreach (string name in files)
            {
                try
                {
                    if (Regex.Match(name, regex).Success)
                    {
                        images[valid] = name;
                        valid++;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentValueException("An invalid regular expression was provided for the image input value.", e);
                }
            }
            if (valid == 0)
            {
                throw new ArgumentValueException(String.Format("The provided regex expression returned 0 usable files in the directory {0}", path));
            }
            else if (valid < images.Length)
            {
                string[] temp = new string[valid];
                Array.Copy(images, temp, valid);
                images = temp;
            }
            return images;
        }

        /// <summary>
        /// Parses the regular expression and path from the value of the --images parameter.
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