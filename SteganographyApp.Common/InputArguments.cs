using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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
    /// Singleton utility class to parse the provided array of arguments and return and instance of
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
        /// Takes in a value retrieved from an associated key, parses it, and sets the
        /// relevant property value in the InputArguments instance
        /// </summary>
        /// <param name="args">The InputArguments param to modify.</param>
        /// <param name="value">The value of the key/value pair from the array of arguments.</param>
        public delegate void ValueParser(InputArguments args, string value);

        /// <summary>
        /// A dictionary containing all of the keys and associated delegate methods
        /// to parse user provided values and sets values in the InputArguments instance.
        /// </summary>
        private Dictionary<string, ValueParser> valueProcessors;

        /// <summary>
        /// The singleton instance of the ArgumentParse class
        /// </summary>
        private static ArgumentParser instance;
        public static ArgumentParser Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new ArgumentParser();
                }
                return instance;
            }
        }

        private ArgumentParser()
        {
            valueProcessors = new Dictionary<string, ValueParser>
            {
                { "--action", ParseEncodeOrDecodeAction },
                { "--input", ParseFileToEncode },
                { "--compress", ParseUseCompression },
                { "--printStack", ParsePrintStack },
                { "--images", ParseImages },
                { "--password", (arguments, value) => { arguments.Password = value; } },
                { "--output", (arguments, value) => { arguments.DecodedOutputFile = value; } },
                { "--chunkSize", ParseChunkSize },
                { "--randomSeed", ParseRandomSeed }
            };
        }

        /// <summary>
        /// Parses the array of command line arguments and returns a new instance of InputArguments.
        /// <para>Requires that all arguments be in the format matching --action=decode with a proper key before the = sign
        /// and a non-empty value that meets the validation criteria.</para>
        /// </summary>
        /// <param name="args">The command line arguments provided by the user when launching the application.</param>
        /// <returns>A new InputArgumentsInstance with property values parsed from the user provided arguments.</returns>
        /// <exception cref="ArgumentParseException">Thrown if an argument has an invalid key,
        /// if a required argument was missing or is an exception was thrown while trying to parse the value of the argument.</exception>
        public InputArguments Parse(string[] args)
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

                if(valueProcessors.TryGetValue(key, out ValueParser processor))
                {
                    try
                    {
                        processor(arguments, value);
                    }
                    catch (ArgumentValueException e)
                    {
                        throw new ArgumentParseException(String.Format("An error occured while parsing argument {0}", key), e);
                    }
                }
                else
                {
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
        /// Parses an int32 from the value string and sets the ChunkByteSize property
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of an int value.</param>
        /// <exception cref="ArgumentValueException">Thrown if the string value could not be converted
        /// to an int or if the value of the int is less than or equal to 0.</exception>
        private void ParseChunkSize(InputArguments arguments, string value)
        {
            try
            {
                arguments.ChunkByteSize = Convert.ToInt32(value);
                if(arguments.ChunkByteSize <= 0)
                {
                    throw new ArgumentValueException("The chunk size value must be a positive whole number with a value more than 0.");
                }
            }
            catch(Exception e)
            {
                throw new ArgumentValueException(String.Format("Could not parse chunk value from value {0}", value), e);
            }
        }

        /// <summary>
        /// Parses a boolean from the value and sets the PrintStack property
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify</param>
        /// <param name="value">A string representation of a boolean value.</param>
        /// <exception cref="ArgumentValueException">Thrown if a boolean value could not
        /// be parsed from the value parameter.</exception>
        private void ParsePrintStack(InputArguments arguments, string value)
        {
            try
            {
                arguments.PrintStack = Boolean.Parse(value);
            }
            catch (Exception e)
            {
                throw new ArgumentValueException(String.Format("Could not parse value for argument: {0}", e.Message));
            }
        }

        /// <summary>
        /// Checks if the specified file, value, exists and then sets the
        /// FileToEncode property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The relative or absolute path to the file to encode.</param>
        /// <exception cref="ArgumentValueException">Thrown if the specified input file
        /// could not be found.</exception>
        private void ParseFileToEncode(InputArguments arguments, string value)
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
        private void ParseUseCompression(InputArguments arguments, string value)
        {
            try
            {
                arguments.UseCompression = Boolean.Parse(value);
            }
            catch (Exception e)
            {
                throw new ArgumentValueException(String.Format("Could not parse value for argument: {0}", e.Message));
            }
        }

        /// <summary>
        /// Sets the EncodeDecode action based on the value returned from the Enum.TryParse method.
        /// </summary>
        /// <param name="args">The InputArguments instance to make modifications to.</param>
        /// <param name="value">A string representation of an EncodeDecodeAction enum value.</param>
        /// <exception cref="ArgumentValueException">Thrown if
        /// the string value does not map to an enum value.</exception>
        private void ParseEncodeOrDecodeAction(InputArguments args, string value)
        {
            value = value.Replace("-", "");
            if(!Enum.TryParse(value, true, out EncodeDecodeAction action))
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
        private void ParseImages(InputArguments arguments, string value)
        {
            string regex = "";
            string[] images = null;

            if (value.Contains("[r]"))
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

                regex = parts[0].Replace("<", "");
                string path = parts[1].Replace("<", "");
                string[] files = Directory.GetFiles(path);
                images = new string[files.Length];
                int valid = 0;
                foreach (string name in files)
                {
                    if (Regex.Match(name, regex).Success)
                    {
                        images[valid] = name;
                        valid++;
                    }
                }
                if (valid == 0)
                {
                    throw new ArgumentValueException(String.Format("The provided regex expression returned 0 usable files in the directory {0}", path));
                }
                else if(valid < images.Length)
                {
                    string[] temp = new string[valid];
                    Array.Copy(images, temp, valid);
                    images = temp;
                }
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
        }

        /// <summary>
        /// Takes in the random seed value and validates that it is of the appropriate length.
        /// </summary>
        /// <param name="arguments">The InputArguments instanced to fill with the parse random seed value.</param>
        /// <param name="value">The string representation of the random seed.</param>
        private void ParseRandomSeed(InputArguments arguments, string value)
        {
            if(value.Length > 235 || value.Length < 3)
            {
                throw new ArgumentValueException("The length of the random seed must be between 3 and 235 characters in length.");
            }
            arguments.RandomSeed = value;
        }

        /// <summary>
        /// Takes in the final result of the InputArgument parsing process and validates that all required
        /// arguments has been provided.
        /// </summary>
        /// <param name="input">The InputArguments instance filled with parsed values retrieved
        /// from user provdied arguments.</param>
        /// <returns>Returns a string with information stating what required argument values are missing.
        /// If no values are missing then NoMissingValues will be returned.</returns>
        private string ValidateParseResult(InputArguments input)
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
        public string RandomSeed { get; set; } = "";

        /// <summary>
        /// Specifies the chunk size. I.e. the number of bytes to read, encode,
        /// and write at any given time.
        /// <para>Value of 131,072</para>
        /// </summary>
        public int ChunkByteSize { get; set; } = 131_072;
    }
}
