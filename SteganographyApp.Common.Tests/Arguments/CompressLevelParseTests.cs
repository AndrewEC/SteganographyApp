using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class CompressLevelParseTests
    {

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(5)]
        [DataRow(9)]
        public void TestCompressionLevelWithValidValue(int value)
        {
            string[] inputArgs = new string[] { "--compressionLevel", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(value, arguments.CompressionLevel);
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(10)]
        public void TestCompressionLevelWithInvalidValueProducesFalseAndParseException(int value)
        {
            string[] inputArgs = new string[] { "--compressionLevel", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;

    }

}