using NUnit.Framework;

using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class RandomizeUtilTests : FixtureWithLogger
    {

        private static string OriginalBinaryString = "1101010101000011101011111000000010101010100";
        private static string RandomSeed = "randomSeed";
        private static string BadRandomSeed = "badRandomSeed";

        [Test]
        public void TestRandomizeTwiceWithSameSeedProducesSameResult()
        {
            var util = new RandomizeUtil();
            util.PostConstruct();

            string randomizedFirst = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            string randomizedSecond = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);

            Assert.AreEqual(randomizedFirst, randomizedSecond);
        }

        [Test]
        public void TestRandomizeAndReorder()
        {
            var util = new RandomizeUtil();
            util.PostConstruct();

            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, RandomSeed);
            Assert.AreEqual(OriginalBinaryString, unrandomized);
        }

        [Test]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {
            var util = new RandomizeUtil();
            util.PostConstruct();

            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, BadRandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, unrandomized);            
        }

    }

}