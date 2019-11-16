using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class HelpParserTests
    {

        private readonly string TEST_PATH = "TestAssets/help.prop";

        [TestMethod]
        public void TestHelpParserHappyPath()
        {
            var parser = new HelpParser();
            Assert.IsTrue(parser.TryParseHelpFile(out HelpInfo info, TEST_PATH));
            Assert.IsNull(parser.LastError);
            foreach(string line in info.GetHelpMessagesFor(HelpItemSet.Main))
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

    }
}
