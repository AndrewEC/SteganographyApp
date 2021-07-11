namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class ChecksTests
    {
        [Test]
        public void TestChecksIsNullOrEmptyReturnsTrueOnEmptyAndNullStrings()
        {
            Assert.IsTrue(Checks.IsNullOrEmpty(string.Empty));
            Assert.IsTrue(Checks.IsNullOrEmpty(null));
        }

        [Test]
        public void TestCheckIsNullOrEmptyReturnsTrueOnNonNullString()
        {
            Assert.IsFalse(Checks.IsNullOrEmpty("testing"));
        }

        [Test]
        public void TestChecksIsNullOrEmptyReturnsTrueOnEmptyArray()
        {
            Assert.IsTrue(Checks.IsNullOrEmpty(new string[0]));
        }

        [Test]
        public void TestCheckIsNullOrEmptyReturnsFalseOnNonEmptyArray()
        {
            Assert.IsFalse(Checks.IsNullOrEmpty(new string[] { "test" }));
        }

        [Test]
        public void TestCheckIsOneOfReturnsTrueWhenArrayContainsAction()
        {
            Assert.IsTrue(Checks.IsOneOf(ActionEnum.Encode, ActionEnum.Clean, ActionEnum.Decode, ActionEnum.Encode));
        }

        [Test]
        public void TestCheckIsOneOfReturnsFalseWhenArrayContainsAction()
        {
            Assert.IsFalse(Checks.IsOneOf(ActionEnum.Encode, ActionEnum.Clean, ActionEnum.Decode));
        }

        [Test]
        public void TestWasHelpRequestedWhenShorthandHelpFlagIsInArgsReturnsTrue()
        {
            string[] args = new string[] { "-h" };
            Assert.IsTrue(Checks.WasHelpRequested(args));
        }

        [Test]
        public void TestWasHelpRequestWhenHelpFlagIsInArgsReturnsTrue()
        {
            string[] args = new string[] { "--help" };
            Assert.IsTrue(Checks.WasHelpRequested(args));
        }

        [Test]
        public void TestWasHelPRequestedWhenNoHelpFlagInArgsReturnsFalse()
        {
            string[] args = new string[0];
            Assert.IsFalse(Checks.WasHelpRequested(args));
        }
    }
}