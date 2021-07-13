namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class ChecksTests
    {
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