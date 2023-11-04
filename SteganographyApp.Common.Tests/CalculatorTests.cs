namespace SteganographyApp.Common.Tests
{
    using Moq;
    using NUnit.Framework;

    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class CalculatorTests : FixtureWithTestObjects
    {
        [Mockup(typeof(IFileIOProxy))]
        public Mock<IFileIOProxy> mockFileIOProxy;

        private const string TestFile = "Test001.png";
        private const int ChunkSize = 131_072;

        [Test]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

            Assert.AreEqual(183, requiredBitsForTable);
            mockFileIOProxy.Verify(fileProxy => fileProxy.GetFileSizeBytes(TestFile), Times.Once());
        }

        [Test]
        public void TestCalculateRequiredBitsForContentTable()
        {
            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

            Assert.AreEqual(5, requiredNumberOfWrites);
            mockFileIOProxy.Verify(fileProxy => fileProxy.GetFileSizeBytes(TestFile), Times.Once());
        }

        protected override void SetupMocks()
        {
            mockFileIOProxy.Setup(fileProxy => fileProxy.GetFileSizeBytes(TestFile)).Returns(ChunkSize * 5);
        }
    }
}