using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class PrintStackParseTest
    {

        [TestMethod]
        public void TestPrintStackWithValidValue()
        {
            string[] inputArgs = new string[] { "--printStack" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments arguments) => null));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.PrintStack);
        }

    }

}