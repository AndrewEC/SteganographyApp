using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class EnableDummiesParsingTests
    {
        [TestMethod]
        public void TestParseInsertDummiesWithValidValue()
        {
            string[] inputArgs = new string[] { "--enableDummies" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments input) => null));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.InsertDummies);
        }

    }

}