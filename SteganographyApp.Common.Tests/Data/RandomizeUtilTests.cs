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
        public Mock<IEncryptionUtil> MockEncryptionUtil = new();

        private const string RandomSeed = "randomSeed";
        private const string BadRandomSeed = "badRandomSeed";
        private const int DummyCount = 5;
        private const int IterationMultiplier = 2;
        private readonly byte[] originalBytes = [1, 34, 57, 31, 4, 7, 53, 78, 21, 9, 31];

        private RandomizeUtil util;

        [SetUp]
        public void Setup()
        {
            util = new RandomizeUtil();
        }

        [Test]
        public void TestRandomizeTwiceWithSameSeedProducesSameResult()
        {
            MockEncryptionUtil.Setup(encryptionUtil => encryptionUtil.GenerateKey("randomSeed422005", 422005)).Returns(Encoding.UTF8.GetBytes("random_key"));

            byte[] first = (byte[])originalBytes.Clone();
            byte[] randomizedFirst = util.Randomize(first, RandomSeed, DummyCount, IterationMultiplier);

            byte[] second = (byte[])originalBytes.Clone();
            byte[] randomizedSecond = util.Randomize(second, RandomSeed, DummyCount, IterationMultiplier);

            Assert.That(randomizedSecond, Is.EqualTo(randomizedFirst));
        }

        [Test]
        public void TestRandomizeAndReorder()
        {
            MockEncryptionUtil.Setup(encryptionUtil => encryptionUtil.GenerateKey("randomSeed422005", 422005)).Returns(Encoding.UTF8.GetBytes("random_key"));

            byte[] copy = (byte[])originalBytes.Clone();
            byte[] randomized = util.Randomize(copy, RandomSeed, DummyCount, IterationMultiplier);
            Assert.That(randomized, Is.Not.EqualTo(originalBytes));

            byte[] unrandomized = util.Reorder(randomized, RandomSeed, DummyCount, IterationMultiplier);
            Assert.That(unrandomized, Is.EqualTo(originalBytes));
        }

        [Test]
        public void TestRandomizeWithIncorrectRandomSeedReturnsBadResult()
        {
            var keyByteQueue = new Queue<byte[]>(new[] { Encoding.UTF8.GetBytes("random_key"), Encoding.UTF8.GetBytes("encoding_key") });
            MockEncryptionUtil.Setup(encryptionUtil => encryptionUtil.GenerateKey(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => keyByteQueue.Dequeue());

            byte[] copy = (byte[])originalBytes.Clone();
            byte[] randomized = util.Randomize(copy, RandomSeed, DummyCount, IterationMultiplier);
            Assert.That(randomized, Is.Not.EqualTo(originalBytes));

            byte[] unrandomized = util.Reorder(randomized, BadRandomSeed, DummyCount, IterationMultiplier);
            Assert.That(unrandomized, Is.Not.EqualTo(originalBytes));
        }
    }
}