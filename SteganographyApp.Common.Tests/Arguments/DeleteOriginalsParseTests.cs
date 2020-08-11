using NUnit.Framework;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class DeleteOriginalParseTests
    {

        [Test]
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