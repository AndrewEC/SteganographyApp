namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class ActionParsingTests
    {
        [TestCase("encode", ActionEnum.Encode)]
        [TestCase("decode", ActionEnum.Decode)]
        [TestCase("clean", ActionEnum.Clean)]
        [TestCase("calculate-storage-space", ActionEnum.CalculateStorageSpace)]
        [TestCase("calculate-encrypted-size", ActionEnum.CalculateEncryptedSize)]
        [TestCase("ces", ActionEnum.CES)]
        [TestCase("css", ActionEnum.CSS)]
        public void TestParseActionWithValidActionValueDoesntProduceException(string actionString, ActionEnum action)
        {
            string[] inputArgs = new string[] { "--action", actionString };
            var parser = new ArgumentParser();
            parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator);
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(action, arguments.EncodeOrDecode);
        }

        [Test]
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