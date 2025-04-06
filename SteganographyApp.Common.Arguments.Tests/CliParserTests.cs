namespace SteganographyApp.Common.Arguments.Tests;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Arguments.Matching;
using SteganographyApp.Common.Arguments.Validation;

[TestFixture]
public class CliParserTests
{
    private readonly Mock<IArgumentRegistration> mockRegistration = new(MockBehavior.Strict);
    private readonly Mock<IHelp> mockHelp = new(MockBehavior.Strict);
    private readonly Mock<IArgumentValueMatcher> mockMatcher = new(MockBehavior.Strict);
    private readonly Mock<ICliValidator> mockValidator = new(MockBehavior.Strict);
    private readonly Mock<IInitializer> mockInitializer = new(MockBehavior.Strict);

    private CliParser cliParser;

    [SetUp]
    public void SetUp()
    {
        mockRegistration.Reset();
        mockHelp.Reset();
        mockMatcher.Reset();
        mockValidator.Reset();
        mockInitializer.Reset();

        cliParser = new(
            mockRegistration.Object,
            mockHelp.Object,
            mockMatcher.Object,
            mockValidator.Object,
            mockInitializer.Object);
    }

    [Test]
    public void TestParseArgs()
    {
        mockValidator.Setup(validator => validator.Validate(It.IsAny<TestClass>()))
            .Verifiable();

        string[] arguments = [];

        TestClass initializedObject = new();
        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject);

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments);

        ImmutableArray<MatchResult> matchResults = [BuildMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults);

        TestClass actual = cliParser.ParseArgs<TestClass>(arguments);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(initializedObject));
            Assert.That(actual.Message, Is.EqualTo("expected_value"));
        });

        mockValidator.Verify(validator => validator.Validate(initializedObject), Times.Once());
    }

    [Test]
    public void TestParseArgsWithErrorInParser()
    {
        mockValidator.Setup(validator => validator.Validate(It.IsAny<TestClass>()))
            .Verifiable();

        string[] arguments = [];

        TestClass initializedObject = new();
        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject);

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments);

        ImmutableArray<MatchResult> matchResults = [BuildErrorMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults);

        ParseException actual = Assert.Throws<ParseException>(
            () => cliParser.ParseArgs<TestClass>(arguments));

        Assert.That(actual.Message, Contains.Substring("Could not read in argument [argument_name]"));

        mockValidator.Verify(validator => validator.Validate(initializedObject), Times.Never());
    }

    [Test]
    public void TestTryParseArgs()
    {
        mockValidator.Setup(validator => validator.Validate(It.IsAny<TestClass>()))
            .Verifiable();

        string[] arguments = [];

        TestClass initializedObject = new();
        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject);

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments);

        ImmutableArray<MatchResult> matchResults = [BuildMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults);

        bool result = cliParser.TryParseArgs(out TestClass actual, arguments);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(actual, Is.EqualTo(initializedObject));
            Assert.That(actual.Message, Is.EqualTo("expected_value"));
        });

        mockValidator.Verify(validator => validator.Validate(initializedObject), Times.Once());
    }

    [Test]
    public void TestTryParseArgsWithErrorInParser()
    {
        mockValidator.Setup(validator => validator.Validate(It.IsAny<TestClass>()))
            .Verifiable();

        string[] arguments = [];

        TestClass initializedObject = new();
        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject);

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments);

        ImmutableArray<MatchResult> matchResults = [BuildErrorMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults);

        bool result = cliParser.TryParseArgs(out TestClass instance, arguments);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(instance, Is.EqualTo(initializedObject));
            Assert.That(cliParser.LastError, Is.Not.Null);
            Assert.That(cliParser.LastError?.Message, Contains.Substring("Could not read in argument [argument_name]"));
        });

        mockValidator.Verify(validator => validator.Validate(initializedObject), Times.Never());
    }

    private MatchResult BuildMatchResult(TestClass instance)
    {
        RegisteredArgument argument = new(
            new("argument_name"), GetMessageMember(instance), (_, value) => value);
        return new(argument, "expected_value");
    }

    private MatchResult BuildErrorMatchResult(TestClass instance)
    {
        RegisteredArgument argument = new(
            new("argument_name"), GetMessageMember(instance), (_, value) => throw new Exception());
        return new(argument, "expected_value");
    }

    private MemberInfo GetMessageMember(TestClass instance)
        => instance.GetType().GetMembers()
            .Where(member => member.Name == nameof(TestClass.Message))
            .First();

    internal class TestClass()
    {
        public string Message { get; set; } = string.Empty;
    }
}