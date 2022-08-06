namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    public sealed class CliParser
    {
        private Exception? lastError;
        public Exception LastError { get => lastError!; }

        public bool TryParseArgs<T>(out T model, string[] arguments, IParserProvider? additionalParsers = null)
        where T : class
        {
            T instance = Initializer.Initialize<T>();
            try
            {
                model = ParseArgs(arguments, additionalParsers, instance);
                return true;
            }
            catch (Exception e)
            {
                model = instance;
                lastError = e;
                return false;
            }
        }

        public static T ParseArgs<T>(string[] arguments, IParserProvider? additionalParsers = null) where T : class => ParseArgs<T>(arguments, additionalParsers, null);

        private static T ParseArgs<T>(string[] arguments, IParserProvider? additionalParsers = null, T? instance = null)
        where T : class
        {
            if (instance == null)
            {
                instance = Initializer.Initialize<T>();
            }
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
                    throw new ParseException($"Could not read in argument [{match.Attribute.Name}] from value [{match.Input}] because: [{e.InnerException.Message}]", e);
                }
            }

            return instance;
        }

        private static bool WasHelpRequested(string[] arguments) => arguments.Contains("--help") || arguments.Contains("-h");
    }
}