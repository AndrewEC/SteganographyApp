namespace SteganographyApp.Common.Tests;

using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Injection.Proxies;

[TestFixture]
public class CalculatorTests
{
    private const string TestFile = "Test001.png";
    private const int ChunkSize = 131_072;
    private const int RequiredNumberOfWrites = 5;

    private readonly Mock<IFileIOProxy> mockFileIOProxy = new(MockBehavior.Strict);

    private Calculator calculator;

    [SetUp]
    public void SetUp()
    {
        mockFileIOProxy.Reset();

        mockFileIOProxy.Setup(fileProxy => fileProxy.GetFileSizeBytes(TestFile))
            .Returns(ChunkSize * RequiredNumberOfWrites);

        calculator = new(mockFileIOProxy.Object);
    }

    [Test]
    public void TestRequiredChunkSizeMatchesExpected()
    {
        int requiredBitsForTable = calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

        Assert.That(requiredBitsForTable, Is.EqualTo(183));
        mockFileIOProxy.Verify(fileProxy => fileProxy.GetFileSizeBytes(TestFile), Times.Once());
    }

    [Test]
    public void TestCalculateRequiredBitsForContentTable()
    {
        int requiredNumberOfWrites = calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

        Assert.That(requiredNumberOfWrites, Is.EqualTo(RequiredNumberOfWrites));
        mockFileIOProxy.Verify(fileProxy => fileProxy.GetFileSizeBytes(TestFile), Times.Once());
    }
}