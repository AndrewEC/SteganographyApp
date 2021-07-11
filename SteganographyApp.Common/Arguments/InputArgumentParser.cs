namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;

    using SteganographyApp.Common.Injection;

    public struct ReadWriteUtils
    {
        public IConsoleReader Reader { get; set; }

        public IConsoleWriter Writer { get; set; }
    }

    ///<summary>
    /// Singleton utility class to parse the provided array of arguments and return and instance of
    /// InputArguments with the required values
    ///</summary>
    public sealed class ArgumentParser
    {
        /// <summary>
        /// The list of user providable arguments.
        /// </summary>
        private readonly ImmutableList<Argument> arguments;

        private readonly SensitiveArgumentParser sensitiveArgumentParser;

        public ArgumentParser()
        {
            sensitiveArgumentParser = new SensitiveArgumentParser();

#pragma warning disable SA1009
            arguments = ImmutableList.Create(
                new Argument("--action", "-a", Parsers.ParseEncodeOrDecodeAction),
                new Argument("--input", "-in", Parsers.ParseFileToEncode),
                new Argument("--enableCompression", "-c", Parsers.ParseUseCompression, true),
                new Argument("--printStack", "-stack", Parsers.ParsePrintStack, true),
                new Argument("--images", "-im", ImagePathParser.ParseImages),
                new Argument("--password", "-p", sensitiveArgumentParser.ParsePassword),
                new Argument("--output", "-o", (arguments, value) => { arguments.DecodedOutputFile = value; }),
                new Argument("--chunkSize", "-cs", Parsers.ParseChunkSize),
                new Argument("--randomSeed", "-rs", sensitiveArgumentParser.ParseRandomSeed),
                new Argument("--enableDummies", "-d", Parsers.ParseInsertDummies, true),
                new Argument("--deleteOriginals", "-do", Parsers.ParseDeleteOriginals, true),
                new Argument("--compressionLevel", "-cl", Parsers.ParseCompressionLevel),
                new Argument("--logLevel", "-ll", Parsers.ParseLogLevel)
            );
#pragma warning restore SA1009
        }

        /// <summary>
        /// The last exception to ocurr while parsing the argument values.
        /// </summary>
        public Exception LastError { get; private set; }

        /// <summary>
        /// A utility method to help print a common error message when parsing the user's arguments fails.
        /// </summary>
        public void PrintCommonErrorMessage()
        {
            var writer = Injector.Provide<IConsoleWriter>();
            writer.WriteLine($"An exception occured while parsing provided arguments: {LastError.Message}");
            var exception = LastError.InnerException;
            while (exception != null)
            {
                writer.WriteLine($"Caused by: {exception.Message}");
                exception = exception.InnerException;
            }
            writer.WriteLine("\nRun the program with --help to get more information.");
        }

        /// <summary>
        /// Attempts to parser the command line arguments into a usable
        /// <see cref="IInputArguments"/> instance.
        /// <para>If the parsing or validation of the arguments fails then
        /// this method will return false and the LastError attribute will be set.</para>
        /// </summary>
        /// <param name="args">The array of command line arguments to parse.</param>
        /// <param name="inputs">The <see cref="IInputArguments"/> instance containing the parsed
        /// argument values to be set during the execution of this method.</param>
        /// <param name="validation">The post validation delegate that will validate if all the
        /// resulting argument/value pairings at the end of parsing all provided arguments are correct.</param>
        /// <returns>True if all the arguments provided were parsed and the validation was successful
        /// else returns false.</returns>
        public bool TryParse(string[] args, out IInputArguments inputs, PostValidation validation)
        {
            try
            {
                inputs = DoTryParse(args, validation);
                return true;
            }
            catch (Exception e)
            {
                LastError = e;
                inputs = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to lookup an Argument instance from the list of arguments.
        /// <para>The key value to lookup from can either be the regular argument name or
        /// the arguments short name.</para>
        /// </summary>
        /// <param name="key">The name of the argument to find. This can either be the arguments name or the
        /// arguments short name</param>
        /// <param name="targetArgument">The Argument instance to be provided if found. If not found this value
        /// will be null.</param>
        /// <returns>True if the argument could be found else false.</returns>
        private bool TryGetArgument(string key, out Argument targetArgument)
        {
            foreach (var argument in arguments)
            {
                if (argument.Name == key || argument.ShortName == key)
                {
                    targetArgument = argument;
                    return true;
                }
            }
            targetArgument = null;
            return false;
        }

        private IInputArguments DoTryParse(string[] userArguments, PostValidation postValidationMethod)
        {
            if (userArguments == null || userArguments.Length == 0)
            {
                throw new ArgumentParseException("No arguments provided.");
            }

            var parsedArguments = new InputArguments();

            for (int i = 0; i < userArguments.Length; i++)
            {
                if (!TryGetArgument(userArguments[i], out Argument argument))
                {
                    throw new ArgumentParseException($"An unrecognized argument was provided: {userArguments[i]}");
                }

                if (sensitiveArgumentParser.IsSensitiveArgument(argument))
                {
                    sensitiveArgumentParser.CaptureArgument(argument, userArguments, i);
                    i++;
                    continue;
                }

                string inputValue = GetRawArgumentValue(argument, userArguments, i);
                ParseArgument(argument, parsedArguments, inputValue);

                if (!argument.IsFlag)
                {
                    i++;
                }
            }

            InvokePostValidation(postValidationMethod, parsedArguments);

            sensitiveArgumentParser.ParseSecureArguments(parsedArguments);

            Parsers.ParseDummyCount(parsedArguments);

            Injector.LoggerFor<ArgumentParser>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(parsedArguments, parsedArguments.GetType()) });

            return parsedArguments.ToImmutable();
        }

        /// <summary>
        /// Retrieves the raw unparsed value that corresponds to a given argument from the set
        /// of command line arguments passed to the program.
        /// </summary>
        private string GetRawArgumentValue(Argument argument, string[] userArguments, int i)
        {
            if (argument.IsFlag)
            {
                return "true";
            }
            else
            {
                if (i + 1 >= userArguments.Length)
                {
                    throw new ArgumentParseException($"Missing required value for ending argument: {userArguments[i]}");
                }
                return userArguments[i + 1];
            }
        }

        /// <summary>
        /// Invokes the custom post validation function.
        /// <para>If the invocation returns a non-null or non-empty string then an ArgumentParseException
        /// will be thrown.</para>
        /// </summary>
        private void InvokePostValidation(PostValidation validation, InputArguments parsed)
        {
            try
            {
                string validationResult = validation(parsed);
                if (!Checks.IsNullOrEmpty(validationResult))
                {
                    throw new ArgumentParseException($"Invalid arguments provided. {validationResult}");
                }
            }
            catch (Exception e) when (!(e is ArgumentParseException))
            {
                throw new ValidationException($"An error occurred while validating your input: {e.Message}", e);
            }
        }

        private void ParseArgument(Argument argument, InputArguments parsedArguments, string rawInput)
        {
            try
            {
                argument.Parser(parsedArguments, rawInput);
            }
            catch (Exception e)
            {
                throw new ArgumentParseException($"Invalid value provided for argument: {argument.Name}", e);
            }
        }
    }
}
