namespace SteganographyApp.Common.Arguments.Matching;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// Utility to help lookup a parser for a specified argument.
/// </summary>
public interface IParserFunctionLookup
{
    /// <summary>
    /// Attempts to find and return a parser function for the specified argument. This will first attempt to look for a custom
    /// parser from the parser provider provided during initialization. If no parser is found then it will look for a default
    /// parser function for the type.
    /// </summary>
    /// <param name="modelType">The type of the model from which the parser may be loaded.</param>
    /// <param name="argumentAttribute">The attribute on top of the property we are searching for a parser for.</param>
    /// <param name="memberInfo">The underlying member whose type will be used to lookup a parser function.</param>
    /// <returns>The parser function that can parsed the specified argument. If no parser is found this will throw an exception.</returns>
    Func<object, string, object> FindParser(
        Type modelType,
        ArgumentAttribute argumentAttribute,
        MemberInfo memberInfo);
}

/// <summary>
/// Utility to help lookup a parser for a specified argument.
/// </summary>
public sealed class ParserFunctionLookup : IParserFunctionLookup
{
    private static readonly ImmutableDictionary<Type, Func<object, string, object>> DefaultParsers
    = new Dictionary<Type, Func<object, string, object>>()
    {
        { typeof(byte), (instance, value) => Convert.ToByte(value) },
        { typeof(short), (instance, value) => Convert.ToInt16(value) },
        { typeof(int), (instance, value) => Convert.ToInt32(value) },
        { typeof(long), (instance, value) => Convert.ToInt64(value) },
        { typeof(float), (instance, value) => float.Parse(value) },
        { typeof(double), (instance, value) => Convert.ToDouble(value) },
        { typeof(bool), (instance, value) => Convert.ToBoolean(value) },
        { typeof(string), (instance, value) => value },
    }.ToImmutableDictionary();

    /// <inheritdoc/>
    public Func<object, string, object> FindParser(
        Type modelType,
        ArgumentAttribute argumentAttribute,
        MemberInfo memberInfo)
    {
        if (argumentAttribute.Parser != null)
        {
            return CreateParserFromMethod(modelType, argumentAttribute.Parser);
        }

        return DefaultParserFor(TypeHelper.GetDeclaredType(memberInfo))
            ?? throw new ParseException($"No parser available to parse argument: [{argumentAttribute.Name}].");
    }

    private static Func<object, string, object>? DefaultParserFor(Type fieldType)
    {
        if (fieldType.IsEnum)
        {
            return (instance, value) => Enum.Parse(fieldType, value, true);
        }
        else if (DefaultParsers.TryGetValue(fieldType, out Func<object, string, object>? parser))
        {
            return parser;
        }

        return null;
    }

    private static Func<object, string, object> CreateParserFromMethod(Type modelType, string methodName)
    {
        MethodInfo method = modelType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(info => info.Name == methodName)
            .FirstOrDefault()
            ?? throw new ParseException("Could not locate parser. No public static method with "
                + $"the name [{methodName}] could be found on type [{modelType.FullName}].");

        return (instance, value) => method.Invoke(null, [instance, value])!;
    }
}