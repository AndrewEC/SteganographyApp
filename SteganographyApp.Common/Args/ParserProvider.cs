namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A top level interface allowing one to augment the cli parser to use custom parsers for custom field types.
    /// </summary>
    public interface IParserProvider
    {
        /// <summary>
        /// Lookup and return function for parsing a field of the type specified by the member parameter.
        /// </summary>
        /// <param name="attribute">The meta attribute about the argument to parse.</param>
        /// <param name="member">The field or property being parsed.</param>
        /// <returns>The matched parser function or null if not matching function could be found.</returns>
        public Func<object?, string, object>? Find(ArgumentAttribute attribute, MemberInfo member);
    }

    /// <summary>
    /// A builder-like class to assist in the process of creating an IParserProvider instance.
    /// </summary>
    public class ParserBuilder
    {
        private readonly Dictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>> parsers = new Dictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>>();

        /// <summary>
        /// Register a new parser for the specified type of T.
        /// </summary>
        /// <param name="parser">The parser function to be executed to parse the member of type T.</param>
        /// <typeparam name="T">The type of the argument to be parsed by the input parser function.</typeparam>
        /// <returns>The current parser builder instance.</returns>
        public ParserBuilder ForType<T>(Func<object?, string, object> parser) => ForType(typeof(T), parser);

        /// <summary>
        /// Register a new parser for the specified type of T.
        /// </summary>
        /// <param name="type">The type of the argument to be parsed by the input parser function.</param>
        /// <param name="parser">The parser function to be executed to parse the member argument of type T.</param>
        /// <returns>The current parser builder instance.</returns>
        public ParserBuilder ForType(Type type, Func<object?, string, object> parser)
        {
            var predicate = (ArgumentAttribute attribute, MemberInfo member) => TypeHelper.DeclaredType(member) == type;
            parsers.Add(predicate, parser);
            return this;
        }

        /// <summary>
        /// Register a new parser to parse a member with the specified name.
        /// </summary>
        /// <param name="name">The name of the member to be parsed by the input parser function.</param>
        /// <param name="parser">The functiont to use to parse the member argument.</param>
        /// <returns>The current parser builder instance.</returns>
        public ParserBuilder ForFieldNamed(string name, Func<object?, string, object> parser)
        {
            var predicate = (ArgumentAttribute attribute, MemberInfo member) => member.Name == name;
            parsers.Add(predicate, parser);
            return this;
        }

        /// <summary>
        /// Register a parser that will parse any member that satisfies the conditions set out in the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to check if the argument member should be parsed by the associated parser function.</param>
        /// <param name="parser">The parser function to parse the argument member that satisies the condition of the associated predicate.</param>
        /// <returns>The current parser build instance.</returns>
        public ParserBuilder Parser(Func<ArgumentAttribute, MemberInfo, bool> predicate, Func<object?, string, object> parser)
        {
            parsers.Add(predicate, parser);
            return this;
        }

        /// <summary>
        /// Returns the new IParserProvider instance.
        /// </summary>
        public IParserProvider Build() => new AdditionalParsers(parsers.ToImmutableDictionary());
    }

    /// <summary>
    /// Assists in the process of building out an IParserProvider instance to provide additional parsers to be consumer by the cli parser.
    /// </summary>
    public class AdditionalParsers : IParserProvider
    {
        private readonly ImmutableDictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>> parsers;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parsers">An immutable dictionary containing the predicate functions to determine which argument member should
        /// be parsed by which parser and the associated parser function to parse any argument member that satisfies the predicate.</param>
        public AdditionalParsers(ImmutableDictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>> parsers)
        {
            this.parsers = parsers;
        }

        /// <summary>
        /// Creates a new builder instance to assist in adding new custom parsers.
        /// </summary>
        /// <returns>A new ParserBuilder instance.</returns>
        public static ParserBuilder Builder() => new ParserBuilder();

        /// <summary>
        /// Creates a new IParserProvider instance that will provide one additional custom parser to parse a field with the specified name.
        /// </summary>
        /// <param name="fieldNamed">The name of the field to be parsed by the parser function.</param>
        /// <param name="parser">The function to parse out a value for the field with the specified fieldName.</param>
        /// <returns>A parser provide that can provide a single parser.</returns>
        public static IParserProvider ForFieldName(string fieldNamed, Func<object?, string, object> parser) => Builder().ForFieldNamed(fieldNamed, parser).Build();

        /// <summary>
        /// Creates a new IParserProvider instance that will provide one additional custom parser to parse a field that corresponds
        /// to the specified type.
        /// </summary>
        /// <param name="parser">The function to parse a value out for the field with the specified type.</param>
        /// <typeparam name="T">The type to be parsed by the parser function.</typeparam>
        /// <returns>A parser provider that can provide a single parser for the specified type.</returns>
        public static IParserProvider ForType<T>(Func<object?, string, object> parser) => ForType(typeof(T), parser);

        /// <summary>
        /// Creates a new IParserProvider instance that will provide one additional custom parser to parse a field that corresponds
        /// to the specified type.
        /// </summary>
        /// <param name="type">The type to be parsed by the specified parser function.</param>
        /// <param name="parser">The function to parse a value out for the field with the specified type.</param>
        /// <returns>A parser provider that can provide a single parser for the specified type.</returns>
        public static IParserProvider ForType(Type type, Func<object?, string, object> parser) => Builder().ForType(type, parser).Build();

        /// <summary>
        /// Looks up a parser function to parse the attributed argument member. This will iterate through the entries of the
        /// dictionary, execute the predicate key, and return the first value upon a successful predicate match.
        /// </summary>
        /// <param name="attribute">The argument information.</param>
        /// <param name="member">The underlying field or property whose value will be set to the value returned by the parser function.</param>
        /// <returns>The parser function corresponding to the to the predicate whose conditions are satisfied
        /// by the input attribute and member info.</returns>
        public Func<object?, string, object>? Find(ArgumentAttribute attribute, MemberInfo member)
            => parsers.Where(entry => entry.Key.Invoke(attribute, member)).Select(entry => entry.Value).FirstOrDefault();
    }
}