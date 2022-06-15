namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class RandomizeUtilTests : FixtureWithLogger
    {
        private const string OriginalBinaryString = "1101010101000011101011111000000010101010100";
        private const string RandomSeed = "randomSeed";
        private const string BadRandomSeed = "badRandomSeed";

        private RandomizeUtil util;

        [SetUp]
        public void Setup()
        {
            util = new RandomizeUtil();
        }

        [Test]
        public void TestRandomizeTwiceWithSameSeedProducesSameResult()
        {
            string randomizedFirst = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            string randomizedSecond = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);

            Assert.AreEqual(randomizedFirst, randomizedSecond);
        }

        [Test]
        public void TestRandomizeAndReorder()
        {
            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, RandomSeed);
            Assert.AreEqual(OriginalBinaryString, unrandomized);
        }

        [Test]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {
            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, BadRandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, unrandomized);
        }
    }
}