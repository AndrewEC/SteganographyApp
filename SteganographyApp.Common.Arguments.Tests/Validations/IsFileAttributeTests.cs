namespace SteganographyApp.Common.Arguments.Tests;

using System.Reflection;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments.Validation;

[TestFixture]
public class IsFileAttributeTests
{
    private readonly IsFileAttribute isFileAttribute = new();
    private Mock<FieldInfo> mockFieldInfo = new();

    [SetUp]
    public void SetUp()
    {
        mockFieldInfo = new();
    }

    [Test]
    public void TestIsFile()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(string));
        Assert.Throws<ValidationFailedException>(() => isFileAttribute.Validate(mockFieldInfo.Object, string.Empty));
    }

    [Test]
    public void TestIsFileWithIncomatibleType()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(int));
        Assert.Throws<IncompatibleTypeException>(() => isFileAttribute.Validate(mockFieldInfo.Object, 1));
    }
}