using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Providers;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class CalculatorTests
    {

        private static readonly string TestFile = "Test001.png";
        private static readonly int ChunkSize = 131_072;

        private Mock<IFileProvider> mockFileProvider;

        [TestInitialize]
        public void Initialize()
        {
            mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(provider => provider.GetFileSizeBytes(It.IsAny<string>())).Returns(ChunkSize * 5);
            Injector.UseProvider<IFileProvider>(mockFileProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Injector.ResetProviders();
        }

        [TestMethod]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);

            Assert.AreEqual(197, requiredBitsForTable);
            mockFileProvider.Verify(provider => provider.GetFileSizeBytes(TestFile), Times.Once());
        }

        [TestMethod]
        public void TestCalculateRequiredBitsForContentTable()
        {
            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);

            Assert.AreEqual(5, requiredNumberOfWrites);
            mockFileProvider.Verify(provider => provider.GetFileSizeBytes(TestFile), Times.Once());
        }

    }

}