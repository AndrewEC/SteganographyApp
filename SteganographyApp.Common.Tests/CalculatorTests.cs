namespace SteganographyApp.Common.Tests;

using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Injection;

[TestFixture]
public class CalculatorTests : FixtureWithTestObjects
{
    [Mockup(typeof(IFileIOProxy))]
    public Mock<IFileIOProxy> mockFileIOProxy = new();

    private const string TestFile = "Test001.png";
    private const int ChunkSize = 131_072;

    [Test]
    public void TestRequiredChunkSizeMatchesExpected()
    {
        int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

        Assert.That(requiredBitsForTable, Is.EqualTo(183));
        mockFileIOProxy.Verify(fileProxy => fileProxy.GetFileSizeBytes(TestFile), Times.Once());
    }

    [Test]
    public void TestCalculateRequiredBitsForContentTable()
    {
        int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

        Assert.That(requiredNumberOfWrites, Is.EqualTo(5));
        mockFileIOProxy.Verify(fileProxy => fileProxy.GetFileSizeBytes(TestFile), Times.Once());
    }

    protected override void SetupMocks()
    {
        mockFileIOProxy.Setup(fileProxy => fileProxy.GetFileSizeBytes(TestFile)).Returns(ChunkSize * 5);
    }
}