namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Provides the functionality to parse the user provided arguments into a concrete class instance.
    /// </summary>
    public sealed class CliParser
    {
        private Exception? lastError;

        /// <summary>
        /// Gets the last error that occurred while trying to parse the user's command line arguments.
        /// If TryParseArgs was invoked on an instance of the CliParser, which resulted in a failure, then this will
        /// be initialized to the exception that was raised.
        /// </summary>
        public Exception LastError { get => lastError!; }

        /// <summary>
        /// Attempts to parse the user provided arguments into the specified class model. If an exception is thrown during the parsing process
        /// this will allow the exception to propogate up.
        /// </summary>
        /// <param name="arguments">The list of user provided arguments to be parsed.</param>
        /// <param name="additionalParsers">An optional parser provider to provide additional parsers for custom types.</param>
        /// <typeparam name="T">The class containing the argument attributes from which the arguments to parsed will be derived from.</typeparam>
        /// <returns>An instance of T.</returns>
        public static T ParseArgs<T>(string[] arguments, IParserProvider? additionalParsers = null)
        where T : class => ParseArgs<T>(arguments, Initializer.Initialize<T>(), additionalParsers);

        /// <summary>
        /// Attempts to parse the user provided arguments into the specified class model. If an exception occurs during the parsing process
        /// this will return false but will not directly re-throw the original exception. The actual exception causing the failure can
        /// be accessed through the LastError property.
        /// </summary>
        /// <param name="model">The model, whose type matches the type of type parameter T, to be initialized.</param>
        /// <param name="arguments">The list of user provided arguments to be parsed.</param>
        /// <param name="additionalParsers">An optional parser provider to provide additional parsers for custom types.</param>
        /// <typeparam name="T">The class containing the argument attributes from which the arguments to parsed will be derived from.</typeparam>
        /// <returns>True if this was successful in parsing out the user provided arguments, otherwise returns false.</returns>
        public bool TryParseArgs<T>(out T model, string[] arguments, IParserProvider? additionalParsers = null)
        where T : class
        {
            T instance = Initializer.Initialize<T>();
            try
            {
                model = ParseArgs(arguments, instance, additionalParsers);
                return true;
            }
            catch (Exception e)
            {
                model = instance;
                lastError = e;
                return false;
            }
        }

        private static T ParseArgs<T>(string[] arguments, T instance, IParserProvider? additionalParsers = null)
        where T : class
        {
            ImmutableArray<RegisteredArgument> registeredArguments = ArgumentFinder.FindAttributedArguments(typeof(T), additionalParsers);
            if (WasHelpRequested(arguments))
            {
                Help.PrintHelp(typeof(T), instance, registeredArguments);
                System.Environment.Exit(0);
            }
            foreach (ArgumentMatchResult match in new ArgumentMatcher(arguments, registeredArguments))
            {
                try
                {
                    var result = match.Parser.Invoke(instance, match.Input);
                    TypeHelper.SetValue(instance, match.Member, result);
                }
                catch (Exception e)
                {
                    throw new ParseException($"Could not read in argument [{match.Attribute.Name}] from value [{match.Input}] because: [{e.InnerException?.Message ?? "No root cause."}]", e);
                }
            }

            return instance;
        }

        private static bool WasHelpRequested(string[] arguments) => arguments.Contains("--help") || arguments.Contains("-h");
    }
}