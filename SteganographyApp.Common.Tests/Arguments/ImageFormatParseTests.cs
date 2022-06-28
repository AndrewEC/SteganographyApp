namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class ImageFormatParseTests : FixtureWithLogger
    {
        [TestCase("png", ImageFormat.Png)]
        [TestCase("webp", ImageFormat.Webp)]
        public void TestParseImageFormatWithValidValue(string value, ImageFormat expected)
        {
            string[] inputArgs = new string[] { "--imageFormat", value };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(expected, arguments.ImageFormat);
        }

        [TestCase("jpg")]
        [TestCase("gif")]
        public void TestParseImageFormatWithInvalidValueProducesFalseAndParseException(string value)
        {
            string[] inputArgs = new string[] { "--imageFormat", value };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;
    }
}