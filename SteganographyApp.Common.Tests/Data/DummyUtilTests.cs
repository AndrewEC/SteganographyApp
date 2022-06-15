namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Data;

    [TestFixture]
    public class DummyUtilTests : FixtureWithLogger
    {
        private const string OriginalBinaryString = "1101010101000011101011111000000010101010100";
        private const int NumberOfDummies = 10;
        private const int IncorrectNumberOfDummies = 3;
        private const string RandomSeed = "random_seed";
        private const string IncorrectRandomSeed = "seed_random";

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
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, inserted);

            GlobalCounter.Instance.Reset();

            string removed = util.RemoveDummies(NumberOfDummies, inserted, RandomSeed);
            Assert.AreEqual(OriginalBinaryString, removed);
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectDummyCountReturnsBadResult()
        {
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, inserted);

            GlobalCounter.Instance.Reset();

            string removed = util.RemoveDummies(IncorrectNumberOfDummies, inserted, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, removed);
        }

        [Test]
        public void TestInsertAndRemoveWithIncorrectRandomSeedReturnsBadResult()
        {
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);

            string removed = util.RemoveDummies(NumberOfDummies, inserted, IncorrectRandomSeed);

            Assert.AreNotEqual(OriginalBinaryString, removed);
        }

        [Test]
        public void TestInsertTwiceProducesDifferentResults()
        {
            var util = new DummyUtil();

            string insertedOnce = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);
            string insertedTwice = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);

            Assert.AreNotEqual(insertedOnce, insertedTwice);
        }
    }
}