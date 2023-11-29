namespace SteganographyApp.Common.Tests;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Injection;

[TestFixture]
public class ParserFunctionTests : FixtureWithTestObjects
{
    [Mockup(typeof(IImageProxy))]
    public Mock<IImageProxy> mockImageProxy;

    [Mockup(typeof(IFileIOProxy))]
    public Mock<IFileIOProxy> mockFileProxy;

    [Test]
    public void ParseFilePath()
    {
        string expected = "image-path";
        mockFileProxy.Setup(fileProxy => fileProxy.IsExistingFile(expected)).Returns(true);

        string actual = ParserFunctions.ParseFilePath(expected);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ParseFilePathThrowsExceptionWhenFileDoesNotExist()
    {
        string expected = "image-path";
        mockFileProxy.Setup(fileProxy => fileProxy.IsExistingFile(expected)).Returns(false);

        Assert.Throws(typeof(ArgumentValueException), () => ParserFunctions.ParseFilePath(expected));
    }
}