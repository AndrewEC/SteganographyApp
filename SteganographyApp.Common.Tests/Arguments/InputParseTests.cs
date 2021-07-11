namespace SteganographyApp.Common.Tests
{
    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class InputParseTests : FixtureWithMockConsoleReaderAndWriter
    {
        private Mock<IFileIOProxy> mockFileIOProxy;

        [SetUp]
        public void InputSetUp()
        {
            mockFileIOProxy = new Mock<IFileIOProxy>();
            Injector.UseInstance<IFileIOProxy>(mockFileIOProxy.Object);
        }

        [Test]
        public void TestFileToEncodeWithValidFile()
        {
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(true);

            string path = "file_that_exists.zip";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(path, arguments.FileToEncode);

            mockFileIOProxy.Verify(provider => provider.IsExistingFile(path), Times.Once());
        }

        [Test]
        public void TestFileToEncodeWithInvalidPathProducesFalseAndparseException()
        {
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(false);

            string path = "file_that_doesnt_exist.zip";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());

            mockFileIOProxy.Verify(provider => provider.IsExistingFile(path), Times.Once());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;
    }
}