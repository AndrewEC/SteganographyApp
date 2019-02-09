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
            Assert.IsTrue(parser.TryParse(TEST_PATH));
            foreach(string line in parser.GetMessagesFor("Description", "Action"))
            {
                Assert.IsNotNull(line);
                Assert.AreNotEqual("", line);
            }
        }

    }
}
