namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class ChecksTests
    {
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