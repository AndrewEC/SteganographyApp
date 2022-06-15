namespace SteganographyApp.Common.Tests
{
    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class InputParseTests : FixtureWithMockConsoleReaderAndWriter
    {
        [Mockup(typeof(IFileIOProxy))]
        public Mock<IFileIOProxy> mockFileIOProxy;

        [Test]
        public void TestFileToEncodeWithValidFile()
        {
            string path = "file_that_exists.zip";

            mockFileIOProxy.Setup(provider => provider.IsExistingFile(path)).Returns(true);

            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(path, arguments.FileToEncode);
        }

        [Test]
        public void TestFileToEncodeWithInvalidPathProducesFalseAndparseException()
        {
            string path = "file_that_doesnt_exist.zip";

            mockFileIOProxy.Setup(provider => provider.IsExistingFile(path)).Returns(false);

            string[] inputArgs = new string[] { "--input", path };
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