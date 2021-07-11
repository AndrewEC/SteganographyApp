namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Text;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Parser class to help house and parse sensitive/interactive arguments.
    /// </summary>
    public sealed class SensitiveArgumentParser
    {
        private static readonly string HiddenInputIndicator = "?";

        private static readonly string ConfirmTemplate = "Confirm {0}";
        private static readonly string PasswordPrompt = "Password";
        private static readonly string RandomSeedPrompt = "Random Seed";
        private static readonly string PasswordName = "--password";
        private static readonly string RandomSeedName = "--randomSeed";

        private readonly IConsoleWriter writer;
        private readonly IConsoleReader reader;

        private ValueTuple<string, Argument> password;
        private ValueTuple<string, Argument> randomSeed;

        public SensitiveArgumentParser()
        {
            writer = Injector.Provide<IConsoleWriter>();
            reader = Injector.Provide<IConsoleReader>();
        }

        public bool IsSensitiveArgument(Argument argument) => argument.Name == PasswordName || argument.Name == RandomSeedName;

        /// <summary>
        /// Captures either of the password or random seed arguments so it can be
        /// interactively parsed after all other parsing and validation has occurred.
        /// </summary>
        public void CaptureArgument(Argument argument, string inputValue)
        {
            if (argument.Name == PasswordName)
            {
                password = (inputValue, argument);
            }
            else if (argument.Name == RandomSeedName)
            {
                randomSeed = (inputValue, argument);
            }
        }

        /// <summary>
        /// Attempts to parse the password and randomSeed arguments and raises any exceptions that may occur
        /// if either argument cannot be properly parsed.
        /// </summary>
        public void ParseSecureArguments(InputArguments inputArguments)
        {
            TryParseSecureItem(inputArguments, password, PasswordName);
            TryParseSecureItem(inputArguments, randomSeed, RandomSeedName);
        }

        /// <summary>
        /// Takes in the random seed value by invoking the <see cref="ReadUserInput"/> method and validating the
        /// input is of a valid length.
        /// </summary>
        /// <param name="arguments">The InputArguments instanced to fill with the parse random seed value.</param>
        /// <param name="value">The string representation of the random seed.</param>
        public void ParseRandomSeed(InputArguments arguments, string value)
        {
            var seed = ReadUserInput(value, RandomSeedPrompt);
            if (seed.Length > 235 || seed.Length < 3)
            {
                throw new ArgumentValueException("The length of the random seed must be between 3 and 235 characters in length.");
            }
            var confirm = ReadUserInput(value, string.Format(ConfirmTemplate, RandomSeedPrompt));
            if (seed != confirm)
            {
                throw new ArgumentValueException("Random Seeds do not match");
            }
            arguments.RandomSeed = seed;
        }

        /// <summary>
        /// Parses the password value using the <see cref="ReadUserInput"/> method.
        /// </summary>
        /// <param name="arguments">The InputArguments instance to insert the password into.</param>
        /// <param name="value">The string representation of the password</param>
        public void ParsePassword(InputArguments arguments, string value)
        {
            var password = ReadUserInput(value, PasswordPrompt);
            var confirm = ReadUserInput(value, string.Format(ConfirmTemplate, RandomSeedPrompt));
            if (password != confirm)
            {
                throw new ArgumentValueException("Passwords do not match.");
            }
            arguments.Password = password;
        }

        /// <summary>
        /// Utility method containing the common logic for parsing the random seed and the password parameters.
        /// </summary>
        private void TryParseSecureItem(InputArguments inputArguments, ValueTuple<string, Argument> argument, string argumentName)
        {
            if (argument.Item1 == null || argument.Item2 == null)
            {
                return;
            }

            try
            {
                argument.Item2.Parser(inputArguments, argument.Item1);
            }
            catch (Exception e)
            {
                throw new ArgumentParseException($"Invalid value provided for argument: {argumentName}", e);
            }
        }

        /// <summary>
        /// Attempts to retrieve the user's input without displaying the input on screen.
        /// </summary>
        /// <param name="value">The original value for the current argument the user provided.
        /// If this value is a question mark then this will invoke the ReadKey method and record
        /// input until the enter key has been pressed and return the result without presenting
        /// the resulting value on screen.</param>
        /// <param name="messagePrompt">The argument to prompt the user to enter.</param>
        /// <returns>Either the original value string value or the value of the user's input
        /// if the original value string value was a question mark.</returns>
        private string ReadUserInput(string value, string messagePrompt)
        {
            if (value == HiddenInputIndicator)
            {
                writer.Write($"Enter {messagePrompt}: ");
                var currentUserInput = new StringBuilder();
                while (true)
                {
                    var key = reader.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        writer.WriteLine(string.Empty);
                        return currentUserInput.ToString();
                    }
                    else if (key.Key == ConsoleKey.Backspace && currentUserInput.Length > 0)
                    {
                        currentUserInput.Remove(currentUserInput.Length - 1, 1);
                    }
                    else
                    {
                        currentUserInput.Append(key.KeyChar);
                    }
                }
            }
            return value;
        }
    }
}