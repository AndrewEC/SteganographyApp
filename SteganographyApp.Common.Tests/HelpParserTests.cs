using NUnit.Framework;
using System.Linq;

namespace SteganographyApp.Common.Tests
{
    [TestFixture]
    public class HelpParserTests : FixtureWithRealObjects
    {

        private readonly string POSITIVE_TEST_PATH = "TestAssets/positive-help.prop";
        private readonly string NEGATIVE_TEST_PATH = "TestAssets/negative-help.prop";

        [TestCase(HelpItemSet.Main)]
        [TestCase(HelpItemSet.Calculator)]
        [TestCase(HelpItemSet.Converter)]
        public void TestHelpParserHappyPath(HelpItemSet itemSet)
        {
            var parser = new HelpParser();
            Assert.IsTrue(parser.TryParseHelpFile(out HelpInfo info, POSITIVE_TEST_PATH));
            Assert.IsNull(parser.LastError);
            foreach(string line in info.GetHelpMessagesFor(itemSet))
            {
                Assert.IsNotNull(line);
                Assert.AreNotEqual("", line);
            }
        }

        [Test]
        public void TestMissingFileReturnsFalse()
        {
            var parser = new HelpParser();
            Assert.IsFalse(parser.TryParseHelpFile(out HelpInfo info, "missing"));
            Assert.IsNotNull(parser.LastError);
        }

        [Test]
        public void TestHelpParserWithMissingPropertiesProducesErrorInHelpItems()
        {
            var parser = new HelpParser();
            Assert.IsTrue(parser.TryParseHelpFile(out HelpInfo info, NEGATIVE_TEST_PATH));
            foreach(string line in info.GetHelpMessagesFor(HelpItemSet.Calculator))
            {
                Assert.IsTrue(line.Contains("No help information configured for"));
            }
        }

    }
}
