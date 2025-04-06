namespace SteganographyApp.Common.Tests;

using System.Collections.Immutable;
using System.IO;
using NUnit.Framework;
using SteganographyApp.Common.Parsers;

[TestFixture]
public class ImagePathParserTests
{
    [Test]
    public void TestParseImages()
    {
        string globs = "TestAssets/**.png";

        ImmutableArray<string> actual = ImagePathParser.ParseImages(globs);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Length.EqualTo(2));
            Assert.That(actual[0], Contains.Substring("CoverImage.png"));
            Assert.That(actual[1], Contains.Substring("Test.png"));
        });
    }

    [Test]
    public void TestParseImagesWithDuplicatePathProducesException()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "TestAssets", "CoverImage.png");
        string globs = path + "," + path;

        ArgumentValueException actual = Assert.Throws<ArgumentValueException>(
            () => ImagePathParser.ParseImages(globs));

        Assert.That(actual.Message, Contains.Substring("Two or more paths point to the same image of"));
    }

    [Test]
    public void TestParseImagesWithNoImagesFoundProducesException()
    {
        ArgumentValueException actual = Assert.Throws<ArgumentValueException>(
            () => ImagePathParser.ParseImages(string.Empty));

        Assert.That(actual.Message, Contains.Substring("No images could be found."));
    }
}