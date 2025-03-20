namespace SteganographyApp.Common.Arguments.Matching;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Matches the arguments attributed on the target class with the values specified by the user's input.
/// </summary>
internal static class ArgumentValueMatcher
{
    private const string TrueString = "true";

    /// <summary>
    /// Pairs all the input cli arguments with the registered arguments.
    /// </summary>
    /// <param name="arguments">The array of user provided arguments to be paired with the registered arguments.</param>
    /// <param name="registeredArguments">The array of attributed arguments from the class being parsed into.</param>
    /// <returns>An array of arguments matched with their respective cli values to parse.</returns>
    public static ImmutableArray<MatchResult> Match(string[] arguments, ImmutableArray<RegisteredArgument> registeredArguments)
    {
        ImmutableArray<MatchResult> matchedArguments = PairRegisteredArgumentsWithValues(arguments, registeredArguments);
        VerifyNoRequiredArgumentsAreMissing(registeredArguments, matchedArguments);
        return matchedArguments;
    }

    private static void VerifyNoRequiredArgumentsAreMissing(ImmutableArray<RegisteredArgument> registeredArguments, ImmutableArray<MatchResult> matchedArguments)
    {
        ImmutableArray<string> matchedArgumentNames = matchedArguments.Select(argument => argument.Attribute.Name)
            .ToImmutableArray();

        ImmutableArray<string> missingRequired = registeredArguments
            .Where(registered => TypeHelper.IsArgumentRequired(registered.Attribute, registered.Member))
            .Select(registered => registered.Attribute.Name)
            .Where(name => !matchedArgumentNames.Contains(name))
            .ToImmutableArray();

        if (missingRequired.Length > 0)
        {
            string joined = string.Join(", ", missingRequired);
            throw new ParseException($"Missing the following required arguments: [{joined}]");
        }
    }

    private static RegisteredArgument? FindArgumentMatchingName(string input, ImmutableArray<RegisteredArgument> registeredArguments)
        => registeredArguments
            .Where(registered => registered.Attribute.Name == input || registered.Attribute.ShortName == input)
            .FirstOrDefault();

    private static RegisteredArgument? FindArgumentWithPosition(int position, ImmutableArray<RegisteredArgument> registeredArguments)
        => registeredArguments.Where(registered => registered.Attribute.Position == position)
            .FirstOrDefault();

    private static ImmutableArray<MatchResult> PairRegisteredArgumentsWithValues(string[] arguments, ImmutableArray<RegisteredArgument> registeredArguments)
    {
        var paired = new List<MatchResult>();
        int currentPosition = 1;
        for (int i = 0; i < arguments.Length; i++)
        {
            var input = arguments[i];
            RegisteredArgument? registered = FindArgumentMatchingName(input, registeredArguments);
            if (registered == null)
            {
                registered = FindArgumentWithPosition(currentPosition, registeredArguments);
                if (registered == null)
                {
                    throw new ParseException($"Received an unrecognized argument: [{input}]");
                }

                currentPosition++;
            }

            if (TypeHelper.GetDeclaredType(registered.Member) == typeof(bool))
            {
                paired.Add(new MatchResult(registered, TrueString));
            }
            else if (registered.Attribute.Position > 0)
            {
                paired.Add(new MatchResult(registered, input));
            }
            else
            {
                if (i + 1 > arguments.Length)
                {
                    throw new ParseException($"Received an invalid number of arguments. No value could be found corresponding to argument: [{input}]");
                }

                paired.Add(new MatchResult(registered, arguments[i + 1]));
                i++;
            }
        }

        return paired.ToImmutableArray();
    }
}