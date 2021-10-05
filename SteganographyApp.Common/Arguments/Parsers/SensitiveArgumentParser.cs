namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Text;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Parser class to help house and parse sensitive/interactive arguments.
    /// </summary>
    public static class SensitiveArgumentParser
    {
        private const string HiddenInputIndicator = "?";
        private const string ConfirmTemplate = "Confirm {0}";
        private const string PasswordPrompt = "Password";
        private const string RandomSeedPrompt = "Random Seed";

        /// <summary>
        /// Attempts to parse the password property. This will either accept the input value or, if the value is
        /// a ? then it will prompt the user to enter the password in an interactive mode.
        /// </summary>
        /// <param name="inputArguments">The input arguments in which the Password property will be set by this method.</param>
        /// <param name="value">The raw value entered by the user as a command line argument.</param>
        public static void ParsePassword(InputArguments inputArguments, string value)
        {
            var password = ReadUserInput(value, PasswordPrompt);
            var confirm = ReadUserInput(value, string.Format(ConfirmTemplate, RandomSeedPrompt));
            if (password != confirm)
            {
                throw new ArgumentValueException("Passwords do not match.");
            }
            inputArguments.Password = password;
        }

        /// <summary>
        /// Attempts to parse the random seed property. This will either accept the input value or, if the value
        /// is an ? then it will prompt the user to enter the password in an interactive mode.
        /// </summary>
        /// <param name="inputArguments">The input arguments in which teh RandomSeed property will be set by this method.</param>
        /// <param name="value">The raw value entered by the user as a command line argument.</param>
        public static void ParseRandomSeed(InputArguments inputArguments, string value)
        {
            var seed = ReadUserInput(value, RandomSeedPrompt);
            if (seed.Length > 235 || seed.Length < 3)
            {
                throw new ArgumentValueException("The length of the random seed must be between 3 and 235 characters in length.");
            }
            var confirm = ReadUserInput(value, string.Format(ConfirmTemplate, RandomSeedPrompt));
            if (seed != confirm)
            {
                throw new ArgumentValueException("Random Seeds do not match.");
            }
            inputArguments.RandomSeed = seed;
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
        private static string ReadUserInput(string value, string messagePrompt)
        {
            var writer = Injector.Provide<IConsoleWriter>();
            var reader = Injector.Provide<IConsoleReader>();
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