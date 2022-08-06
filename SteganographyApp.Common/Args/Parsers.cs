namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class DefaultParsers
    {
        public static Func<object?, string, object>? DefaultParserFor(Type fieldType)
        {
            if (fieldType == typeof(byte))
            {
                return (instance, value) => Convert.ToByte(value);
            }
            else if (fieldType == typeof(short))
            {
                return (instance, value) => Convert.ToInt16(value);
            }
            else if (fieldType == typeof(int))
            {
                return (instance, value) => Convert.ToInt32(value);
            }
            else if (fieldType == typeof(long))
            {
                return (instance, value) => Convert.ToInt64(value);
            }
            else if (fieldType == typeof(float))
            {
                return (instance, value) => float.Parse(value);
            }
            else if (fieldType == typeof(double))
            {
                return (instance, value) => Convert.ToDouble(value);
            }
            else if (fieldType == typeof(bool))
            {
                return (instance, value) => Convert.ToBoolean(value);
            }
            else if (fieldType == typeof(string))
            {
                return (instance, value) => value;
            }
            else if (fieldType.IsEnum)
            {
                return (instance, value) => Enum.Parse(fieldType, value, true);
            }
            return null;
        }
    }

    public sealed class ParserMatcher
    {
        private readonly IParserProvider? additionalParsers;

        public ParserMatcher(IParserProvider? additionalParsers)
        {
            this.additionalParsers = additionalParsers;
        }

        public Func<object?, string, object> FindParser(ArgumentAttribute argumentAttribute, MemberInfo memberInfo)
            => additionalParsers?.Find(argumentAttribute, memberInfo) ?? FindDefaultParserForField(argumentAttribute.Name, TypeHelper.DeclaredType(memberInfo));

        private Func<object?, string, object> FindDefaultParserForField(string name, Type fieldType)
            => DefaultParsers.DefaultParserFor(fieldType)
                ?? throw new ParseException($"No parser available to parse argument: [{name}]. There is no registered parser supporting type: [{fieldType}]");

        public static Func<object?, string, object> CreateParserFromMethod(Type modelType, string methodName)
        {
            MethodInfo? method = modelType.GetMethods().Where(info => info.Name == methodName).FirstOrDefault()
                ?? throw new ParseException($"Could not locate parser. No method with the name [{methodName}] could be found on the type [{modelType.FullName}] .");
            return (instance, value) => method.Invoke(null, new []{ instance, value });
        }
    }
}