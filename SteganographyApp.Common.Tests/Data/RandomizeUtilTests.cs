namespace SteganographyApp.Common.Tests
{
    using System.Collections.Generic;
    using System.Text;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class RandomizeUtilTests : FixtureWithTestObjects
    {
        private const string OriginalBinaryString = "1101010101000011101011111000000010101010100";
        private const string RandomSeed = "randomSeed";
        private const string BadRandomSeed = "badRandomSeed";
        private const int DummyCount = 350;
        private const int IterationMultiplier = 3;

        [Mockup(typeof(IEncryptionUtil))]
        public Mock<IEncryptionUtil> mockEncryptionUtil;

        private RandomizeUtil util;

        [SetUp]
        public void Setup()
        {
            util = new RandomizeUtil();
        }

        [Test]
        public void TestRandomizeTwiceWithSameSeedProducesSameResult()
        {
            string randomizedFirst = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed, DummyCount, IterationMultiplier);
            string randomizedSecond = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed, DummyCount, IterationMultiplier);

            Assert.AreEqual(randomizedFirst, randomizedSecond);
        }

        [Test]
        public void TestRandomizeAndReorder()
        {
            mockEncryptionUtil.Setup(util => util.GenerateKey(RandomSeed + DummyCount, DummyCount)).Returns(Encoding.UTF8.GetBytes("random_key"));

            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreEqual(OriginalBinaryString, unrandomized);
        }

        [Test]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {   
            var keyByteQueue = new Queue<byte[]>(new[]{ Encoding.UTF8.GetBytes("random_key"), Encoding.UTF8.GetBytes("encoding_key") });
            mockEncryptionUtil.Setup(util => util.GenerateKey(RandomSeed + DummyCount, DummyCount))
                .Returns(() => keyByteQueue.Dequeue());

            string randomized = util.RandomizeBinaryString(OriginalBinaryString, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(OriginalBinaryString, randomized);

            string unrandomized = util.ReorderBinaryString(randomized, BadRandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(OriginalBinaryString, unrandomized);
        }
    }
}