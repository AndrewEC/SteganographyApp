using System;

namespace SteganographyApp.Common.Arguments
{

    /// <summary>
    /// Parser class to help house and parse sensitive/interactive arguments.
    /// </summary>
    public sealed class SensitiveArgumentParser
    {

        private ValueTuple<string, Argument> password;
        private ValueTuple<string, Argument> randomSeed;

        public bool IsSensitiveArgument(Argument argument)
        {
            return argument.Name == "--password" || argument.Name == "--randomSeed";
        }

        /// <summary>
        /// Captures either of the password or random seed arguments so it can be
        /// interactively parsed after all other parsing and validation has occurred.
        /// </summary>
        public void CaptureArgument(Argument argument, string[] userArguments, int argumentIndex)
        {
            if(argumentIndex + 1 >= userArguments.Length)
            {
                throw new ArgumentParseException(string.Format("Missing required value for ending argument: {0}", userArguments[argumentIndex]));
            }
            if(argument.Name == "--password")
            {
                password = (userArguments[argumentIndex + 1], argument);
            }
            else if (argument.Name == "--randomSeed")
            {
                randomSeed = (userArguments[argumentIndex + 1], argument);
            }
        }

        /// <summary>
        /// Attempts to parse the password and randomSeed arguments and raises any exceptions that may occur
        /// if either argument cannot be properly parsed.
        /// </summary>
        public void ParseSecureArguments(InputArguments inputArguments)
        {
            TryParseSecureItem(inputArguments, password, "--password");
            TryParseSecureItem(inputArguments, randomSeed, "--randomSeed");
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
                throw new ArgumentParseException(string.Format("Invalid value provided for argument: {0}", argumentName), e);
            }
        }

    }

}