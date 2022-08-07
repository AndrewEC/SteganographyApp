namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Static class to help identify all the field and properties of a given type that are attributed
    /// with the argument attribute.
    /// </summary>
    internal static class ArgumentFinder
    {
        /// <summary>
        /// Finds all the attributed argument fields and properties of the input type and returns an array of the attributed members,
        /// the associated parsers, and the attribute info.
        /// </summary>
        /// <param name="modelType">The type of the class from which the attribute argument fields will be pulled from.</param>
        /// <param name="additionalParsers">An optional provider that can provide a set of custom parsers for custom argument types.</param>
        /// <returns>An array of the attributed members, the associated parsers, and the attribute info.</returns>
        public static ImmutableArray<RegisteredArgument> FindAttributedArguments(Type modelType, IParserProvider? additionalParsers)
        {
            var matcher = new ParserMatcher(additionalParsers);
            var names = new List<string>();
            var registered = new List<RegisteredArgument>();

            ImmutableArray<MemberInfo> members = GetAllMembers(modelType);
            foreach (MemberInfo member in members)
            {
                ArgumentAttribute? attribute = member.GetCustomAttribute(typeof(ArgumentAttribute)) as ArgumentAttribute;
                if (attribute == null)
                {
                    continue;
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

                if (TypeHelper.DeclaredType(member) == typeof(bool) && attribute.Position > 0)
                {
                    throw new ParseException($"Argument [{attribute.Name}] is invalid. An argument cannot be a boolean and have a position.");
                }

                names.Add(attribute.Name);
                if (attribute.Parser != null)
                {
                    registered.Add(new RegisteredArgument(attribute, member, ParserMatcher.CreateParserFromMethod(modelType, attribute.Parser)));
                }
                else
                {
                    registered.Add(new RegisteredArgument(attribute, member, matcher.FindParser(attribute, member)));
                }
            }

            VerifyArgumentPositions(registered);
            return registered.ToImmutableArray();
        }

        private static ImmutableArray<MemberInfo> GetAllMembers(Type modelType)
        {
            MemberInfo[] fields = modelType.GetFields();
            MemberInfo[] properties = modelType.GetProperties();
            return new List<MemberInfo>(fields).Concat(properties).ToImmutableArray();
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

    /// <summary>
    /// Contains basic information about the argument found from the argument class including the attribute info,
    /// member info, and the parser to use to parse a value for the Member.
    /// </summary>
    internal class RegisteredArgument
    {
        /// <summary>
        /// Initializes the registered argument.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="member">The field or property of the argument class that has been attributed.</param>
        /// <param name="parser">The parser function that will parse a value from the user's input and set the member value.</param>
        public RegisteredArgument(ArgumentAttribute attribute, MemberInfo member, Func<object?, string, object> parser)
        {
            Attribute = attribute;
            Member = member;
            Parser = parser;
        }

        /// <summary>
        /// Gets a reference to the attribute info.
        /// </summary>
        public ArgumentAttribute Attribute { get; }

        /// <summary>
        /// Gets a referce to the member of the argument class that has been attributed with the argument attribute.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the parser function that will be used to parse out a value from the user's input and set the Member value.
        /// </summary>
        public Func<object?, string, object> Parser { get; }
    }
}