namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    
    internal static class ArgumentFinder
    {
        private static ImmutableArray<MemberInfo> GetAllMembers(Type modelType)
        {
            MemberInfo[] fields = modelType.GetFields();
            MemberInfo[] properties = modelType.GetProperties();
            return new List<MemberInfo>(fields).Concat(properties).ToImmutableArray();
        }

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

    internal class RegisteredArgument
    {
        public RegisteredArgument(ArgumentAttribute attribute, MemberInfo member, Func<object?, string, object> parser)
        {
            Attribute = attribute;
            Member = member;
            Parser = parser;
        }

        public ArgumentAttribute Attribute { get; }
        public MemberInfo Member { get; }
        public Func<object?, string, object> Parser { get; }
    }
}