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
        [Mockup(typeof(IEncryptionUtil))]
        public Mock<IEncryptionUtil> mockEncryptionUtil;

        private const string RandomSeed = "randomSeed";
        private const string BadRandomSeed = "badRandomSeed";
        private const int DummyCount = 5;
        private const int IterationMultiplier = 2;
        private readonly byte[] OriginalBytes = new byte[]{ 1, 34, 57, 31, 4, 7, 53, 78, 21, 9, 31 };

        private RandomizeUtil util;

        [SetUp]
        public void Setup()
        {
            util = new RandomizeUtil();
        }

        [Test]
        public void TestRandomizeTwiceWithSameSeedProducesSameResult()
        {
            byte[] first = (byte[])OriginalBytes.Clone();
            byte[] randomizedFirst = util.Randomize(first, RandomSeed, DummyCount, IterationMultiplier);

            byte[] second = (byte[])OriginalBytes.Clone();
            byte[] randomizedSecond = util.Randomize(OriginalBytes, RandomSeed, DummyCount, IterationMultiplier);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void TestRandomizeAndReorder()
        {
            mockEncryptionUtil.Setup(util => util.GenerateKey(RandomSeed + DummyCount, DummyCount)).Returns(Encoding.UTF8.GetBytes("random_key"));

            byte[] copy = (byte[])OriginalBytes.Clone();
            byte[] randomized = util.Randomize(copy, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(OriginalBytes, randomized);

            byte[] unrandomized = util.Reorder(randomized, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreEqual(OriginalBytes, unrandomized);
        }

        [Test]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {   
            var keyByteQueue = new Queue<byte[]>(new[]{ Encoding.UTF8.GetBytes("random_key"), Encoding.UTF8.GetBytes("encoding_key") });
            mockEncryptionUtil.Setup(util => util.GenerateKey(RandomSeed + DummyCount, DummyCount))
                .Returns(() => keyByteQueue.Dequeue());

            byte[] copy = (byte[])OriginalBytes.Clone();
            byte[] randomized = util.Randomize(copy, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(OriginalBytes, randomized);

            byte[] unrandomized = util.Reorder(randomized, BadRandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(OriginalBytes, unrandomized);
        }
    }
}