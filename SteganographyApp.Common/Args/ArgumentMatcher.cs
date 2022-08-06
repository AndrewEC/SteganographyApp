namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    internal sealed class ArgumentMatcher : IEnumerable<ArgumentMatchResult>
    {
        private const string TrueString = "true";

        private readonly ImmutableArray<ArgumentMatchResult> matchedArguments;

        public ArgumentMatcher(string[] arguments, ImmutableArray<RegisteredArgument> registeredArguments)
        {
            matchedArguments = PairAttributedArgumentsWithValues(arguments, registeredArguments);
            VerifyNoRequiredArgumentsAreMissing(registeredArguments, matchedArguments);
        }

        private void VerifyNoRequiredArgumentsAreMissing(ImmutableArray<RegisteredArgument> registeredArguments,
            ImmutableArray<ArgumentMatchResult> matchedArguments)
        {

            ImmutableArray<string> matchedArgumentNames = matchedArguments.Select(argument => argument.Attribute.Name).ToImmutableArray();

            ImmutableArray<string> missingRequired = registeredArguments.Select(registered => registered.Attribute)
                .Where(argument => argument.Required)
                .Select(argument => argument.Name)
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

                if (TypeHelper.DeclaredType(registered!.Member) == typeof(bool))
                {
                    paired.Add(new ArgumentMatchResult((RegisteredArgument) registered, TrueString));
                }
                else if (registered?.Attribute.Position > 0)
                {
                    paired.Add(new ArgumentMatchResult((RegisteredArgument)  registered, input));
                }
                else
                {
                    if (i + 1 > arguments.Length)
                    {
                        throw new ParseException($"Received an invalid number of arguments. No value could be found corresponding to argument: [{input}]");
                    }
                    paired.Add(new ArgumentMatchResult((RegisteredArgument) registered!, arguments[i + 1]));
                    i++;
                }
            }
            return paired.ToImmutableArray();
        }

        private RegisteredArgument? FindMatchingArgument(string input, ImmutableArray<RegisteredArgument> pairedFields)
            => pairedFields.Where(registered => registered.Attribute.Name == input || registered.Attribute.ShortName == input).FirstOrDefault();
        
        private RegisteredArgument? FindMatchingArgument(int position, ImmutableArray<RegisteredArgument> pairedFields)
            => pairedFields.Where(registered => registered.Attribute.Position == position).FirstOrDefault();
        
        public IEnumerator<ArgumentMatchResult> GetEnumerator() => matchedArguments.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal readonly struct ArgumentMatchResult
    {
        public ArgumentMatchResult(RegisteredArgument registeredArgument, string input)
        {
            Attribute = registeredArgument.Attribute;
            Member = registeredArgument.Member;
            Parser = registeredArgument.Parser;
            Input = input;
        }

        public readonly ArgumentAttribute Attribute { get; }
        public readonly MemberInfo Member { get; }
        public readonly string Input { get; }
        public readonly Func<object?, string, object> Parser;
    }
}