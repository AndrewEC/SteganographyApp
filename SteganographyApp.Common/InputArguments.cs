using System;

namespace SteganographyApp.Common
{

    /// <summary>
    /// Enum specifying whether the program is to attempt to proceed with encoding a file
    /// or decoding to an output file.
    /// </summary>
    public enum EncodeDecodeAction
    {
        Encode,
        Decode,
        Clean,
        CalculateStorageSpace,
        CalculateEncryptedSize
    }

    /// <summary>
    /// Specifies that an exception occured while trying to read and parse the command line arguments
    /// or that certain required arguments were not present.
    /// </summary>
    public class ArgumentParseException : Exception
    {
        public ArgumentParseException(string message) : base(message) { }
        public ArgumentParseException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Specifies that a value provided for a particular argument was not valid or could not be properly
    /// parsed into the required data type.
    /// </summary>
    public class ArgumentValueException : Exception
    {
        public ArgumentValueException(string message) : base(message) { }
        public ArgumentValueException(string message, Exception inner) : base(message, inner) { }
    }

    ///<summary>
    /// Static utility class to parse the provided array of arguments and return and instance of
    /// InputArguments with the required values
    ///</summary>
    public class ArgumentParser
    {

        /// <summary>
        /// String returned by the ValidateParseResult method if no validation errors
        /// occured.
        /// </summary>
        private static readonly string NoMissingValues = "NoMissingValues";

        /// <summary>
        /// Parses the array of command line arguments and returns a new instance of InputArguments.
        /// <para>Requires that all arguments be in the format matching --action=decode with a proper key before the = sign
        /// and a non-empty value that meets the validation criteria.</para>
        /// </summary>
        /// <param name="args">The command line arguments provided by the user when launching the application.</param>
        /// <returns>A new InputArgumentsInstance with property values parsed from the user provided arguments.</returns>
        /// <exception cref="ArgumentParseException">Thrown if an argument has an invalid key,
        /// if a required argument was missing or is an exception was thrown while trying to parse the value of the argument.</exception>
        public static InputArguments Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentParseException("No arguments provided to parse.");
            }

            var arguments = new InputArguments();

            foreach (string arg in args)
            {
                if (!arg.Contains("="))
                {
                    throw new ArgumentParseException(String.Format("Invalid argument {0}. Did not contain required = sign.", arg));
                }

                var key = arg.Split('=')[0].Trim();
                var value = arg.Split('=')[1].Trim();

                if (key == "")
                {
                    throw new ArgumentParseException("Argument name was empty when it is required.");
                }

                if (value == "")
                {
                    throw new ArgumentParseException(String.Format("Value for argument {0} was empty. A value must be provided for all specified arguments.", key));
                }

                switch (key)
                {
                    case "--action":
                    case "-a":
                        try
                        {
                            arguments.EncodeOrDecode = ParseEncodeOrDecodeAction(value);
                        }
                        catch (ArgumentValueException e)
                        {
                            throw new ArgumentParseException(String.Format("An error occured while parsing argument {0}", key), e);
                        }
                        break;
                    case "--input":
                    case "-i":
                        arguments.FileToEncode = value;
                        if (!System.IO.File.Exists(value))
                        {
                            throw new ArgumentParseException(String.Format("File to decode could not be found at {0}", value));
                        }
                        break;
                    case "--output":
                    case "-o":
                        arguments.DecodedOutputFile = value;
                        break;
                    case "--images":
                    case "-im":
                        try
                        {
                            arguments.CoverImages = ParseImages(value);
                        }
                        catch (ArgumentValueException e)
                        {
                            throw new ArgumentParseException(String.Format("An error occured while parsing argument {0}", key), e);
                        }
                        break;
                    case "--password":
                    case "-p":
                        arguments.Password = value;
                        break;
                    case "--printStack":
                    case "-ps":
                        try
                        {
                            arguments.PrintStack = Boolean.Parse(value);
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentParseException(String.Format("Could not parse value for argument {0}", key), e);
                        }
                        break;
                    case "--compress":
                    case "-c":
                        try
                        {
                            arguments.UseCompression = Boolean.Parse(value);
                        }
                        catch (Exception e)
                        {
                            throw new ArgumentParseException(String.Format("Could not parse value for argument {0}", key), e);
                        }
                        break;
                    default:
                        throw new ArgumentParseException(String.Format("An invalid key value was provided: {0}", key));
                }
            }

            string validation = ValidateParseResult(arguments);
            if (validation != NoMissingValues)
            {
                throw new ArgumentParseException(String.Format("Missing required values. {0}", validation));
            }

            return arguments;
        }

        /// <summary>
        /// Takes in a value for the --length parameter and attempts to parse the
        /// string to an int where int >= 8 and int % 8 == 0
        /// </summary>
        /// <param name="value">A string containing a integer representation of the
        /// number of bits to decode.</param>
        /// <returns>An int value that is > 8 and a multiple of 8</returns>
        /// <exception cref="ArgumentValueException">Thrown if the string value cannot be
        /// parsed into an int, if the parsed value is less then 8, or if the value is not a multiple of 8.</exception>
        private static int ParseDecodeLength(string value)
        {
            try
            {
                int length = Convert.ToInt32(value);
                if (length < 8 || length % 8 != 0)
                {
                    throw new ArgumentValueException("Decode value must be a whole number that is 8 or greater and evenly divisiable by 8.");
                }
                return length;
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(ArgumentValueException))
                {
                    throw e;
                }
                throw new ArgumentValueException("Not a valid 32bit Int value.");
            }
        }

        /// <summary>
        /// Takes in a string representation of the EncodeDecodeAction and returns an instance of
        /// EncodeDecodeAction.
        /// </summary>
        /// <param name="value">A string representation of an EncodeDecodeAction enum value.</param>
        /// <returns>A enum value of type EncodeDecodeAction as determined by the input string.</returns>
        /// <exception cref="ArgumentValueException">Thrown if
        /// the string value does not map to an enum value.</exception>
        private static EncodeDecodeAction ParseEncodeOrDecodeAction(string value)
        {
            EncodeDecodeAction action;
            if (value.ToLower() == "encode")
            {
                action = EncodeDecodeAction.Encode;
            }
            else if (value.ToLower() == "decode")
            {
                action = EncodeDecodeAction.Decode;
            }
            else if (value.ToLower() == "calculate-storage-space")
            {
                action = EncodeDecodeAction.CalculateStorageSpace;
            }
            else if (value.ToLower() == "calculate-encrypted-size")
            {
                action = EncodeDecodeAction.CalculateEncryptedSize;
            }
            else if (value.ToLower() == "clean")
            {
                action = EncodeDecodeAction.Clean;
            }
            else
            {
                throw new ArgumentValueException(String.Format("Invalid value for action argument. Expected 'encode', 'decode', 'clean', 'calculate-storage-space', or 'calculate-encrypted-size' got {0}", value));
            }
            return action;
        }

        /// <summary>
        /// Takes in a string of comma delimited image names and returns an array of strings.
        /// </summary>
        /// <param name="value">A string representation of a number, or a single, image where encoded
        /// data will be writted to or where decoded data will be read from.</param>
        /// <returns>A string array containing the names of all the images. If no comma was specified
        /// in the string then an array with a length of 1 will be returned.</returns>
        /// <exception cref="ArgumentValueException">Thrown if the image
        /// could not be found at the specified path.</exception>
        private static string[] ParseImages(string value)
        {
            string[] images;
            if (value.Contains(","))
            {
                images = value.Split(',');
            }
            else
            {
                images = new string[1];
                images[0] = value;
            }

            for (int i = 0; i < images.Length; i++)
            {
                images[i] = images[i].Trim();
                if (!System.IO.File.Exists(images[i]))
                {
                    throw new ArgumentValueException(String.Format("Image could not be found at {0}", images[i]));
                }
            }
            return images;
        }

        /// <summary>
        /// Takes in the final result of the InputArgument parsing process and validates that all required
        /// arguments has been provided.
        /// </summary>
        /// <param name="input">The InputArguments instance created a filled with parsed values retrieved
        /// from user provdied arguments.</param>
        /// <returns>Returns a string with information stating what required argument values are missing.
        /// If no values are missing then NoMissingValues will be returned.</returns>
        private static string ValidateParseResult(InputArguments input)
        {
            if (input.EncodeOrDecode == EncodeDecodeAction.Encode)
            {
                if (input.FileToEncode == null || input.FileToEncode.Length == 0)
                {
                    return "Specified encode action but no file to encode was provided in arguments.";
                }
            }
            else if (input.EncodeOrDecode == EncodeDecodeAction.Decode)
            {
                if (input.DecodedOutputFile == null || input.DecodedOutputFile.Length == 0)
                {
                    return "Specified decode action but no file to decode was provided in arguments.";
                }
            }
            else if (input.EncodeOrDecode == EncodeDecodeAction.CalculateEncryptedSize)
            {
                if (input.FileToEncode == null || input.FileToEncode.Length == 0)
                {
                    return "A file must be specified in order to calculate the encrypted file size.";
                }
            }

            if (input.EncodeOrDecode != EncodeDecodeAction.CalculateEncryptedSize)
            {
                if (input.CoverImages == null || input.CoverImages.Length == 0)
                {
                    if (input.EncodeOrDecode == EncodeDecodeAction.CalculateStorageSpace)
                    {
                        return "One or more images must be specified in order to calculate storage space.";
                    }
                    else
                    {
                        return "No cover images were provided in arguments.";
                    }
                }
            }
            return NoMissingValues;
        }
    }

    /// <summary>
    /// Contains a number of properties that will contain values parsed from the user provided command line arguments.
    /// </summary>
    public class InputArguments
    {
        public string Password { get; set; } = "";
        public string FileToEncode { get; set; } = "";
        public string DecodedOutputFile { get; set; } = "";
        public string[] CoverImages { get; set; }
        public EncodeDecodeAction EncodeOrDecode { get; set; }
        public bool PrintStack { get; set; } = false;
        public bool UseCompression { get; set; } = false;
    }
}
