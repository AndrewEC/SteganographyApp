using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class RandomizeUtilTests
    {

        private static string OriginalBinaryString = "1101010101000011101011111000000010101010100";
        private static string RandomSeed = "randomSeed";
        private static string BadRandomSeed = "badRandomSeed";

        [TestMethod]
        public void TestRandomizeAndReorder()
        {
            var util = new RandomizeUtil();

            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, RandomSeed);
            Assert.AreEqual(OriginalBinaryString, unrandomized);
        }

        [TestMethod]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {
            var util = new RandomizeUtil();

            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, BadRandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, unrandomized);            
        }

    }

}