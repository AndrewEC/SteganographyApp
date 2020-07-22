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

        [TestMethod]
        public void TestInsertAndRemoveDummies()
        {
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString);
            Assert.AreNotEqual(OriginalBinaryString, inserted);

            string removed = util.RemoveDummies(NumberOfDummies, inserted);
            Assert.AreEqual(OriginalBinaryString, removed);
        }

        [TestMethod]
        public void TestInsertAndRemoveWithIncorrectDummyCountReturnsBadResult()
        {
            var util = new DummyUtil();

            string inserted = util.InsertDummies(NumberOfDummies, OriginalBinaryString);
            Assert.AreNotEqual(OriginalBinaryString, inserted);

            string removed = util.RemoveDummies(IncorrectNumberOfDummies, inserted);
            Assert.AreNotEqual(OriginalBinaryString, removed);
        }

    }

}