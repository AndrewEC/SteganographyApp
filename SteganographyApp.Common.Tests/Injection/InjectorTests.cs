namespace SteganographyApp.Common.Tests;

using System;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Injection;

[TestFixture]
public class InjectorTests : FixtureWithRealObjects
{
    [Test]
    public void TestProvideDefaultInjectableInstance()
    {
        var reader = Injector.Provide<IConsoleReader>();
        Assert.That(reader is ConsoleKeyReader, Is.True);
    }

    [Test]
    public void TestProvideMockedInjectableInstance()
    {
        var expected = new Mock<IConsoleReader>();
        Injector.UseInstance<IConsoleReader>(expected.Object);

        var actual = Injector.Provide<IConsoleReader>();

        Assert.That(actual, Is.EqualTo(expected.Object));
    }

    [Test]
    public void TestProvideDefaultInstanceWhenOnlyMocksAreAllowedThrowsException()
    {
        Injector.AllowOnlyMockObjects();

        var actual = Assert.Throws<InvalidOperationException>(() =>
            Injector.Provide<IConsoleReader>());

        Assert.That(actual!.Message, Does.Contain(typeof(IConsoleReader).Name));
    }

    [Test]
    public void TestResetInstancesProvidesDefaultInstance()
    {
        var mockReader = new Mock<IConsoleReader>();
        Injector.UseInstance<IConsoleReader>(mockReader.Object);

        Injector.ResetInstances();
        var reader = Injector.Provide<IConsoleReader>();

        Assert.That(reader, Is.Not.EqualTo(mockReader.Object));
    }
}