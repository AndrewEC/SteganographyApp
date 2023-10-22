namespace SteganographyApp.Common.Tests
{
    using System.Collections.Generic;
    using System.Text;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    using static Moq.It;

    [TestFixture]
    public class RandomizeUtilTests : FixtureWithTestObjects
    {
        [Mockup(typeof(IEncryptionUtil))]
        public Mock<IEncryptionUtil> mockEncryptionUtil;

        private const string RandomSeed = "randomSeed";
        private const string BadRandomSeed = "badRandomSeed";
        private const int DummyCount = 5;
        private const int IterationMultiplier = 2;
        private readonly byte[] originalBytes = new byte[] { 1, 34, 57, 31, 4, 7, 53, 78, 21, 9, 31 };

        private RandomizeUtil util;

        [SetUp]
        public void Setup()
        {
            util = new RandomizeUtil();
        }

        [Test]
        public void TestRandomizeTwiceWithSameSeedProducesSameResult()
        {
            mockEncryptionUtil.Setup(util => util.GenerateKey("randomSeed422005", 422005)).Returns(Encoding.UTF8.GetBytes("random_key"));

            byte[] first = (byte[])originalBytes.Clone();
            byte[] randomizedFirst = util.Randomize(first, RandomSeed, DummyCount, IterationMultiplier);

            byte[] second = (byte[])originalBytes.Clone();
            byte[] randomizedSecond = util.Randomize(second, RandomSeed, DummyCount, IterationMultiplier);

            Assert.AreEqual(randomizedFirst, randomizedSecond);
        }

        [Test]
        public void TestRandomizeAndReorder()
        {
            mockEncryptionUtil.Setup(util => util.GenerateKey("randomSeed422005", 422005)).Returns(Encoding.UTF8.GetBytes("random_key"));

            byte[] copy = (byte[])originalBytes.Clone();
            byte[] randomized = util.Randomize(copy, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(originalBytes, randomized);

            byte[] unrandomized = util.Reorder(randomized, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreEqual(originalBytes, unrandomized);
        }

        [Test]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {
            var keyByteQueue = new Queue<byte[]>(new[] { Encoding.UTF8.GetBytes("random_key"), Encoding.UTF8.GetBytes("encoding_key") });
            mockEncryptionUtil.Setup(util => util.GenerateKey(IsAny<string>(), IsAny<int>()))
                .Returns(() => keyByteQueue.Dequeue());

            byte[] copy = (byte[])originalBytes.Clone();
            byte[] randomized = util.Randomize(copy, RandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(originalBytes, randomized);

            byte[] unrandomized = util.Reorder(randomized, BadRandomSeed, DummyCount, IterationMultiplier);
            Assert.AreNotEqual(originalBytes, unrandomized);
        }
    }
}