namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The result of matching a registered argument to the user's input.
    /// </summary>
    /// <remarks>
    /// Initializes the match result.
    /// </remarks>
    /// <param name="registeredArgument">The registered argument containing the attribute, member, and parser information.</param>
    /// <param name="input">The user provided input to be parsed and set based on the info from the registered argumetn.</param>
    internal readonly struct ArgumentMatchResult(RegisteredArgument registeredArgument, string input)
    {
        /// <summary>
        /// Gets the argument attribute.
        /// </summary>
        public readonly ArgumentAttribute Attribute { get; } = registeredArgument.Attribute;

        /// <summary>
        /// Gets the reflective info about the field or property being set.
        /// </summary>
        public readonly MemberInfo Member { get; } = registeredArgument.Member;

        /// <summary>
        /// Gets the user provided input value to be parsed.
        /// </summary>
        public readonly string Input { get; } = input;

        /// <summary>
        /// Gets the parser function used to parse the Input value and set the value of the Member.
        /// </summary>
        public readonly Func<object, string, object> Parser { get; } = registeredArgument.Parser;
    }

    /// <summary>
    /// Matches the arguments attributed on the target class with the values specified by the user's input.
    /// </summary>
    internal sealed class ArgumentMatcher : IEnumerable<ArgumentMatchResult>
    {
        private const string TrueString = "true";

        private readonly ImmutableArray<ArgumentMatchResult> matchedArguments;

        /// <summary>
        /// Initializes the argument matcher.
        /// </summary>
        /// <param name="arguments">The array of user provided arguments to be paired with the registered arguments.</param>
        /// <param name="registeredArguments">The array of attributed arguments from the class being parsed into.</param>
        public ArgumentMatcher(string[] arguments, ImmutableArray<RegisteredArgument> registeredArguments)
        {
            matchedArguments = PairAttributedArgumentsWithValues(arguments, registeredArguments);
            VerifyNoRequiredArgumentsAreMissing(registeredArguments, matchedArguments);
        }

        /// <summary>
        /// Get the enumerator for the argument match results.
        /// </summary>
        /// <returns>Enumerator for the argument match results.</returns>
        public IEnumerator<ArgumentMatchResult> GetEnumerator() => matchedArguments.AsEnumerable().GetEnumerator();

        /// <summary>
        /// Get the enumerator for the argument match results.
        /// </summary>
        /// <returns>Enumerator for the argument match results.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private void VerifyNoRequiredArgumentsAreMissing(ImmutableArray<RegisteredArgument> registeredArguments, ImmutableArray<ArgumentMatchResult> matchedArguments)
        {
            ImmutableArray<string> matchedArgumentNames = matchedArguments.Select(argument => argument.Attribute.Name).ToImmutableArray();

            ImmutableArray<string> missingRequired = registeredArguments.Where(registered => registered.Attribute.Required)
                .Select(registered => registered.Attribute.Name)
                .Where(name => !matchedArgumentNames.Contains(name))
                .ToImmutableArray();

            if (missingRequired.Length > 0)
            {
                string joined = string.Join(", ", missingRequired);
                throw new ParseException($"Missing the following required arguments: [{joined}]");
            }
        }

        private ImmutableArray<ArgumentMatchResult> PairAttributedArgumentsWithValues(string[] arguments, ImmutableArray<RegisteredArgument> registeredArguments)
        {
            var paired = new List<ArgumentMatchResult>();
            int currentPosition = 1;
            for (int i = 0; i < arguments.Length; i++)
            {
                var input = arguments[i];
                RegisteredArgument? registered = FindMatchingArgument(input, registeredArguments);
                if (registered == null)
                {
                    registered = FindMatchingArgument(currentPosition, registeredArguments);
                    if (registered == null)
                    {
                        throw new ParseException($"Received an unrecognized argument: [{input}]");
                    }
                    currentPosition++;
                }

                if (TypeHelper.DeclaredType(registered.Member) == typeof(bool))
                {
                    paired.Add(new ArgumentMatchResult(registered, TrueString));
                }
                else if (registered.Attribute.Position > 0)
                {
                    paired.Add(new ArgumentMatchResult(registered, input));
                }
                else
                {
                    if (i + 1 > arguments.Length)
                    {
                        throw new ParseException($"Received an invalid number of arguments. No value could be found corresponding to argument: [{input}]");
                    }
                    paired.Add(new ArgumentMatchResult(registered, arguments[i + 1]));
                    i++;
                }
            }
            return paired.ToImmutableArray();
        }

        private RegisteredArgument? FindMatchingArgument(string input, ImmutableArray<RegisteredArgument> pairedFields)
            => pairedFields.Where(registered => registered.Attribute.Name == input || registered.Attribute.ShortName == input).FirstOrDefault();

        private RegisteredArgument? FindMatchingArgument(int position, ImmutableArray<RegisteredArgument> pairedFields)
            => pairedFields.Where(registered => registered.Attribute.Position == position).FirstOrDefault();
    }
}