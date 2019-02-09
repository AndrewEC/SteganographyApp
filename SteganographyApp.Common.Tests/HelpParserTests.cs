using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SteganographyApp.Common.Help;

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
            Assert.IsTrue(parser.TryParse(out HelpInfo info, TEST_PATH));
            Assert.IsNull(parser.LastError);
            foreach(string line in info.GetMessagesFor("Description", "Action"))
            {
                Assert.IsNotNull(line);
                Assert.AreNotEqual("", line);
            }
        }

        [TestMethod]
        public void TestMissingFileReturnsFalse()
        {
            var parser = new HelpParser();
            Assert.IsFalse(parser.TryParse(out HelpInfo info, "missing"));
            Assert.IsNotNull(parser.LastError);
        }

    }
}
