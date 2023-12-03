namespace SteganographyApp.Common.Tests
{
    using System;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class DummyUtilTests : FixtureWithTestObjects
    {
        private const int NumberOfDummies = 10;
        private const int IncorrectNumberOfDummies = 3;
        private const string RandomSeed = "random_seed";
        private const string IncorrectRandomSeed = "seed_random";

        private readonly byte[] originalBytes = [8, 3, 4, 9, 53, 6, 3, 25, 78, 42, 56, 14, 74, 32, 63];

        private readonly DummyUtil util = new();

        [SetUp]
        public void Initialize()
        {
            GlobalCounter.Instance.Reset();
        }

        [TearDown]
        public void Cleanup()
        {
            GlobalCounter.Instance.Reset();
        }

        [Test]
        public void TestInsertAndRemoveDummies()
        {
            byte[] inserted = util.InsertDummies(NumberOfDummies, originalBytes, RandomSeed);
            Assert.That(inserted, Is.Not.EqualTo(originalBytes));

            byte[] removed = util.RemoveDummies(NumberOfDummies, inserted, RandomSeed);
            Assert.That(removed, Is.EqualTo(originalBytes));
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectDummyCountReturnsBadResult()
        {
            byte[] inserted = util.InsertDummies(NumberOfDummies, originalBytes, RandomSeed);
            Assert.That(inserted, Is.Not.EqualTo(originalBytes));

            byte[] removed = util.RemoveDummies(IncorrectNumberOfDummies, inserted, RandomSeed);
            Assert.That(removed, Is.Not.EqualTo(originalBytes));
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectRandomSeedReturnsBadResult()
        {
            try
            {
                byte[] inserted = util.InsertDummies(NumberOfDummies, originalBytes, RandomSeed);
                Assert.That(util.RemoveDummies(NumberOfDummies, inserted, IncorrectRandomSeed), Is.Not.EqualTo(originalBytes));
            }
            catch (Exception)
            {
            }
        }
    }
}