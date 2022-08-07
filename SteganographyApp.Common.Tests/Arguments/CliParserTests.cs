namespace SteganographyApp.Common.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class CliParserTests
    {
        [Test]
        public void TestDefaultParsers()
        {
            var arguments = new string[]
            {
                "--byteNumber", "2",
                "--shortNumber", "4",
                "--intNumber", "6",
                "--longNumber", "8",
                "--floatingNumber", "10.2",
                "--doubleNumber", "12.4",
                "--booleanFlag",
                "--stringValue", "testing",
                "--place", "First",
            };
            var parsed = CliParser.ParseArgs<TestDefaultParsersArguments>(arguments);

            Assert.AreEqual(2, parsed.ByteNumber);
            Assert.AreEqual(4, parsed.ShortNumber);
            Assert.AreEqual(6, parsed.IntNumber);
            Assert.AreEqual(8, parsed.LongNumber);
            Assert.AreEqual(10.2f, parsed.FloatingNumber);
            Assert.AreEqual(12.4, parsed.DoubleNumber);
            Assert.IsTrue(parsed.Flag);
            Assert.AreEqual("testing", parsed.StringValue);
            Assert.AreEqual(Place.First, parsed.Place);
        }

        [Test]
        public void TestRequiredMissing()
        {
            var arguments = new string[] { };
            var parser = new CliParser();
            parser.TryParseArgs(out TestDefaultParsersArguments args, arguments);
            Assert.NotNull(parser.LastError);
        }

        [Test]
        public void TestDuplicateArgumentNamesThrowsException()
        {
            Assert.Throws(typeof(ParseException), () => CliParser.ParseArgs<TestDuplicateArguments>(new string[] { }));
        }

        [Test]
        public void TestPositionalArguments()
        {
            var arguments = new string[] { "testing1", "testing2" };
            var parsed = CliParser.ParseArgs<TestPositionalArguments>(arguments);
            Assert.AreEqual("testing1", parsed.First);
            Assert.AreEqual("testing2", parsed.Second);
        }

        [Test]
        public void TestInvalidPositionThrowsException()
        {
            Assert.Throws(typeof(ParseException), () => CliParser.ParseArgs<TestInvalidPositionalArguments>(new string[] { }));
        }

        [Test]
        public void TestCustomParser()
        {
            var arguments = new string[] { "--names", "Jane,John" };
            var parsed = CliParser.ParseArgs<CustomInlineParser>(arguments);
            Assert.AreEqual(2, parsed.Names.Count);
            Assert.AreEqual("Jane", parsed.Names[0]);
            Assert.AreEqual("John", parsed.Names[1]);
        }

        [Test]
        public void TestAdditionalParsers()
        {
            var parsers = AdditionalParsers.Builder().ForType<List<string>>((instance, value) => value.Split(",").ToList()).Build();
            var arguments = new string[] { "--names", "Jane,John" };
            var parsed = CliParser.ParseArgs<CustomAdditionalParser>(arguments, parsers);
            Assert.AreEqual(2, parsed.Names.Count);
            Assert.AreEqual("Jane", parsed.Names[0]);
            Assert.AreEqual("John", parsed.Names[1]);
        }
    }

    internal enum Place
    {
        First,
        Last,
    }

#pragma warning disable CS0649
    internal sealed class CustomAdditionalParser
    {
        [Argument("--names")]
        public List<string> Names;
    }

    internal sealed class CustomInlineParser
    {
        [Argument("--names", parser: nameof(ParseNames))]
        public List<string> Names;

        public static object ParseNames(object target, string value) => value.Split(",").ToList();
    }

    internal sealed class TestInvalidPositionalArguments
    {
        [Argument("--first", position: 10)]
        public string First;
    }

    internal sealed class TestPositionalArguments
    {
        [Argument("--first", position: 1)]
        public string First;

        [Argument("--second", position: 2)]
        public string Second;
    }

    internal sealed class TestDuplicateArguments
    {
        [Argument("--value")]
        public string Value;

        [Argument("--value")]
        public string Value2;
    }

    internal sealed class TestDefaultParsersArguments
    {
        [Argument("--byteNumber", "-b", required: true)]
        public byte ByteNumber;

        [Argument("--shortNumber", "-s")]
        public short ShortNumber;

        [Argument("--intNumber", "-i")]
        public int IntNumber;

        [Argument("--longNumber", "-l")]
        public long LongNumber;

        [Argument("--floatingNumber", "-f")]
        public float FloatingNumber;

        [Argument("--doubleNumber", "-d")]
        public double DoubleNumber;

        [Argument("--booleanFlag", "-bf")]
        public bool Flag;

        [Argument("--stringValue", "-sv")]
        public string StringValue;

        [Argument("--place", "-p")]
        public Place Place;
    }
#pragma warning restore CS0649
}