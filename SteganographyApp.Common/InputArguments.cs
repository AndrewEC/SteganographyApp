using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SteganographyApp.Common.Data;

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
        /// Encapsulates information about an argument that the user can specify when invoking the
        /// tool.
        /// </summary>
        public class Argument
        {
            public string Name { get; private set; }
            public string ShortName { get; private set; }
            public ValueParser Parser { get; private set; }
            public bool IsFlag { get; private set; }

            public Argument(string name, string shortName, ValueParser parser, bool flag=false)
            {
                Name = name;
                ShortName = shortName;
                Parser = parser;
                IsFlag = flag;
            }
        }

        /// <summary>
        /// The list of user providable arguments.
        /// </summary>
        private readonly List<Argument> arguments;

        public Exception LastError { get; private set; }

        public ArgumentParser()
        {
            arguments = new List<Argument>()
            {
                new Argument("--action", "-a", ParseEncodeOrDecodeAction),
                new Argument("--input", "-in", ParseFileToEncode),
                new Argument("--enableCompression", "-c", ParseUseCompression, true),
                new Argument("--printStack", "-stack", ParsePrintStack, true),
                new Argument("--images", "-im", ParseImages),
                new Argument("--password", "-p", ParsePassword),
                new Argument("--output", "-o", (arguments, value) => { arguments.DecodedOutputFile = value; }),
                new Argument("--chunkSize", "-cs", ParseChunkSize),
                new Argument("--randomSeed", "-rs", ParseRandomSeed),
                new Argument("--enableDummies", "-d", ParseInsertDummies, true)
            };
        }

        /// <summary>
        /// Attempts to lookup an Argument instance from the list of arguments.
        /// <para>The key value to lookup from can either be the regular argument name or
        /// the arguments short name.</para>
        /// </summary>
        /// <param name="key">The name of the argument to find. This can either be the arguments name or the
        /// arguments short name</param>
        /// <param name="argument">The Argument instance to be provided if found. If not found this value
        /// will be null.</param>
        /// <returns>True if the argument could be found else false.</returns>
        private bool TryGetArgument(string key, out Argument argument)
        {
            foreach(Argument arg in arguments)
            {
                if(arg.Name == key || arg.ShortName == key)
                {
                    argument = arg;
                    return true;
                }
            }
            argument = null;
            return false;
        }

        /// <summary>
        /// Attempts to parser the command line arguments into a usable
        /// <see cref="InputArguments"/> instance.
        /// <para>If the parsing or validation of the arguments fails then
        /// this method will return false and the LastError attribute will be set.</para>
        /// </summary>
        /// <param name="args">The array of command line arguments to parse.</param>
        /// <param name="inputs">The <see cref="InputArguments"/> instance containing the parsed
        /// argument values.</param>
        /// <returns>True if all the arguments provided were parsed and the validation was successful
        /// else returns false.</returns>
        public bool TryParse(string[] args, out InputArguments inputs)
        {
            inputs = new InputArguments();
            if(args == null || args.Length == 0)
            {
                LastError = new ArgumentParseException("No arguments provided to parse.");
                return false;
            }

            ValueTuple<string, Argument> password = ("", null);
            ValueTuple<string, Argument> randomSeed = ("", null);

            for (int i = 0; i < args.Length; i++)
            {
                if (!TryGetArgument(args[i], out Argument argument))
                {
                    LastError = new ArgumentParseException(string.Format("An unrecognized argument was provided: {0}", args[i]));
                    return false;
                }

                //Retrieve the password and randomSeed values to process them last as they may
                //require interactive input in which we should throw any validations error available before
                //requesting further user input.
                if (argument.Name == "--password" || argument.Name == "--randomSeed")
                {
                    if (i + 1 >= args.Length)
                    {
                        LastError = new ArgumentParseException(string.Format("Missing required value for ending argument: {0}", args[i]));
                        return false;
                    }
                    if (argument.Name == "--password")
                    {
                        password = (args[i + 1], argument);
                    }
                    else if (argument.Name == "--randomSeed")
                    {
                        randomSeed = (args[i + 1], argument);
                    }
                    i++;
                    continue;
                }

                if (argument.IsFlag)
                {
                    try
                    {
                        argument.Parser(inputs, "true");
                    }
                    catch (Exception e)
                    {
                        LastError = new ArgumentParseException(string.Format("Invalid value provided for argument: {0}", args[i]), e);
                        return false;
                    }
                }
                else
                {
                    if (i + 1 >= args.Length)
                    {
                        LastError = new ArgumentParseException(string.Format("Missing required value for ending argument: {0}", args[i]));
                        return false;
                    }
                    try
                    {
                        argument.Parser(inputs, args[i + 1]);
                        i++;
                    }
                    catch (Exception e)
                    {
                        LastError = new ArgumentParseException(string.Format("Invalid value provided for argument: {0}", args[i]), e);
                        return false;
                    }
                }
            }

            string validation = ValidateParseResult(inputs);
            if (validation != NoMissingValues)
            {
                LastError = new ArgumentParseException(string.Format("Missing required values. {0}", validation));
                return false;
            }

            if (password.Item2 != null)
            {
                password.Item2.Parser(inputs, password.Item1);
            }
            if (randomSeed.Item2 != null)
            {
                randomSeed.Item2.Parser(inputs, randomSeed.Item1);
            }

            return true;
        }

        /// <summary>
        /// Attempts to retrieve the user's input without displaying the input on screen.
        /// </summary>
        /// <param name="value">The original value for the current argument the user provided.
        /// If this value is a question mark then this will invoke the ReadKey method and record
        /// input until the enter key has been pressed and return the result without presenting
        /// the resulting value on screen.</param>
        /// <param name="message">The argument to prompt the user to enter.</param>
        /// <returns>Either the original value string value or the value of the user's input
        /// if the original value string value was a question mark.</returns>
        private string ReadString(string value, string message)
        {
            if(value == "?")
            {
                Console.Write("Enter {0}: ", message);
                var builder = new StringBuilder();
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return builder.ToString();
                    }
                    else if (key.Key == ConsoleKey.Backspace && builder.Length > 0)
                    {
                        builder.Remove(builder.Length - 1, 1);
                    }
                    else
                    {
                        builder.Append(key.KeyChar);
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parses a boolean from the value parameter and sets the InsertDummies property.
        /// Will also attempt to call <see cref="ParseDummyCount"/>
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the InsertDummies boolean flag.</param>
        private void ParseInsertDummies(InputArguments arguments, string value)
        {
            try
            {
                arguments.InsertDummies = Boolean.Parse(value);
                ParseDummyCount(arguments);
            }
            catch (Exception e)
            {
                throw new ArgumentValueException(string.Format("Could not parse insert dummies from value: {0}", value), e);
            }
        }

        /// <summary>
        /// Parses the number of dummy entries to insert into the image based on the
        /// properties retrieved from the first and last images.
        /// <para>Note: the first and last image can be the same image if only one cover image
        /// was provided.</para>
        /// </summary>
        private void ParseDummyCount(InputArguments arguments)
        {
            if (!arguments.InsertDummies || arguments.CoverImages == null)
            {
                return;
            }
            
            long dummyCount = 1;
            int[] points = new int[] { 0, arguments.CoverImages.Length - 1 };
            foreach (int point in points)
            {
                using(Image<Rgba32> image = Image.Load(arguments.CoverImages[point]))
                {
                    dummyCount += dummyCount * (image.Width * image.Height);
                }
            }
            String seed = dummyCount.ToString();
            arguments.DummyCount = IndexGenerator.FromString(seed).Next(10);
        }

        /// <summary>
        /// Retrieves the encryption password by invoking the <see cref="ReadString"/> method to retrieve
        /// either the user input or the original string value.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the password</param>
        private void ParsePassword(InputArguments arguments, string value)
        {
            arguments.Password = ReadString(value, "Password");
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
            string[] images = null;

            if (value.Contains("[r]"))
            {
                images = ImagesFromRegex(value);
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
        private string[] ImagesFromRegex(string value)
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
        private (string, string) ParseRegex(string value)
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

        /// <summary>
        /// Takes in the random seed value by invoking the <see cref="ReadString"/> method and validating the
        /// input is of a valid length.
        /// </summary>
        /// <param name="arguments">The InputArguments instanced to fill with the parse random seed value.</param>
        /// <param name="value">The string representation of the random seed.</param>
        private void ParseRandomSeed(InputArguments arguments, string value)
        {
            var seed = ReadString(value, "Random Seed");
            if(seed.Length > 235 || seed.Length < 3)
            {
                throw new ArgumentValueException("The length of the random seed must be between 3 and 235 characters in length.");
            }
            arguments.RandomSeed = seed;
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
        public int DummyCount { get; set; } = 0;
        public bool InsertDummies { get; set; } = false;

        /// <summary>
        /// Specifies the chunk size. I.e. the number of bytes to read, encode,
        /// and write at any given time.
        /// <para>Higher values will improve the time to encode a larger file and reduce
        /// ther overall encoded file size though values too high run the risk of
        /// having memory related errors.</para>
        /// <para>Value of 131,072</para>
        /// </summary>
        public int ChunkByteSize { get; set; } = 131_072;
    }
}
