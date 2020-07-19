using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class CalculatorTests
    {

        private static readonly string TestFile = "TestAssets/001.png";
        private static readonly int ChunkSize = 131_072;

        [TestMethod]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(TestFile, ChunkSize);
            Assert.AreEqual(65, requiredBitsForTable);
        }

        [TestMethod]
        public void TestCalculateRequiredBitsForContentTable()
        {
            int requiredNumberOfWrites = Calculator.CalculateRequiredNumberOfWrites(TestFile, ChunkSize);
            Assert.AreEqual(1, requiredNumberOfWrites);
        }

    }

}