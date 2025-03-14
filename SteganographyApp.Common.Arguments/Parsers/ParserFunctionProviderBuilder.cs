namespace SteganographyApp.Common.Arguments.Parsers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

/// <summary>
/// A builder-like class to assist in the process of creating an IParserProvider instance.
/// </summary>
public class ParserFunctionProviderBuilder
{
    private readonly Dictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object, string, object>> parsers = [];

    /// <summary>
    /// Register a new parser for the specified type of T.
    /// </summary>
    /// <param name="parser">The parser function to be executed to parse the member of type T.</param>
    /// <typeparam name="T">The type of the argument to be parsed by the input parser function.</typeparam>
    /// <returns>The current parser builder instance.</returns>
    public ParserFunctionProviderBuilder ForType<T>(Func<object, string, object> parser) => ForType(typeof(T), parser);

    /// <summary>
    /// Register a new parser for the specified type of T.
    /// </summary>
    /// <param name="type">The type of the argument to be parsed by the input parser function.</param>
    /// <param name="parser">The parser function to be executed to parse the member argument of type T.</param>
    /// <returns>The current parser builder instance.</returns>
    public ParserFunctionProviderBuilder ForType(Type type, Func<object, string, object> parser)
    {
        parsers.Add((attribute, member) => TypeHelper.GetDeclaredType(member) == type, parser);
        return this;
    }

    /// <summary>
    /// Register a new parser to parse a member with the specified name.
    /// </summary>
    /// <param name="name">The name of the member to be parsed by the input parser function.</param>
    /// <param name="parser">The functiont to use to parse the member argument.</param>
    /// <returns>The current parser builder instance.</returns>
    public ParserFunctionProviderBuilder ForFieldNamed(string name, Func<object, string, object> parser)
    {
        parsers.Add((attribute, member) => member.Name == name, parser);
        return this;
    }

    /// <summary>
    /// Register a parser that will parse any member that satisfies the conditions set out in the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to check if the argument member should be parsed by the associated parser function.</param>
    /// <param name="parser">The parser function to parse the argument member that satisies the condition of the associated predicate.</param>
    /// <returns>The current parser build instance.</returns>
    public ParserFunctionProviderBuilder Parser(Func<ArgumentAttribute, MemberInfo, bool> predicate, Func<object, string, object> parser)
    {
        parsers.Add(predicate, parser);
        return this;
    }

    /// <summary>
    /// Returns the new IParserProvider instance.
    /// </summary>
    /// <returns>Returns a new IParserProvider instance containing the all the previously added predicates and parsers.</returns>
    public IParserFunctionProvider Build() => new AdditionalParserFunctionsProvider(parsers.ToImmutableDictionary());
}
