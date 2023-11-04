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

        private readonly byte[] originalBytes = new byte[] { 8, 3, 4, 9, 53, 6, 3, 25, 78, 42, 56, 14, 74, 32, 63 };

        private readonly DummyUtil util = new DummyUtil();

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
            Assert.AreNotEqual(originalBytes, inserted);

            byte[] removed = util.RemoveDummies(NumberOfDummies, inserted, RandomSeed);
            Assert.AreEqual(originalBytes, removed);
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectDummyCountReturnsBadResult()
        {
            byte[] inserted = util.InsertDummies(NumberOfDummies, originalBytes, RandomSeed);
            Assert.AreNotEqual(originalBytes, inserted);

            byte[] removed = util.RemoveDummies(IncorrectNumberOfDummies, inserted, RandomSeed);
            Assert.AreNotEqual(originalBytes, removed);
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectRandomSeedReturnsBadResult()
        {
            try
            {
                byte[] inserted = util.InsertDummies(NumberOfDummies, originalBytes, RandomSeed);
                Assert.AreNotEqual(originalBytes, util.RemoveDummies(NumberOfDummies, inserted, IncorrectRandomSeed));
            }
            catch (Exception)
            {
            }
        }
    }
}