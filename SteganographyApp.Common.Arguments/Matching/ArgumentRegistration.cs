namespace SteganographyApp.Common.Arguments.Matching;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using SteganographyApp.Common.Arguments.Parsers;

/// <summary>
/// Static class to help identify all the field and properties of a given type that are attributed
/// with the argument attribute.
/// </summary>
internal static class ArgumentRegistration
{
    /// <summary>
    /// Finds all the attributed argument fields and properties of the input type and returns an array of the attributed members,
    /// the associated parsers, and the attribute info.
    /// </summary>
    /// <param name="modelType">The type of the class from which the attribute argument fields will be pulled from.</param>
    /// <param name="additionalParsers">An optional provider that can provide a set of custom parsers for custom argument types.</param>
    /// <returns>An array of the attributed members, the associated parsers, and the attribute info.</returns>
    public static ImmutableArray<RegisteredArgument> FindAttributedArguments(Type modelType, IParserFunctionProvider? additionalParsers)
    {
        var lookup = new ParserFunctionLookup(additionalParsers);
        var names = new List<string>();
        var registered = new List<RegisteredArgument>();

        foreach (MemberInfo member in TypeHelper.GetAllFieldsAndProperties(modelType))
        {
            if (member.GetCustomAttribute(typeof(ArgumentAttribute)) is not ArgumentAttribute attribute)
            {
                continue;
            }

            if (string.IsNullOrEmpty(attribute.Name))
            {
                throw new ParseException($"An invalid argument was provided. No name was provided for the member: [{member.Name}]");
            }

            if (names.Contains(attribute.Name))
            {
                throw new ParseException($"Two or more arguments attempted to register using the same name of: [{attribute.Name}]");
            }

            if (attribute.ShortName != null)
            {
                if (names.Contains(attribute.ShortName))
                {
                    throw new ParseException($"Two or more arguments attempted to register using the same name of: [{attribute.ShortName}]");
                }
                names.Add(attribute.ShortName);
            }

            if (TypeHelper.GetDeclaredType(member) == typeof(bool) && attribute.Position > 0)
            {
                throw new ParseException($"Argument [{attribute.Name}] is invalid. An argument cannot be a boolean and have a position.");
            }

            names.Add(attribute.Name);
            if (attribute.Parser != null)
            {
                registered.Add(new RegisteredArgument(attribute, member, ParserFunctionLookup.CreateParserFromMethod(modelType, attribute.Parser)));
            }
            else
            {
                registered.Add(new RegisteredArgument(attribute, member, lookup.FindParser(attribute, member)));
            }
        }

        if (registered.Count == 0)
        {
            throw new ParseException($"No registered arguments found on input type: [{modelType.Name}]");
        }

        VerifyArgumentPositions(registered);
        return registered.ToImmutableArray();
    }

    private static void VerifyArgumentPositions(List<RegisteredArgument> registeredArguments)
    {
        ImmutableArray<RegisteredArgument> positionalArguments = registeredArguments
            .Where(argument => argument.Attribute.Position > -1)
            .OrderBy(argument => argument.Attribute.Position)
            .ToImmutableArray();

        if (positionalArguments.Length == 0)
        {
            return;
        }

        for (int i = 1; i <= positionalArguments.Length; i++)
        {
            var argument = positionalArguments[i - 1];
            int position = argument.Attribute.Position;
            if (position != i)
            {
                throw new ParseException($"Expected argument [{argument.Attribute.Name}] to have a position of [{i}]. Instead it had a position of [{position}]");
            }
        }
    }
}