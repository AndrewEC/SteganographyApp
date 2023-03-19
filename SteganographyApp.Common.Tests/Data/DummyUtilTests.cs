namespace SteganographyApp.Common.Tests
{
    using System;

    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class DummyUtilTests : FixtureWithLogger
    {
        private const int NumberOfDummies = 10;
        private const int IncorrectNumberOfDummies = 3;
        private const string RandomSeed = "random_seed";
        private const string IncorrectRandomSeed = "seed_random";

        private readonly byte[] OriginalBytes = new byte[]{8, 3, 4, 9, 53, 6, 3, 25, 78, 42, 56, 14, 74, 32, 63};

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
            byte[] inserted = util.InsertDummies(NumberOfDummies, OriginalBytes, RandomSeed);
            Assert.AreNotEqual(OriginalBytes, inserted);

            byte[] removed = util.RemoveDummies(NumberOfDummies, inserted, RandomSeed);
            Assert.AreEqual(OriginalBytes, removed);
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectDummyCountReturnsBadResult()
        {
            byte[] inserted = util.InsertDummies(NumberOfDummies, OriginalBytes, RandomSeed);
            Assert.AreNotEqual(OriginalBytes, inserted);

            byte[] removed = util.RemoveDummies(IncorrectNumberOfDummies, inserted, RandomSeed);
            Assert.AreNotEqual(OriginalBytes, removed);
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectRandomSeedReturnsBadResult()
        {
            try
            {
                byte[] inserted = util.InsertDummies(NumberOfDummies, OriginalBytes, RandomSeed);
                Assert.AreNotEqual(OriginalBytes, util.RemoveDummies(NumberOfDummies, inserted, IncorrectRandomSeed));
            }
            catch (Exception)
            {
            }
        }
    }
}