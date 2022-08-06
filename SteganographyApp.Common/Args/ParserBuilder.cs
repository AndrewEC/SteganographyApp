namespace SteganographyApp.Common.Arguments.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CollectionParserBuilder<T>
    {
        private readonly Func<IEnumerable<T>, object> enumerableCastFunction;
        public CollectionParserBuilder(Func<IEnumerable<T>, object> enumerableCastFunction)
        {
            this.enumerableCastFunction = enumerableCastFunction;
        }

        public Func<object?, string, object> SeparatedBy(string splitter) => CollectionParsers.Compose<T>(splitter, enumerableCastFunction);
        public Func<object?, string, object> CommaSeparated() => SeparatedBy(",");
    }

    public static class ParseList
    {
        public static CollectionParserBuilder<T> Of<T>() => new CollectionParserBuilder<T>((enumerable) => enumerable.ToList());
    }

    public static class ParseArray
    {
        public static CollectionParserBuilder<T> Of<T>() => new CollectionParserBuilder<T>((enumerable) => enumerable.ToArray());
    }

    public static class CollectionParsers
    {
        public static Func<object?, string, object> Compose<T>(string splitter, Func<IEnumerable<T>, object> enumerableCastFunction)
        {
            Type listItemType = typeof(T);
            var valueParser = DefaultParsers.DefaultParserFor(listItemType)
                ?? throw new ParseException($"Could not create parser to parse collection of [{listItemType.Name}]. No parser for type [{listItemType.Name}] could be found.");
            return (instance, value) => enumerableCastFunction.Invoke(value.Split(splitter).Select(x => (T) valueParser.Invoke(null, x)));
        }
    }

}