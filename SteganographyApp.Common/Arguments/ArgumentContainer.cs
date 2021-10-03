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

        /// <summary>
        /// Constructor.
        /// </summary>
        public ArgumentContainer()
        {
            SensitiveArgumentParser = new SensitiveArgumentParser();
#pragma warning disable SA1009
            arguments = ImmutableList.Create(
                new Argument("--action", "-a", Parsers.ParseEncodeOrDecodeAction),
                new Argument("--input", "-in", Parsers.ParseFileToEncode),
                new Argument("--enableCompression", "-c", Parsers.ParseUseCompression, true),
                new Argument("--printStack", "-stack", Parsers.ParsePrintStack, true),
                new Argument("--images", "-im", ImagePathParser.ParseImages),
                new Argument("--password", "-p", SensitiveArgumentParser.ParsePassword),
                new Argument("--output", "-o", (arguments, value) => { arguments.DecodedOutputFile = value; }),
                new Argument("--chunkSize", "-cs", Parsers.ParseChunkSize),
                new Argument("--randomSeed", "-rs", SensitiveArgumentParser.ParseRandomSeed),
                new Argument("--enableDummies", "-d", Parsers.ParseInsertDummies, true),
                new Argument("--deleteOriginals", "-do", Parsers.ParseDeleteOriginals, true),
                new Argument("--compressionLevel", "-cl", Parsers.ParseCompressionLevel),
                new Argument("--logLevel", "-ll", Parsers.ParseLogLevel)
            );
#pragma warning restore SA1009
        }

        /// <summary>
        /// Gets a parser specifically made to help parser out fields that may be considered sensitive.
        /// </summary>
        public SensitiveArgumentParser SensitiveArgumentParser { get; private set; }

        /// <summary>
        /// Matches input list of user provided/command line arguments against the list of registered arguments to
        /// determine what arguments need to be parsed and what value should be used for tha parsing.
        /// </summary>
        /// <param name="userArguments">The array of command line/user provided arguments to be matched into a set of
        /// argument and value pairs.</param>
        /// <returns>A list of tuples in which the first element of the tuple is the registered argument whose parser
        /// will be invoked and the second element represents the raw input value to be passed to the parser.</returns>
        public ImmutableList<(Argument, string)> MatchAllArguments(string[] userArguments)
        {
            var identifiedArguments = new List<(Argument, string)>();
            for (int i = 0; i < userArguments.Length; i++)
            {
                if (!TryGetArgument(userArguments[i], out Argument argument))
                {
                    throw new ArgumentParseException($"An unrecognized argument was provided: {userArguments[i]}");
                }

                string inputValue = GetRawArgumentValue(argument, userArguments, i);
                identifiedArguments.Add((argument, inputValue));

                if (!argument.IsFlag)
                {
                    i++;
                }
            }
            return identifiedArguments.ToImmutableList();
        }

        private bool TryGetArgument(string key, out Argument targetArgument)
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
            else
            {
                if (i + 1 >= userArguments.Length)
                {
                    throw new ArgumentParseException($"Missing required value for ending argument: {userArguments[i]}");
                }
                return userArguments[i + 1];
            }
        }
    }
}