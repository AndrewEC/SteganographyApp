using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class CalculatorTests : FixtureWithTestObjects
    {

        private static readonly string TestFile = "Test001.png";
        private static readonly int ChunkSize = 131_072;

        [Mockup(typeof(IFileProvider))]
        public Mock<IFileProvider> mockFileProvider;

        protected override void SetupMocks()
        {
            mockFileProvider.Setup(provider => provider.GetFileSizeBytes(It.IsAny<string>())).Returns(ChunkSize * 5);
        }

        [Test]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

            Assert.AreEqual(197, requiredBitsForTable);
            mockFileProvider.Verify(provider => provider.GetFileSizeBytes(TestFile), Times.Once());
        }

        [Test]
        public void TestCalculateRequiredBitsForContentTable()
        {
            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

            Assert.AreEqual(5, requiredNumberOfWrites);
            mockFileProvider.Verify(provider => provider.GetFileSizeBytes(TestFile), Times.Once());
        }

    }

}