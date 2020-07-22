using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class DeleteOriginalParseTests
    {

        [TestMethod]
        public void TestParseDeleteOriginalsWithValidValue()
        {
            string[] inputArgs = new string[] { "--deleteOriginals" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments input) => null));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.DeleteAfterConversion);
        }

    }

}