namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Text;

    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Static utility class to allow the user to enter in secure parameters in an interactive manner.
    /// </summary>
    public static class SecureParser
    {
        private const string SecureReadIndicator = "?";

        /// <summary>
        /// Allows the user to securely enter a sensitive value in an interactive input. If the value of userInput
        /// is a question mark then this will allow the user to enter a new value which will be subsequently returned.
        /// If the value of the original userInput is not a question mark then the original value will be returned.
        /// </summary>
        /// <param name="prompt">The text to display to indicate what value the user is entering.</param>
        /// <param name="userInput">The user's input. If the original input is a question mark this method will
        /// allow the user to interactively input a value, otherwise, this will return the original value of this parameter.</param>
        /// <returns>If the value of userInput
        /// is a question mark then this will allow the user to enter a new value which will be subsequently returned.
        /// If the value of the original userInput is not a question mark then the original value will be returned.</returns>
        public static string ReadUserInput(string prompt, string userInput)
        {
            if (userInput != SecureReadIndicator)
            {
                return userInput;
            }

            var reader = Injector.Provide<IConsoleReader>();
            var writer = Injector.Provide<IConsoleWriter>();

            writer.Write(prompt);

            var builder = new StringBuilder();
            while (true)
            {
                var key = reader.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (builder.Length == 0)
                    {
                        continue;
                    }
                    builder.Remove(builder.Length - 1, 1);
                    writer.Write("\b \b");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else
                {
                    builder.Append(key.KeyChar);
                    writer.Write("*");
                }
            }
            writer.WriteLine("");
            return builder.ToString();
        }
    }

}