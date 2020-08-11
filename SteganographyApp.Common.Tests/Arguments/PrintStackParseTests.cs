using NUnit.Framework;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class PrintStackParseTest : FixtureWithMockConsoleReaderAndWriter
    {

        [Test]
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