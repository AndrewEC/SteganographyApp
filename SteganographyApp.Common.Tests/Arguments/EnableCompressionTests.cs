namespace SteganographyApp.Common.Tests
{
    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;

    [TestFixture]
    public class EnableCompressParseTests : FixtureWithMockConsoleReaderAndWriter
    {
        [Test]
        public void TestEnableCompressionWithValidValue()
        {
            string[] inputArgs = new string[] { "--enableCompression" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments input) => null));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.UseCompression);
        }
    }
}