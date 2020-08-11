using NUnit.Framework;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class CompressLevelParseTests
    {

        [TestCase(0)]
        [TestCase(5)]
        [TestCase(9)]
        public void TestCompressionLevelWithValidValue(int value)
        {
            string[] inputArgs = new string[] { "--compressionLevel", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(value, arguments.CompressionLevel);
        }

        [TestCase(-1)]
        [TestCase(10)]
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