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

        private static readonly string TestFile = "Test001.png";
        private static readonly int ChunkSize = 131_072;

        [Test]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

            Assert.AreEqual(197, requiredBitsForTable);
            mockFileIOProxy.Verify(provider => provider.GetFileSizeBytes(TestFile), Times.Once());
        }

        [Test]
        public void TestCalculateRequiredBitsForContentTable()
        {
            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

            Assert.AreEqual(5, requiredNumberOfWrites);
            mockFileIOProxy.Verify(provider => provider.GetFileSizeBytes(TestFile), Times.Once());
        }

        protected override void SetupMocks()
        {
            mockFileIOProxy.Setup(provider => provider.GetFileSizeBytes(It.IsAny<string>())).Returns(ChunkSize * 5);
        }
    }
}