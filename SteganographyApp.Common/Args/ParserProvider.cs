namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public interface IParserProvider
    {
        public Func<object?, string, object>? Find(ArgumentAttribute attribute, MemberInfo member);
    }

    public class ParserBuilder
    {
        private readonly Dictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>> parsers = new Dictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>>();

        public ParserBuilder ForType<T>(Func<object?, string, object> parser) => ForType(typeof(T), parser);

        public ParserBuilder ForType(Type type, Func<object?, string, object> parser)
        {
            var predicate = (ArgumentAttribute attribute, MemberInfo member) => TypeHelper.DeclaredType(member) == type;
            parsers.Add(predicate, parser);
            return this;
        }

        public ParserBuilder ForFieldNamed(string name, Func<object?, string, object> parser)
        {
            var predicate = (ArgumentAttribute attribute, MemberInfo member) => member.Name == name;
            parsers.Add(predicate, parser);
            return this;
        }

        public ParserBuilder Parser(Func<ArgumentAttribute, MemberInfo, bool> predicate, Func<object?, string, object> parser)
        {
            parsers.Add(predicate, parser);
            return this;
        }

        public IParserProvider Build() => new AdditionalParsers(parsers.ToImmutableDictionary());
    }

    public class AdditionalParsers : IParserProvider
    {
        private readonly ImmutableDictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>> parsers;

        public AdditionalParsers(ImmutableDictionary<Func<ArgumentAttribute, MemberInfo, bool>, Func<object?, string, object>> parsers)
        {
            this.parsers = parsers;
        }

        public Func<object?, string, object>? Find(ArgumentAttribute attribute, MemberInfo member)
            => parsers.Where(entry => entry.Key.Invoke(attribute, member)).Select(entry => entry.Value).FirstOrDefault();

        public static ParserBuilder Builder() => new ParserBuilder();

        public static IParserProvider ForFieldName(string fieldNamed, Func<object?, string, object> parser) => Builder().ForFieldNamed(fieldNamed, parser).Build();

        public static IParserProvider ForType<T>(Func<object?, string, object> parser) => ForType(typeof(T), parser);

        public static IParserProvider ForType(Type type, Func<object?, string, object> parser) => Builder().ForType(type, parser).Build();
    }
}