namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text.Json;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Singleton utility class to parse the provided array of arguments and return and instance of
    /// InputArguments with the required values.
    /// </summary>
    public sealed class ArgumentParser
    {
        /// <summary>
        /// Gets last exception to ocurr while parsing the argument values.
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
        /// <exception cref="ArgumentParseException">Thrown if an exception ocurrs while parsing a given argument.
        /// The cause of the exception will be set to the original exception causing the parsing failure.</exception>
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

        private IInputArguments DoTryParse(string[] userArguments, PostValidation postValidationMethod)
        {
            if (userArguments == null || userArguments.Length == 0)
            {
                throw new ArgumentParseException("No arguments provided.");
            }

            var parsedArguments = new InputArguments();
            var container = new ArgumentContainer(userArguments);

            ParseArguments(container.GetAllNonSensitiveArguments(), parsedArguments);

            InvokePostValidation(postValidationMethod, parsedArguments);

            ParseArguments(container.GetAllSensitiveArguments(), parsedArguments);

            Parsers.ParseDummyCount(parsedArguments);

            Injector.LoggerFor<ArgumentParser>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(parsedArguments) });

            return parsedArguments.ToImmutable();
        }

        private void ParseArguments(ImmutableDictionary<Argument, string> selected, InputArguments parsedArguments)
        {
            selected.ToList().ForEach(argumentAndValue => ParseArgument(argumentAndValue.Key, parsedArguments, argumentAndValue.Value));
        }

        private void InvokePostValidation(PostValidation validation, InputArguments parsed)
        {
            try
            {
                string validationResult = validation(parsed);
                if (!string.IsNullOrEmpty(validationResult))
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
