using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class DummyUtilTests
    {

        private static readonly string OriginalBinaryString = "1101010101000011101011111000000010101010100";
        private static readonly int NumberOfDummies = 10;
        private static readonly int IncorrectNumberOfDummies = 3;
        private static readonly string RandomSeed = "random_seed";

        [TestInitialize]
        public void Initialize()
        {
            GlobalCounter.Instance.Reset();
        }

        [TestCleanup]
        public void Cleanup()
        {
            GlobalCounter.Instance.Reset();
        }

        [TestMethod]
        public void TestInsertAndRemoveDummies()
        {
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, inserted);

            GlobalCounter.Instance.Reset();

            string removed = util.RemoveDummies(NumberOfDummies, inserted, RandomSeed);
            Assert.AreEqual(OriginalBinaryString, removed);
        }

        [TestMethod]
        public void TestInsertAndRemoveWithIncorrectDummyCountReturnsBadResult()
        {
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, inserted);

            GlobalCounter.Instance.Reset();

            string removed = util.RemoveDummies(IncorrectNumberOfDummies, inserted, RandomSeed);
            Assert.AreNotEqual(OriginalBinaryString, removed);
        }

    }

}