using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteganographyApp.Common.Tests
{

    public class CalculatorTests
    {

        [TestMethod]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable("TestAssets/001.png", 131_072);
            Assert.AreEqual(65, requiredBitsForTable);
        }

    }

}