using System.Text;

using NUnit.Framework;

using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class RandomSeedParseTests : FixtureWithMockConsoleReaderAndWriter
    {

        [TestCase(3)]
        [TestCase(235)]
        public void TestParseRandomSeedWithValidValue(int stringLength)
        {
            string seedValue = CreateString(stringLength);
            string[] inputArgs = new string[] { "--randomSeed", seedValue };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(seedValue, arguments.RandomSeed);
        }

        [TestCase(2)]
        [TestCase(236)]
        public void TestParseRandomSeedWithBadStringLengthProducesFalseAndParseException(int stringLength)
        {
            string[] inputArgs = new string[] { "--randomSeed", CreateString(stringLength) };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());
        }

        private string CreateString(int stringLength)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < stringLength; i++)
            {
                builder.Append("0");
            }
            return builder.ToString();
        }

        private string NullReturningPostValidator(IInputArguments input) => null;

    }

}