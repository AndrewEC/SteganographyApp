using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class ChunkSizeParseTests
    {

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10_000)]
        [DataRow(1_000_000)]
        public void TestParseChunkSizeWithValidValue(int value)
        {
            string[] inputArgs = new string[] { "--chunkSize", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(value, arguments.ChunkByteSize);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow("test")]
        public void TestParseChunkSizeWithInvalidValueProducesFalseAndParseException(object value)
        {
            string[] inputArgs = new string[] { "--chunkSize", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;

    }

}