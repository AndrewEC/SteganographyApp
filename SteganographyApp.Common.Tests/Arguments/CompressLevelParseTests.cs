namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SixLabors.ImageSharp.Formats.Png;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class CompressLevelParseTests
    {
        [TestCase(0, PngCompressionLevel.Level0)]
        [TestCase(5, PngCompressionLevel.Level5)]
        [TestCase(9, PngCompressionLevel.Level9)]
        public void TestCompressionLevelWithValidValue(int value, PngCompressionLevel expected)
        {
            string[] inputArgs = new string[] { "--compressionLevel", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(expected, arguments.CompressionLevel);
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