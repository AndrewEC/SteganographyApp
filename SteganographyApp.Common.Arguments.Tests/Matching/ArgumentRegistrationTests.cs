namespace SteganographyApp.Common.Arguments.Tests;

using System;
using System.Collections.Immutable;
using System.Reflection;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments.Matching;

[TestFixture]
public class ArgumentRegistrationTests
{
    private readonly Mock<IParserFunctionLookup> lookup = new(MockBehavior.Strict);

    private ArgumentRegistration argumentRegistration;

    [SetUp]
    public void SetUp()
    {
        lookup.Reset();
        argumentRegistration = new(lookup.Object);
    }

    [Test]
    public void TestFindAttributedArguments()
    {
        Func<object, string, object> parser = (_, _) => new object();

        lookup.Setup(look => look.FindParser(
            It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()))
            .Returns(parser);

        ImmutableArray<RegisteredArgument> arguments = argumentRegistration
            .FindAttributedArguments(typeof(TestValidAttributes));

        Assert.That(arguments, Has.Length.EqualTo(2));

        RegisteredArgument actual = arguments[0];

        Assert.Multiple(() =>
        {
            Assert.That(actual.Attribute, Is.Not.Null);
            Assert.That(actual.Attribute.Name, Is.EqualTo("--hasAtt"));
            Assert.That(actual.Member, Is.Not.Null);
            Assert.That(actual.Member.Name, Is.EqualTo(nameof(TestValidAttributes.HasAttribute)));
            Assert.That(actual.Parser, Is.EqualTo(parser));
        });

        RegisteredArgument actual2 = arguments[1];

        Assert.Multiple(() =>
        {
            Assert.That(actual2.Attribute, Is.Not.Null);
            Assert.That(actual2.Attribute.Name, Is.EqualTo("--hasSecondAtt"));
            Assert.That(actual2.Member, Is.Not.Null);
            Assert.That(actual2.Member.Name, Is.EqualTo(nameof(TestValidAttributes.HasSecondAttribute)));
            Assert.That(actual2.Parser, Is.EqualTo(parser));
        });
    }

    [Test]
    public void TestFindAttributedArgumentsWithMissingArgumentName()
    {
        ParseException actual = Assert.Throws<ParseException>(
            () => argumentRegistration.FindAttributedArguments(typeof(TestMissingName)));

        Assert.That(actual.Message, Contains.Substring(nameof(TestMissingName.MissingName)));

        lookup.Verify(
            look => look.FindParser(It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()),
            Times.Never());
    }

    [Test]
    public void TestFindAttributedArgumentsWithDuplicateNames()
    {
        Func<object, string, object> parser = (_, _) => new object();

        lookup.Setup(look => look.FindParser(
            It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()))
            .Returns(parser);

        ParseException actual = Assert.Throws<ParseException>(
            () => argumentRegistration.FindAttributedArguments(typeof(TestDuplicateNames)));

        Assert.That(actual.Message, Contains.Substring("duplicateName"));

        lookup.Verify(
            look => look.FindParser(It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()),
            Times.Once());
    }

    [Test]
    public void TestFindAttributedArgumentsWithDuplicateShortNames()
    {
        Func<object, string, object> parser = (_, _) => new object();

        lookup.Setup(look => look.FindParser(
            It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()))
            .Returns(parser);

        ParseException actual = Assert.Throws<ParseException>(
            () => argumentRegistration.FindAttributedArguments(typeof(TestDuplicateShortNames)));

        Assert.That(actual.Message, Contains.Substring("duplicateShortName"));

        lookup.Verify(
            look => look.FindParser(It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()),
            Times.Once());
    }

    [Test]
    public void TestFindAttributedArgumentsWithPositionalBoolean()
    {
        ParseException actual = Assert.Throws<ParseException>(
            () => argumentRegistration.FindAttributedArguments(typeof(TestPositionalBooleanArgument)));

        Assert.That(actual.Message, Contains.Substring("invalidFlag"));

        lookup.Verify(
            look => look.FindParser(It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()),
            Times.Never());
    }

    [Test]
    public void TestFindAttributedArgumentsWithNoArgumentsFound()
    {
        ParseException actual = Assert.Throws<ParseException>(
            () => argumentRegistration.FindAttributedArguments(typeof(TestNoArguments)));

        Assert.That(actual.Message, Contains.Substring(nameof(TestNoArguments)));

        lookup.Verify(
            look => look.FindParser(It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()),
            Times.Never());
    }

    [Test]
    public void TestFindAttributedArgumentsWithInvalidPositions()
    {
        ParseException actual = Assert.Throws<ParseException>(
            () => argumentRegistration.FindAttributedArguments(typeof(TestNoArguments)));

        Assert.That(actual.Message, Contains.Substring(nameof(TestNoArguments)));

        lookup.Verify(
            look => look.FindParser(It.IsAny<Type>(), It.IsAny<ArgumentAttribute>(), It.IsAny<MemberInfo>()),
            Times.Never());
    }

    internal class TestValidAttributes
    {
        public string? HasNoAttribute { get; }

        [Argument("--hasAtt", position: 1)]
        public string? HasAttribute { get; }

        [Argument("--hasSecondAtt", position: 2)]
        public string? HasSecondAttribute { get; }
    }

    internal class TestMissingName
    {
        [Argument("")]
        public string? MissingName { get; }
    }

    internal class TestDuplicateNames
    {
        [Argument("duplicateName")]
        public string? First { get; }

        [Argument("duplicateName")]
        public string? Second { get; }
    }

    internal class TestDuplicateShortNames
    {
        [Argument("firstName", "duplicateShortName")]
        public string? First { get; }

        [Argument("secondName", "duplicateShortName")]
        public string? Second { get; }
    }

    internal class TestPositionalBooleanArgument
    {
        [Argument("invalidFlag", position: 1)]
        public bool InvalidFlag { get; }
    }

    internal class TestNoArguments { }

    internal class TestArgumentWithBadPositions
    {
        [Argument("first", position: 1)]
        public string? First { get; }

        [Argument("second", position: 3)]
        public string? Second { get; }
    }
}