using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class OutputParseTests
    {

        [TestMethod]
        public void TestParseOutputFileWithValidValue()
        {
            string[] inputArgs = new string[] { "--output", "testing.txt" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments input) => null));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual("testing.txt", arguments.DecodedOutputFile);
        }

    }

}