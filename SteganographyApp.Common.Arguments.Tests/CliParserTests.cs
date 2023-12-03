namespace SteganographyApp.Common.Tests;

using System;
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
        Assert.Multiple(() =>
        {
            Assert.That(parsed.ByteNumber, Is.EqualTo(2));
            Assert.That(parsed.ShortNumber, Is.EqualTo(4));
            Assert.That(parsed.IntNumber, Is.EqualTo(6));
            Assert.That(parsed.LongNumber, Is.EqualTo(8));
            Assert.That(parsed.FloatingNumber, Is.EqualTo(10.2f));
            Assert.That(parsed.DoubleNumber, Is.EqualTo(12.4));
            Assert.That(parsed.Flag, Is.True);
            Assert.That(parsed.StringValue, Is.EqualTo("testing"));
            Assert.That(parsed.Place, Is.EqualTo(Place.First));
        });
    }

    [Test]
    public void TestRequiredMissing()
    {
        var arguments = Array.Empty<string>();
        var parser = new CliParser();
        parser.TryParseArgs(out TestDefaultParsersArguments args, arguments);
        Assert.That(parser.LastError, Is.Not.Null);
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
        Assert.That(parsed.First, Is.EqualTo("testing1"));
        Assert.That(parsed.Second, Is.EqualTo("testing2"));
    }

    [Test]
    public void TestInvalidPositionThrowsException()
    {
        Assert.Throws(typeof(ParseException), () => CliParser.ParseArgs<TestInvalidPositionalArguments>(Array.Empty<string>()));
    }

    [Test]
    public void TestCustomParser()
    {
        var arguments = new string[] { "--names", "Jane,John" };
        var parsed = CliParser.ParseArgs<CustomInlineParser>(arguments);
        Assert.That(parsed.Names, Has.Count.EqualTo(2));
        Assert.That(parsed.Names[0], Is.EqualTo("Jane"));
        Assert.That(parsed.Names[1], Is.EqualTo("John"));
    }

    [Test]
    public void TestAdditionalParsers()
    {
        var parsers = AdditionalParsers.ForType<List<string>>((instance, value) => value.Split(",").ToList());
        var arguments = new string[] { "--names", "Jane,John" };
        var parsed = CliParser.ParseArgs<CustomAdditionalParser>(arguments, parsers);
        Assert.That(parsed.Names, Has.Count.EqualTo(2));
        Assert.That(parsed.Names[0], Is.EqualTo("Jane"));
        Assert.That(parsed.Names[1], Is.EqualTo("John"));
    }
}

internal enum Place
{
    Unspecified,
    First,
    Last,
}

#pragma warning disable CS0649
internal sealed class CustomAdditionalParser
{
    [Argument("--names")]
    public List<string> Names = [];
}

internal sealed class CustomInlineParser
{
    [Argument("--names", parser: nameof(ParseNames))]
    public List<string> Names = [];

    public static object ParseNames(object target, string value) => value.Split(",").ToList();
}

internal sealed class TestInvalidPositionalArguments
{
    [Argument("--first", position: 10)]
    public string First = string.Empty;
}

internal sealed class TestPositionalArguments
{
    [Argument("--first", position: 1)]
    public string First = string.Empty;

    [Argument("--second", position: 2)]
    public string Second = string.Empty;
}

internal sealed class TestDuplicateArguments
{
    [Argument("--value")]
    public string Value = string.Empty;

    [Argument("--value")]
    public string Value2 = string.Empty;
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
    public bool Flag = false;

    [Argument("--stringValue", "-sv")]
    public string StringValue = string.Empty;

    [Argument("--place", "-p")]
    public Place Place = Place.Unspecified;
}
#pragma warning restore CS0649