using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class ChecksTests
    {

        [TestMethod]
        public void TestChecksIsNullOrEmptyReturnsTrueOnEmptyAndNullStrings()
        {
            Assert.IsTrue(Checks.IsNullOrEmpty(""));
            Assert.IsTrue(Checks.IsNullOrEmpty(null));
        }

        [TestMethod]
        public void TestCheckIsNullOrEmptyReturnsTrueOnNonNullString()
        {
            Assert.IsFalse(Checks.IsNullOrEmpty("testing"));
        }

        [TestMethod]
        public void TestChecksIsNullOrEmptyReturnsTrueOnEmptyArray()
        {
            Assert.IsTrue(Checks.IsNullOrEmpty(new string[0]));
        }

        [TestMethod]
        public void TestCheckIsNullOrEmptyReturnsFalseOnNonEmptyArray()
        {
            Assert.IsFalse(Checks.IsNullOrEmpty(new string[] { "test" }));
        }

        [TestMethod]
        public void TestCheckIsOneOfReturnsTrueWhenArrayContainsAction()
        {
            Assert.IsTrue(Checks.IsOneOf(ActionEnum.Encode, ActionEnum.Clean, ActionEnum.Decode, ActionEnum.Encode));
        }

        [TestMethod]
        public void TestCheckIsOneOfReturnsFalseWhenArrayContainsAction()
        {
            Assert.IsFalse(Checks.IsOneOf(ActionEnum.Encode, ActionEnum.Clean, ActionEnum.Decode));
        }

        [TestMethod]
        public void TestWasHelpRequestedWhenShorthandHelpFlagIsInArgsReturnsTrue()
        {
            string[] args = new string[] { "-h" };
            Assert.IsTrue(Checks.WasHelpRequested(args));
        }

        [TestMethod]
        public void TestWasHelpRequestWhenHelpFlagIsInArgsReturnsTrue()
        {
            string[] args = new string[] { "--help" };
            Assert.IsTrue(Checks.WasHelpRequested(args));
        }

        [TestMethod]
        public void TestWasHelPRequestedWhenNoHelpFlagInArgsReturnsFalse()
        {
            string[] args = new string[0];
            Assert.IsFalse(Checks.WasHelpRequested(args));
        }

    }

}