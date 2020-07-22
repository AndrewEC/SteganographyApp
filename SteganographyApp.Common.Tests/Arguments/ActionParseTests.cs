using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class ActionParsingTests
    {
        [DataTestMethod]
        [DataRow("encode", ActionEnum.Encode)]
        [DataRow("decode", ActionEnum.Decode)]
        [DataRow("clean", ActionEnum.Clean)]
        [DataRow("convert", ActionEnum.Convert)]
        [DataRow("calculate-storage-space", ActionEnum.CalculateStorageSpace)]
        [DataRow("calculate-encrypted-size", ActionEnum.CalculateEncryptedSize)]
        [DataRow("ces", ActionEnum.CES)]
        [DataRow("css", ActionEnum.CSS)]
        public void TestParseActionWithValidActionValueDoesntProduceException(string actionString, ActionEnum action)
        {
            string[] inputArgs = new string[] { "--action", actionString };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(action, arguments.EncodeOrDecode);
        }

        [TestMethod]
        public void TestParseActionWithInvalidActionReturnsFalseAndProducesParseException()
        {
            string[] inputArgs = new string[] { "--action", "invalid-action" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;

    }

}