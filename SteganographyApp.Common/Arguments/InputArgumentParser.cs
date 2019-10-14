using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Arguments
{

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
        /// Takes in a value retrieved from an associated key, parses it, and sets the
        /// relevant property value in the InputArguments instance
        /// </summary>
        /// <param name="args">The InputArguments param to modify.</param>
        /// <param name="value">The value of the key/value pair from the array of arguments.</param>
        delegate void ValueParser(InputArguments args, string value);

        /// <summary>
        /// Takes in the collected set of argument/value pairs and performs a final validation
        /// on them.
        /// <para>If the string returned is neither null or empty than then the validation is treated
        /// as a failure.</para>
        /// </summary>
        /// <param name="args">The InputArguments and all their associated values.</param>
        public delegate string PostValidation(IInputArguments args);

        /// <summary>
        /// Encapsulates information about an argument that the user can specify when invoking the
        /// tool.
        /// </summary>
        class Argument
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
                new Argument("--password", "-p", (arguments, value) => { arguments.Password = ReadString(value, "Password"); }),
                new Argument("--output", "-o", (arguments, value) => { arguments.DecodedOutputFile = value; }),
                new Argument("--chunkSize", "-cs", ParseChunkSize),
                new Argument("--randomSeed", "-rs", ParseRandomSeed),
                new Argument("--enableDummies", "-d", ParseInsertDummies, true),
                new Argument("--deleteOriginals", "-do", ParseDeleteOriginals, true),
                new Argument("--compressionLevel", "-co", ParseCompressionLevel)
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
        /// <see cref="IInputArguments"/> instance.
        /// <para>If the parsing or validation of the arguments fails then
        /// this method will return false and the LastError attribute will be set.</para>
        /// </summary>
        /// <param name="args">The array of command line arguments to parse.</param>
        /// <param name="inputs">The <see cref="IInputArguments"/> instance containing the parsed
        /// argument values.</param>
        /// <param name="validation">The post validation delegate that will validate if all the
        /// resulting argument/value pairings at the end of parsing all provided arguments are correct.</param>
        /// <returns>True if all the arguments provided were parsed and the validation was successful
        /// else returns false.</returns>
        public bool TryParse(string[] args, out IInputArguments inputs, PostValidation validation)
        {
            InputArguments parsed = new InputArguments();
            inputs = null;
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
                        argument.Parser(parsed, "true");
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
                        argument.Parser(parsed, args[i + 1]);
                        i++;
                    }
                    catch (Exception e)
                    {
                        LastError = new ArgumentParseException(string.Format("Invalid value provided for argument: {0}", args[i]), e);
                        return false;
                    }
                }
            }

            string validationResult = validation(parsed);
            if (validationResult != null && validationResult.Length != 0)
            {
                LastError = new ArgumentParseException(string.Format("Invalid arguments provided. {0}", validationResult));
                return false;
            }

            if(!TryParseSecureItems(parsed, password, randomSeed))
            {
                return false;
            }

            inputs = parsed.ToImmutable();
            return true;
        }

        private bool TryParseSecureItems(InputArguments inputs, ValueTuple<string, Argument> password, ValueTuple<string, Argument> randomSeed)
        {
            return TryParseSecureItem(inputs, password, "--password") && TryParseSecureItem(inputs, randomSeed, "--randomSeed");
        }

        /// <summary>
        /// Utility method containing the common logic for parsing the random seed and the password parameters.
        /// </summary>
        private bool TryParseSecureItem(InputArguments inputArguments, ValueTuple<string, Argument> argument, string argumentName)
        {
            if (argument.Item1 == null || argument.Item2 == null)
            {
                return true;
            }

            try
            {
                argument.Item2.Parser(inputArguments, argument.Item1);
            }
            catch (Exception e)
            {
                LastError = new ArgumentParseException(string.Format("Invalid value provided for argument: {0}", argumentName), e);
                return false;
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
        /// Parses a boolean from the value parameter and sets the DeleteAfterConversion property.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of the InsertDummies boolean flag.</param>
        private void ParseDeleteOriginals(InputArguments arguments, string value)
        {
            try
            {
                arguments.DeleteAfterConversion = Boolean.Parse(value);
            }
            catch (Exception e)
            {
                throw new ArgumentValueException(string.Format("Could not parse delete originals from value: {0}", value), e);
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
        /// Parses the compression level and sets the CompressionLevel property value.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to modify.</param>
        /// <param name="value">The string representation of an int value.</param>
        /// <exception cref="ArgumentValueException">Thrown if the string value could not be converted
        /// to an int or if the value is less than 0 or greater than 9.</exception>
        private void ParseCompressionLevel(InputArguments arguments, string value)
        {
            try
            {
                arguments.CompressionLevel = Convert.ToInt32(value);
                if (arguments.CompressionLevel < 0 || arguments.CompressionLevel > 9)
                {
                    throw new ArgumentValueException(string.Format("The compression level must be a whole number between 0 and 9 inclusive."));
                }
            }
            catch (Exception e)
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
        private void ParseImages(InputArguments arguments, string value)
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
        /// A utility method to help print a common error message when parsing the user's arguments fails.
        /// </summary>
        public void PrintCommonErrorMessage()
        {
            Console.WriteLine("An exception occured while parsing provided arguments: {0}", LastError.Message);
            var exception = LastError;
            while (exception.InnerException != null)
            {
                Console.WriteLine("Caused by: {0}", LastError.InnerException.Message);
                exception = exception.InnerException;
            }
            Console.WriteLine("\nRun the program with --help to get more information.");
        }
    }
}
