using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Arguments
{

    static class Parsers
    {

        /// <summary>
        /// Parses a boolean from the value parameter and sets the InsertDummies property.
        /// Will also attempt to call <see cref="ParseDummyCount"/>
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the InsertDummies boolean flag.</param>
        public static void ParseInsertDummies(InputArguments arguments, string value)
        {
            arguments.InsertDummies = Boolean.Parse(value);
            ParseDummyCount(arguments);
        }

        /// <summary>
        /// Parses a boolean from the value parameter and sets the DeleteAfterConversion property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the InsertDummies boolean flag.</param>
        public static void ParseDeleteOriginals(InputArguments arguments, string value)
        {
            arguments.DeleteAfterConversion = Boolean.Parse(value);
        }

        /// <summary>
        /// Parses the number of dummy entries to insert into the image based on the
        /// properties retrieved from the first and last images.
        /// <para>Note: the first and last image can be the same image if only one cover image
        /// was provided.</para>
        /// </summary>
        public static void ParseDummyCount(InputArguments arguments)
        {
            if (!arguments.InsertDummies || arguments.CoverImages == null)
            {
                return;
            }
            
            long dummyCount = 1;
            int[] imageIndexes = new int[] { 0, arguments.CoverImages.Length - 1 };
            foreach (int imageIndex in imageIndexes)
            {
                using(Image<Rgba32> image = Image.Load(arguments.CoverImages[imageIndex]))
                {
                    dummyCount += dummyCount * (image.Width * image.Height);
                }
            }
            string seed = dummyCount.ToString();
            arguments.DummyCount = IndexGenerator.FromString(seed).Next(10);
        }

        /// <summary>
        /// Parses the compression level and sets the CompressionLevel property value.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of an int value.</param>
        /// <exception cref="ArgumentValueException">Thrown if the string value could not be converted
        /// to an int or if the value is less than 0 or greater than 9.</exception>
        public static void ParseCompressionLevel(InputArguments arguments, string value)
        {
            try
            {
                arguments.CompressionLevel = Convert.ToInt32(value);
                if (arguments.CompressionLevel < 0 || arguments.CompressionLevel > 9)
                {
                    throw new ArgumentValueException(string.Format("The compression level must be a whole number between 0 and 9 inclusive."));
                }
            }
            catch (Exception e) when (e is FormatException || e is OverflowException)
            {
                throw new ArgumentValueException(string.Format("Could not parse compression level from value: {0}", value), e);
            }
        }

        /// <summary>
        /// Parses an int32 from the value string and sets the ChunkByteSize property
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of an int value.</param>
        /// <exception cref="ArgumentValueException">Thrown if the string value could not be converted
        /// to an int or if the value of the int is less than or equal to 0.</exception>
        public static void ParseChunkSize(InputArguments arguments, string value)
        {
            try
            {
                arguments.ChunkByteSize = Convert.ToInt32(value);
                if(arguments.ChunkByteSize <= 0)
                {
                    throw new ArgumentValueException("The chunk size value must be a positive whole number with a value more than 0.");
                }
            }
            catch(Exception e) when (e is FormatException || e is OverflowException)
            {
                throw new ArgumentValueException(String.Format("Could not parse chunk size from value {0}", value), e);
            }
        }

        /// <summary>
        /// Parses a boolean from the value and sets the PrintStack property
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify</param>
        /// <param name="value">A string representation of a boolean value.</param>
        /// <exception cref="ArgumentValueException">Thrown if a boolean value could not
        /// be parsed from the value parameter.</exception>
        public static void ParsePrintStack(InputArguments arguments, string value)
        {
            arguments.PrintStack = Boolean.Parse(value);
        }

        /// <summary>
        /// Checks if the specified file, value, exists and then sets the
        /// FileToEncode property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The relative or absolute path to the file to encode.</param>
        /// <exception cref="ArgumentValueException">Thrown if the specified input file
        /// could not be found.</exception>
        public static void ParseFileToEncode(InputArguments arguments, string value)
        {
            if (!File.Exists(value))
            {
                throw new ArgumentValueException(String.Format("File to decode could not be found at {0}", value));
            }
            else if (File.GetAttributes(value).HasFlag(FileAttributes.Directory))
            {
                throw new ArgumentValueException(String.Format("Input file at {0} was a directory but a file is required.", value));
            }
            arguments.FileToEncode = value;
        }

        /// <summary>
        /// Parses a boolean from the value string and sets the UseCompression property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify</param>
        /// <param name="value">A string representation of a boolean value.</param>
        /// <exception cref="ArgumentValueException">Thrown if a boolean value could not be parsed
        /// from the value parameter</exception>
        public static void ParseUseCompression(InputArguments arguments, string value)
        {
            arguments.UseCompression = Boolean.Parse(value);
        }

        /// <summary>
        /// Sets the EncodeDecode action based on the value returned from the Enum.TryParse method.
        /// </summary>
        /// <param name="args">The InputArguments instance to make modifications to.</param>
        /// <param name="value">A string representation of an EncodeDecodeAction enum value.</param>
        /// <exception cref="ArgumentValueException">Thrown if
        /// the string value does not map to an enum value.</exception>
        public static void ParseEncodeOrDecodeAction(InputArguments args, string value)
        {
            value = value.Replace("-", "");
            if(!Enum.TryParse(value, true, out ActionEnum action))
            {
                throw new ArgumentValueException(String.Format("Invalid value for action argument. Expected 'encode', 'decode', 'clean', 'calculate-storage-space', or 'calculate-encrypted-size' got {0}", value));
            }
            args.EncodeOrDecode = action;
        }

        /// <summary>
        /// Takes in a string of comma delimited image names and returns an array of strings.
        /// Will also parse for a regex expression if an expression has been specified with the [r]
        /// prefix.
        /// </summary>
        /// /// <param name="arguments">The input arguments instance to make modifications to.</param>
        /// <param name="value">A string representation of a number, or a single, image where encoded
        /// data will be writted to or where decoded data will be read from.</param>
        /// <exception cref="ArgumentValueException">Thrown if the image
        /// could not be found at the specified path.</exception>
        public static void ParseImages(InputArguments arguments, string value)
        {
            string[] images = null;

            if (value.Contains("[r]"))
            {
                images = ImageRegexParser.ImagesFromRegex(value);
            }
            else if (value.Contains(","))
            {
                images = value.Split(',');
            }
            else
            {
                images = new string[] { value };
            }

            for (int i = 0; i < images.Length; i++)
            {
                images[i] = images[i].Trim();
                if (!File.Exists(images[i]))
                {
                    throw new ArgumentValueException(String.Format("Image could not be found at {0}", images[i]));
                }
                else if (File.GetAttributes(images[i]).HasFlag(FileAttributes.Directory))
                {
                    throw new ArgumentValueException(String.Format("File found at {0} was a directory instead of an image.", images[i]));
                }
            }
            arguments.CoverImages = images;
            ParseDummyCount(arguments);
        }

    }

}