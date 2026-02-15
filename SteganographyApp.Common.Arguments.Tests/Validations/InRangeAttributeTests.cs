namespace SteganographyApp.Common.Arguments.Tests;

using System.Reflection;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments.Validation;

[TestFixture]
public class InRangeAttributeTests
{
    private readonly InRangeAttribute inRangeAttribute = new(10, 20);
    private Mock<FieldInfo> mockFieldInfo = new();

    [SetUp]
    public void SetUp()
    {
        mockFieldInfo = new();
    }

    [Test]
    public void TestWholeNumberInRange()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(int));
        inRangeAttribute.Validate(mockFieldInfo.Object, 10);
        inRangeAttribute.Validate(mockFieldInfo.Object, 15);
        inRangeAttribute.Validate(mockFieldInfo.Object, 20);
    }

    [Test]
    public void TestWholeNumberOutOfRange()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(int));
        Assert.Throws<ValidationFailedException>(() => inRangeAttribute.Validate(mockFieldInfo.Object, 21));
    }

    [Test]
    public void TestDecimalNumber()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(float));
        inRangeAttribute.Validate(mockFieldInfo.Object, 10.0f);
        inRangeAttribute.Validate(mockFieldInfo.Object, 15.0f);
        inRangeAttribute.Validate(mockFieldInfo.Object, 20.0f);
    }

    [Test]
    public void TestDecimalNumberOutOfRange()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(float));
        Assert.Throws<ValidationFailedException>(() => inRangeAttribute.Validate(mockFieldInfo.Object, 30));
    }

    [Test]
    public void TestIncompatibleType()
    {
        mockFieldInfo.Setup(info => info.FieldType).Returns(typeof(string));
        Assert.Throws<IncompatibleTypeException>(() => inRangeAttribute.Validate(mockFieldInfo.Object, "testing"));
    }
}