namespace SteganographyApp.Common.Arguments.Tests;

using NUnit.Framework;

using SteganographyApp.Common.Arguments;

[TestFixture]
public class ChecksTests
{
    [Test]
    public void TestWasHelpRequestedWhenShorthandHelpFlagIsInArgsReturnsTrue()
    {
        string[] args = ["-h"];
        Assert.That(Checks.WasHelpRequested(args), Is.True);
    }

    [Test]
    public void TestWasHelpRequestWhenHelpFlagIsInArgsReturnsTrue()
    {
        string[] args = ["--help"];
        Assert.That(Checks.WasHelpRequested(args), Is.True);
    }

    [Test]
    public void TestWasHelPRequestedWhenNoHelpFlagInArgsReturnsFalse()
    {
        string[] args = [];
        Assert.That(Checks.WasHelpRequested(args), Is.False);
    }
}