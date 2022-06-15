namespace SteganographyApp.Common.Arguments
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// An internal class that contains the immatable list of arguments and their
    /// respective parsers as well as utility methods for identitifying active arguments
    /// based on the users input.
    /// </summary>
    internal class ArgumentContainer
    {
        private ImmutableList<Argument> arguments;
        private ImmutableDictionary<Argument, string> argumentsAndValues;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userArguments">The list of user provided arguments and values that need to be parsed out.</param>
        public ArgumentContainer(string[] userArguments)
        {
#pragma warning disable SA1009
            arguments = ImmutableList.Create(
                new Argument("--action", "-a", Parsers.ParseEncodeOrDecodeAction),
                new Argument("--input", "-in", Parsers.ParseFileToEncode),
                new Argument("--enableCompression", "-c", Parsers.ParseUseCompression, true),
                new Argument("--printStack", "-stack", Parsers.ParsePrintStack, true),
                new Argument("--images", "-im", ImagePathParser.ParseImages),
                new Argument("--password", "-p", SensitiveArgumentParser.ParsePassword, false, true),
                new Argument("--output", "-o", (arguments, value) => { arguments.DecodedOutputFile = value; }),
                new Argument("--chunkSize", "-cs", Parsers.ParseChunkSize),
                new Argument("--randomSeed", "-rs", SensitiveArgumentParser.ParseRandomSeed, false, true),
                new Argument("--enableDummies", "-d", Parsers.ParseInsertDummies, true),
                new Argument("--deleteOriginals", "-do", Parsers.ParseDeleteOriginals, true),
                new Argument("--compressionLevel", "-cl", Parsers.ParseCompressionLevel),
                new Argument("--logLevel", "-ll", Parsers.ParseLogLevel)
            );
#pragma warning restore SA1009
            argumentsAndValues = MatchAllArgumentsWithValuesToParse(userArguments);
        }

        /// <summary>
        /// Retrieves a subset of all the available arguments and the associated values to be parsed. All the
        /// argument keys in this dictionary will have an IsSensitive value of false.
        /// </summary>
        /// <returns>A subset of all teh available arguments that are not sensitive.</returns>
        public ImmutableDictionary<Argument, string> GetAllNonSensitiveArguments() => argumentsAndValues.Where(pair => !pair.Key.IsSensitive).ToImmutableDictionary();

        /// <summary>
        /// Retrieves a subset of all the available arguments and the associated values to be parsed. All the
        /// argument keys in this dictionary will have an IsSensitive value of true.
        /// </summary>
        /// <returns>A subset of all the available arguments that are sensitive.</returns>
        public ImmutableDictionary<Argument, string> GetAllSensitiveArguments() => argumentsAndValues.Where(pair => pair.Key.IsSensitive).ToImmutableDictionary();

        private ImmutableDictionary<Argument, string> MatchAllArgumentsWithValuesToParse(string[] userArguments)
        {
            var identifiedArguments = new Dictionary<Argument, string>();
            for (int i = 0; i < userArguments.Length; i++)
            {
                if (!TryGetArgument(userArguments[i], out Argument? argument))
                {
                    throw new ArgumentParseException($"An unrecognized argument was provided: {userArguments[i]}");
                }

                string inputValue = GetRawArgumentValue(argument!, userArguments, i);
                identifiedArguments.Add(argument!, inputValue);

                if (!argument!.IsFlag)
                {
                    i++;
                }
            }
            return identifiedArguments.ToImmutableDictionary();
        }

        private bool TryGetArgument(string key, out Argument? targetArgument)
        {
            var matchingArgument = arguments.Where(argument => argument.Name == key || argument.ShortName == key).FirstOrDefault();
            targetArgument = matchingArgument;
            return matchingArgument != null;
        }

        private string GetRawArgumentValue(Argument argument, string[] userArguments, int i)
        {
            if (argument.IsFlag)
            {
                return "true";
            }
            else if (i + 1 >= userArguments.Length)
            {
                throw new ArgumentParseException($"Missing required value for ending argument: {userArguments[i]}");
            }
            return userArguments[i + 1];
        }
    }
}