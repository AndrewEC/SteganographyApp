namespace SteganographyApp.Common.Arguments
{
    using System;

    using SixLabors.ImageSharp.Formats.Png;

    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    internal static class Parsers
    {
        private static readonly string CompressionLevelTemplate = "Level{0}";
        private static readonly int MaxDummyCount = 1000;
        private static readonly int MinDummyCount = 100;

        /// <summary>
        /// Parses a boolean from the parameter value and set sthe EnableLogs property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify. In this case this parameter
        /// is required to be declared but will not be consumed.</param>
        /// <param name="value">The string representation of the EnableLogs boolean flag.</param>
        public static void ParseLogLevel(InputArguments arguments, string value)
        {
            if (!Enum.TryParse(value, true, out LogLevel level))
            {
                throw new ArgumentValueException($"Could not parse log level. Log level must be one of Trace, Debug, or Error.");
            }

            RootLogger.Instance.Enable(level);
        }

        /// <summary>
        /// Parses a boolean from the value parameter and sets the InsertDummies property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the InsertDummies boolean flag.</param>
        public static void ParseInsertDummies(InputArguments arguments, string value)
        {
            arguments.InsertDummies = bool.Parse(value);
        }

        /// <summary>
        /// Parses a boolean from the value parameter and sets the DeleteAfterConversion property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the InsertDummies boolean flag.</param>
        public static void ParseDeleteOriginals(InputArguments arguments, string value)
        {
            arguments.DeleteAfterConversion = bool.Parse(value);
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

            var imageProxy = Injector.Provide<IImageProxy>();

            long dummyCount = 1;
            int[] imageIndexes = new int[] { 0, arguments.CoverImages.Length - 1 };
            foreach (int imageIndex in imageIndexes)
            {
                using (var image = imageProxy.LoadImage(arguments.CoverImages[imageIndex]))
                {
                    dummyCount += dummyCount * (image.Width * image.Height);
                }
            }

            string userRandomSeed = Checks.IsNullOrEmpty(arguments.RandomSeed) ? string.Empty : arguments.RandomSeed;
            string seed = userRandomSeed + dummyCount.ToString();
            arguments.DummyCount = IndexGenerator.FromString(seed).Next(MaxDummyCount - MinDummyCount) + MinDummyCount;
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
            string enumName = string.Format(CompressionLevelTemplate, value);
            if (!Enum.TryParse(enumName, true, out PngCompressionLevel level))
            {
                throw new ArgumentValueException($"Could nto parse compression level. Compression level must be a number between 0 and 9 inclusive.");
            }
            arguments.CompressionLevel = level;
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
            }
            catch (Exception e)
            {
                throw new ArgumentValueException($"Could not parse chunk size from value {value}", e);
            }

            if (arguments.ChunkByteSize <= 0)
            {
                throw new ArgumentValueException("The chunk size value must be a positive whole number with a value more than 0.");
            }
        }

        /// <summary>
        /// Parses a boolean from the value and sets the PrintStack property
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify</param>
        /// <param name="value">A string representation of a boolean value.</param>
        /// <exception cref="ArgumentValueException">Thrown if a boolean value could not
        /// be parsed from the value parameter.</exception>
        public static void ParsePrintStack(InputArguments arguments, string value) => arguments.PrintStack = bool.Parse(value);

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
            if (!Injector.Provide<IFileIOProxy>().IsExistingFile(value))
            {
                throw new ArgumentValueException($"Input file could not be found or is not a file: {value}");
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
            arguments.UseCompression = bool.Parse(value);
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
            value = value.Replace("-", string.Empty);
            if (!Enum.TryParse(value, true, out ActionEnum action))
            {
                throw new ArgumentValueException($"Invalid value for action argument. Expected one of 'encode', 'decode', 'clean', 'calculate-storage-space', 'css', 'calculate-encrypted-size', 'ces' got {value}");
            }
            args.EncodeOrDecode = action;
        }
    }
}