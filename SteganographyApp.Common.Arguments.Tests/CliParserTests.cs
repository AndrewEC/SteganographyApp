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
        cliParser = new(
            mockRegistration.Object,
            mockHelp.Object,
            mockMatcher.Object,
            mockValidator.Object,
            mockInitializer.Object);
    }

    [TearDown]
    public void TearDown()
    {
        mockRegistration.Reset();
        mockHelp.Reset();
        mockMatcher.Reset();
        mockValidator.Reset();
        mockInitializer.Reset();
    }

    [Test]
    public void TestParseArgs()
    {
        mockValidator.Setup(validator => validator.Validate(It.IsAny<TestClass>()))
            .Verifiable();

        string[] arguments = [];

        TestClass initializedObject = new();
        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject)
            .Verifiable();

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments)
            .Verifiable();

        ImmutableArray<MatchResult> matchResults = [BuildMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults)
            .Verifiable();

        TestClass actual = cliParser.ParseArgs<TestClass>(arguments);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.EqualTo(initializedObject));
            Assert.That(actual.Message, Is.EqualTo("expected_value"));
        }

        mockValidator.VerifyAll();
        mockInitializer.VerifyAll();
        mockRegistration.VerifyAll();
        mockMatcher.VerifyAll();
    }

    [Test]
    public void TestParseArgsWithErrorInParser()
    {
        TestClass initializedObject = new();

        mockValidator.Setup(validator => validator.Validate(initializedObject))
            .Verifiable();

        string[] arguments = [];

        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject)
            .Verifiable();

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments)
            .Verifiable();

        ImmutableArray<MatchResult> matchResults = [BuildErrorMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults)
            .Verifiable();

        ParseException actual = Assert.Throws<ParseException>(
            () => cliParser.ParseArgs<TestClass>(arguments));

        Assert.That(actual.Message, Contains.Substring("Could not read in argument [argument_name]"));

        mockValidator.VerifyNoOtherCalls();
        mockInitializer.VerifyAll();
        mockRegistration.VerifyAll();
        mockMatcher.VerifyAll();
    }

    [Test]
    public void TestTryParseArgs()
    {
        TestClass initializedObject = new();

        mockValidator.Setup(validator => validator.Validate(initializedObject))
            .Verifiable();

        string[] arguments = [];

        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject)
            .Verifiable();

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments)
            .Verifiable();

        ImmutableArray<MatchResult> matchResults = [BuildMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults)
            .Verifiable();

        bool result = cliParser.TryParseArgs(out TestClass? actual, arguments);

        Assert.That(actual, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(actual, Is.EqualTo(initializedObject));
            Assert.That(actual.Message, Is.EqualTo("expected_value"));
        }

        mockValidator.VerifyAll();
        mockInitializer.VerifyAll();
        mockRegistration.VerifyAll();
        mockMatcher.VerifyAll();
    }

    [Test]
    public void TestTryParseArgsWithErrorInParser()
    {
        TestClass initializedObject = new();

        mockValidator.Setup(validator => validator.Validate(initializedObject))
            .Verifiable();

        string[] arguments = [];

        mockInitializer.Setup(initializer => initializer.Initialize<TestClass>())
            .Returns(initializedObject);

        ImmutableArray<RegisteredArgument> registeredArguments = [];
        mockRegistration.Setup(registration => registration.FindAttributedArguments(typeof(TestClass)))
            .Returns(registeredArguments);

        ImmutableArray<MatchResult> matchResults = [BuildErrorMatchResult(initializedObject)];
        mockMatcher.Setup(matcher => matcher.Match(arguments, registeredArguments))
            .Returns(matchResults);

        bool result = cliParser.TryParseArgs(out TestClass? instance, arguments);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(instance, Is.Null);
            Assert.That(result, Is.False);
            Assert.That(cliParser.LastError, Is.Not.Null);
            Assert.That(cliParser.LastError?.Message, Contains.Substring("Could not read in argument [argument_name]"));
        }

        mockValidator.VerifyNoOtherCalls();
        mockInitializer.VerifyAll();
        mockRegistration.VerifyAll();
        mockMatcher.VerifyAll();
    }

    private static MatchResult BuildMatchResult(TestClass instance)
    {
        RegisteredArgument argument = new(
            new("argument_name"), GetMessageMember(instance), (_, value) => value);
        return new(argument, "expected_value");
    }

    private static MatchResult BuildErrorMatchResult(TestClass instance)
    {
        RegisteredArgument argument = new(
            new("argument_name"), GetMessageMember(instance), (_, value) => throw new Exception());
        return new(argument, "expected_value");
    }

    private static MemberInfo GetMessageMember(TestClass instance)
        => instance.GetType().GetMembers()
            .First(member => member.Name == nameof(TestClass.Message));

    internal class TestClass()
    {
        public string Message { get; set; } = string.Empty;
    }
}