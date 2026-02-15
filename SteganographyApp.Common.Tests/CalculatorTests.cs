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
        mockFileIOProxy.Setup(fileProxy => fileProxy.GetFileSizeBytes(TestFile))
            .Returns(ChunkSize * RequiredNumberOfWrites)
            .Verifiable();

        calculator = new(mockFileIOProxy.Object);
    }

    [TearDown]
    public void TearDown()
    {
        mockFileIOProxy.VerifyAll();

        mockFileIOProxy.Reset();
    }

    [Test]
    public void TestRequiredChunkSizeMatchesExpected()
    {
        int requiredBitsForTable = calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

        Assert.That(requiredBitsForTable, Is.EqualTo(183));
    }

    [Test]
    public void TestCalculateRequiredBitsForContentTable()
    {
        int requiredNumberOfWrites = calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

        Assert.That(requiredNumberOfWrites, Is.EqualTo(RequiredNumberOfWrites));
    }
}