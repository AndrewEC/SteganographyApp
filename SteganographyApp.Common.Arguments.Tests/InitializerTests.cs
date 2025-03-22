namespace SteganographyApp.Common.Arguments.Tests;

using NUnit.Framework;

[TestFixture]
public class InitializerTests
{
    private readonly Initializer initializer = new();

    [Test]
    public void TestInitialize()
    {
        CanInitialize actual = initializer.Initialize<CanInitialize>();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.InstanceOf(typeof(CanInitialize)));
        });
    }

    [Test]
    public void TestInitializeWithException()
    {
        ParseException actual = Assert.Throws<ParseException>(
            () => initializer.Initialize<CannotInitialize>());

        Assert.That(actual.Message, Is.EqualTo("Could not instantiate type [SteganographyApp.Common.Arguments.Tests.InitializerTests+CannotInitialize]. Make sure type is a class and has a default constructor."));
    }

    internal sealed class CanInitialize { }

    internal sealed class CannotInitialize(string message) { }
}