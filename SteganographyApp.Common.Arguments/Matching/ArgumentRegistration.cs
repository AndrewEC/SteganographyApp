namespace SteganographyApp.Common.Arguments.Matching;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// Static class to help identify all the field and properties of a given type that are attributed
/// with the argument attribute.
/// </summary>
public interface IArgumentRegistration
{
    /// <summary>
    /// Finds all the attributed argument fields and properties of the input type and returns an array of the attributed members,
    /// the associated parsers, and the attribute info.
    /// </summary>
    /// <param name="modelType">The type of the class from which the attribute argument fields will be pulled from.</param>
    /// <returns>An array of the attributed members, the associated parsers, and the attribute info.</returns>
    public ImmutableArray<RegisteredArgument> FindAttributedArguments(Type modelType);
}

/// <summary>
/// Static class to help identify all the field and properties of a given type that are attributed
/// with the argument attribute.
/// </summary>
public sealed class ArgumentRegistration(IParserFunctionLookup lookup) : IArgumentRegistration
{
    /// <inheritdoc/>
    public ImmutableArray<RegisteredArgument> FindAttributedArguments(Type modelType)
    {
        var names = new HashSet<string>();
        var registered = new List<RegisteredArgument>();

        foreach (MemberInfo member in TypeHelper.GetAllFieldsAndProperties(modelType))
        {
            if (member.GetCustomAttribute(typeof(ArgumentAttribute)) is not ArgumentAttribute attribute)
            {
                continue;
            }

            string name = attribute.Name.Trim();
            if (string.IsNullOrEmpty(name))
            {
                throw new ParseException($"Argument of field named [{member.Name}] is invalid. No name "
                    + "was provided in argument attribute.");
            }

            if (!names.Add(name))
            {
                throw new ParseException($"Two or more arguments on type [{modelType.FullName}] have the same name of: [{attribute.Name}]");
            }

            string shortName = attribute.ShortName?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(shortName) && !names.Add(shortName))
            {
                throw new ParseException($"Two or more arguments have the same short name of: [{shortName}]");
            }

            if (TypeHelper.GetDeclaredType(member) == typeof(bool) && attribute.Position > 0)
            {
                throw new ParseException($"Argument [{attribute.Name}] is invalid. "
                    + "An argument cannot be a boolean and have a position.");
            }

            registered.Add(new RegisteredArgument(attribute, member, lookup
                .FindParser(modelType, attribute, member)));
        }

        if (registered.Count == 0)
        {
            throw new ParseException($"No registered arguments found on input type: [{modelType.Name}]");
        }

        VerifyArgumentPositions(registered);
        return [.. registered];
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
                throw new ParseException($"Expected argument [{argument.Attribute.Name}] to have "
                    + $"a position of [{i}]. Instead it had a position of [{position}]");
            }
        }
    }
}