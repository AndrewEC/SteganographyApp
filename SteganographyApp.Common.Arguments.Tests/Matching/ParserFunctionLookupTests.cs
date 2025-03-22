namespace SteganographyApp.Common.Arguments.Tests;

using System;
using System.Reflection;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments.Matching;

[TestFixture]
public class ParserFunctionLookupTests
{
    private readonly ParserFunctionLookup lookup = new();

    [TestCase(typeof(byte), "3", 3)]
    [TestCase(typeof(short), "18", 18)]
    [TestCase(typeof(int), "33000", 33_000)]
    [TestCase(typeof(long), "2147483650", 2147483650)]
    [TestCase(typeof(double), "214.92", 214.92)]
    [TestCase(typeof(bool), "true", true)]
    [TestCase(typeof(bool), "false", false)]
    [TestCase(typeof(string), "test_string", "test_string")]
    public void TestFindParserForPrimitive(Type primitiveType, string value, object expected)
    {
        Func<object, string, object> parser = lookup
            .FindParser(typeof(ParserFunctionLookupTests), new(string.Empty), BuildMockField(primitiveType));

        object actual = parser.Invoke(new(), value);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("First", TestEnum.First)]
    [TestCase("Second", TestEnum.Second)]
    public void TestFindParserForEnum(string input, TestEnum expected)
    {
        Func<object, string, object> parser = lookup
            .FindParser(typeof(ParserFunctionLookupTests), new(string.Empty), BuildMockField(typeof(TestEnum)));

        object actual = parser.Invoke(new(), input);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestFindParserWithNoParserFoundThrowsException()
    {
        string argumentName = "argument_name";

        ParseException actual = Assert.Throws<ParseException>(
            () => lookup.FindParser(typeof(ParserFunctionLookup), new(argumentName), BuildMockField(typeof(object))));

        Assert.That(actual.Message, Contains.Substring(argumentName));
    }

    [Test]
    public void TestFindParserWithStaticParserMethod()
    {
        ArgumentAttribute attribute = new(string.Empty, parser: nameof(TestStaticParser.ParserFunction));
        Func<object, string, object> parser = lookup
            .FindParser(typeof(TestStaticParser), attribute, BuildMockField(typeof(TestStaticParser)));

        string input = "test_input";
        object result = parser.Invoke(new(), input);

        Assert.That(result, Is.EqualTo(string.Format(TestStaticParser.ExpectedMessage, input)));
    }

    [Test]
    public void TestFindParserWithStaticParserMethodNotFoundThrowsException()
    {
        ArgumentAttribute attribute = new(string.Empty, parser: "missing_method");

        ParseException actual = Assert.Throws<ParseException>(
            () => lookup.FindParser(typeof(TestStaticParser), attribute, BuildMockField(typeof(TestStaticParser))));

        Assert.That(actual.Message, Contains.Substring("missing_method"));
    }

    private static MemberInfo BuildMockField(Type type)
    {
        Mock<FieldInfo> mockField = new(MockBehavior.Strict);
        mockField.Setup(field => field.FieldType).Returns(type);
        return mockField.Object;
    }

    public enum TestEnum
    {
        First,
        Second,
    }

    public sealed class TestStaticParser
    {
        public static readonly string ExpectedMessage = "Result [{0}]";

        public static object ParserFunction(object target, string value)
            => string.Format(ExpectedMessage, value);
    }
}