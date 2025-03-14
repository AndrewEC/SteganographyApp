namespace SteganographyApp.Common.Arguments.Parsers;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// Assists in the process of building out an IParserProvider instance to provide additional parsers to be consumer by the cli parser.
/// </summary>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
/// <param name="parsers">An immutable dictionary containing the predicate functions to determine which argument member should
/// be parsed by which parser and the associated parser function to parse any argument member that satisfies the predicate.</param>
public class AdditionalParserFunctionsProvider(ImmutableDictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object, string, object>> parsers) : IParserFunctionProvider
{
    private readonly ImmutableDictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object, string, object>> parsers = parsers;

    /// <summary>
    /// Creates a new builder instance to assist in adding new custom parsers.
    /// </summary>
    /// <returns>A new ParserBuilder instance.</returns>
    public static ParserFunctionProviderBuilder Builder() => new();

    /// <summary>
    /// Creates a new IParserProvider instance that will provide one additional custom parser to parse a field with the specified name.
    /// </summary>
    /// <param name="fieldNamed">The name of the field to be parsed by the parser function.</param>
    /// <param name="parser">The function to parse out a value for the field with the specified fieldName.</param>
    /// <returns>A parser provide that can provide a single parser.</returns>
    public static IParserFunctionProvider ForFieldName(string fieldNamed, Func<object, string, object> parser) => Builder().ForFieldNamed(fieldNamed, parser).Build();

    /// <summary>
    /// Creates a new IParserProvider instance that will provide one additional custom parser to parse a field that corresponds
    /// to the specified type.
    /// </summary>
    /// <param name="parser">The function to parse a value out for the field with the specified type.</param>
    /// <typeparam name="T">The type to be parsed by the parser function.</typeparam>
    /// <returns>A parser provider that can provide a single parser for the specified type.</returns>
    public static IParserFunctionProvider ForType<T>(Func<object, string, object> parser) => ForType(typeof(T), parser);

    /// <summary>
    /// Creates a new IParserProvider instance that will provide one additional custom parser to parse a field that corresponds
    /// to the specified type.
    /// </summary>
    /// <param name="type">The type to be parsed by the specified parser function.</param>
    /// <param name="parser">The function to parse a value out for the field with the specified type.</param>
    /// <returns>A parser provider that can provide a single parser for the specified type.</returns>
    public static IParserFunctionProvider ForType(Type type, Func<object, string, object> parser) => Builder().ForType(type, parser).Build();

    /// <summary>
    /// Looks up a parser function to parse the attributed argument member. This will iterate through the entries of the
    /// dictionary, execute the predicate key, and return the first value upon a successful predicate match.
    /// </summary>
    /// <param name="attribute">The argument information.</param>
    /// <param name="member">The underlying field or property whose value will be set to the value returned by the parser function.</param>
    /// <returns>The parser function corresponding to the to the predicate whose conditions are satisfied
    /// by the input attribute and member info.</returns>
    public Func<object, string, object>? Find(ArgumentAttribute attribute, MemberInfo member)
        => parsers.Where(entry => entry.Key.Invoke(attribute, member))
            .Select(entry => entry.Value)
            .FirstOrDefault();
}