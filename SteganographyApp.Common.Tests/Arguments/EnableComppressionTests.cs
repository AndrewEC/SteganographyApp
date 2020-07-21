using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class EnableCompressParseTests
    {

        [TestMethod]
        public void TestEnableCompressionWithValidValue()
        {
            string[] inputArgs = new string[] { "--enableCompression" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments input) => null));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.UseCompression);
        }

    }

}