using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class HelpParserTests
    {

        private readonly string POSITIVE_TEST_PATH = "TestAssets/positive-help.prop";
        private readonly string NEGATIVE_TEST_PATH = "TestAssets/negative-help.prop";

        [TestMethod]
        [DataTestMethod]
        [DataRow(HelpItemSet.Main)]
        [DataRow(HelpItemSet.Calculator)]
        [DataRow(HelpItemSet.Converter)]
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

        [TestMethod]
        public void TestMissingFileReturnsFalse()
        {
            var parser = new HelpParser();
            Assert.IsFalse(parser.TryParseHelpFile(out HelpInfo info, "missing"));
            Assert.IsNotNull(parser.LastError);
        }

        [TestMethod]
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
