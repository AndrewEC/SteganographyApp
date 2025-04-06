namespace SteganographyApp.Common.Arguments.Matching;

using System;
using System.Collections.Immutable;
using System.Reflection;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments.Validation;

[TestFixture]
public class ArgumentValueMatcherTests
{
    private readonly ArgumentValueMatcher matcher = new();

    [Test]
    public void TestMatch()
    {
        string[] arguments = ["--message", "messageValue"];
        ImmutableArray<RegisteredArgument> registeredArguments = BuildNamedArguments();

        ImmutableArray<MatchResult> actualResults = matcher.Match(arguments, registeredArguments);

        Assert.That(actualResults, Has.Length.EqualTo(1));

        MatchResult actual = actualResults[0];

        Assert.Multiple(() =>
        {
            Assert.That(actual.Attribute, Is.EqualTo(registeredArguments[0].Attribute));
            Assert.That(actual.Member, Is.EqualTo(registeredArguments[0].Member));
            Assert.That(actual.Parser, Is.EqualTo(registeredArguments[0].Parser));
            Assert.That(actual.Input, Is.EqualTo(arguments[1]));
        });
    }

    [Test]
    public void TestMatchWithPosition()
    {
        string[] arguments = ["messageValue"];
        ImmutableArray<RegisteredArgument> registeredArguments = BuildPositionalArguments();

        ImmutableArray<MatchResult> actualResults = matcher.Match(arguments, registeredArguments);

        Assert.That(actualResults, Has.Length.EqualTo(1));

        MatchResult actual = actualResults[0];

        Assert.Multiple(() =>
        {
            Assert.That(actual.Attribute, Is.EqualTo(registeredArguments[0].Attribute));
            Assert.That(actual.Member, Is.EqualTo(registeredArguments[0].Member));
            Assert.That(actual.Parser, Is.EqualTo(registeredArguments[0].Parser));
            Assert.That(actual.Input, Is.EqualTo(arguments[0]));
        });
    }

    [TestCase(new string[] { "--message", "messageValue", "oneTooMany" }, "Received an unrecognized argument: [oneTooMany]")]
    [TestCase(new string[] { "--message" }, "Received an invalid number of arguments.")]
    [TestCase(new string[] { }, "Missing the following required arguments: [--message]")]
    public void TestMatchProducesException(string[] arguments, string expectedMessage)
    {
        Console.WriteLine(string.Join(",", arguments));
        ImmutableArray<RegisteredArgument> registeredArguments = BuildNamedArguments();

        ParseException actual = Assert.Throws<ParseException>(
            () => matcher.Match(arguments, registeredArguments));

        Assert.That(actual.Message, Contains.Substring(expectedMessage));
    }

    private static ImmutableArray<RegisteredArgument> BuildPositionalArguments()
    {
        FieldInfo field = MockFieldInfo("MessageField", false);

        ArgumentAttribute attribute = new("--message", position: 1);

        return BuildRegisteredArguments(field, attribute);
    }

    private static ImmutableArray<RegisteredArgument> BuildNamedArguments()
    {
        FieldInfo field = MockFieldInfo("MessageField", true);

        ArgumentAttribute attribute = new("--message");

        return BuildRegisteredArguments(field, attribute);
    }

    private static ImmutableArray<RegisteredArgument> BuildRegisteredArguments(
        FieldInfo field,
        ArgumentAttribute attribute)
    {
        Func<object, string, object> parser = (_, _) => new object();
        return [new(attribute, field, parser)];
    }

    private static FieldInfo MockFieldInfo(string name, bool required)
    {
        Mock<FieldInfo> member = new(MockBehavior.Strict);
        member.Setup(mem => mem.FieldType).Returns(typeof(string));
        member.Setup(mem => mem.MemberType).Returns(MemberTypes.Field);
        member.Setup(mem => mem.Name).Returns(name);

        if (required)
        {
            member.Setup(mem => mem.GetCustomAttributes(typeof(RequiredAttribute), true))
                .Returns(() => new Attribute[] { new RequiredAttribute() });
        }
        else
        {
            member.Setup(mem => mem.GetCustomAttributes(typeof(RequiredAttribute), true))
                .Returns(() => new Attribute[] { });
        }

        return member.Object;
    }
}
