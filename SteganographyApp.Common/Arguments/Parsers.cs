namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A static class providing methods to lookup a default function to parser a field or property
    /// of the specified type.
    /// </summary>
    internal static class DefaultParsers
    {
        /// <summary>
        /// Attempts to find a parser function for the specified type.
        /// </summary>
        /// <param name="fieldType">The type of the field being parsed.</param>
        /// <returns>Returns a parser function to parse the field of the specified type.</returns>
        public static Func<object, string, object>? DefaultParserFor(Type fieldType)
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

    /// <summary>
    /// Utility class to help lookup a parser for a specified argument.
    /// </summary>
    /// <remarks>
    /// Initializes the parser matcher with an option parser provider.
    /// </remarks>
    /// <param name="additionalParsers">An optional provide of to provide additional parsers for custom types.</param>
    internal sealed class ParserMatcher(IParserProvider? additionalParsers)
    {
        private readonly IParserProvider? additionalParsers = additionalParsers;

        /// <summary>
        /// Creates a parser function from a specified method name. This requires the method, specified by methodName, has been declared by the
        /// modelType and the method is static.
        /// </summary>
        /// <param name="modelType">The class type that contains the method to lookup.</param>
        /// <param name="methodName">The name of the method declared by the modelType to use as an argument parser.</param>
        /// <returns>A parser function derived from the method from the provided type.</returns>
        public static Func<object, string, object> CreateParserFromMethod(Type modelType, string methodName)
        {
            MethodInfo method = modelType.GetMethods().Where(info => info.Name == methodName && info.IsStatic).FirstOrDefault()
                ?? throw new ParseException($"Could not locate parser. No static method with the name [{methodName}] could be found on the type [{modelType.FullName}].");
            return (instance, value) => method.Invoke(null, [instance, value])!;
        }

        /// <summary>
        /// Attempts to find and return a parser function for the specified argument. This will first attempt to look for a custom
        /// parser from the parser provider provided during initialization. If no parser is found then it will look for a default
        /// parser function for the type.
        /// </summary>
        /// <param name="argumentAttribute">The attribute on top of the property we are searching for a parser for.</param>
        /// <param name="memberInfo">The underlying member whose type will be used to lookup a parser function.</param>
        /// <returns>The parser function that can parsed the specified argument. If no parser is found this will throw an exception.</returns>
        public Func<object, string, object> FindParser(ArgumentAttribute argumentAttribute, MemberInfo memberInfo)
            => additionalParsers?.Find(argumentAttribute, memberInfo) ?? FindDefaultParserForField(argumentAttribute.Name, TypeHelper.DeclaredType(memberInfo));

        private static Func<object, string, object> FindDefaultParserForField(string name, Type fieldType)
            => DefaultParsers.DefaultParserFor(fieldType)
                ?? throw new ParseException($"No parser available to parse argument: [{name}]. There is no registered parser supporting type: [{fieldType}]");
    }
}