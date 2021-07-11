namespace SteganographyApp.Common.Tests
{
    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class InputParseTests : FixtureWithMockConsoleReaderAndWriter
    {
        private Mock<IFileProvider> mockFileProvider;

        [SetUp]
        public void InputSetUp()
        {
            mockFileProvider = new Mock<IFileProvider>();
            Injector.UseInstance<IFileProvider>(mockFileProvider.Object);
        }

        [Test]
        public void TestFileToEncodeWithValidFile()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(true);

            string path = "file_that_exists.zip";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(path, arguments.FileToEncode);

            mockFileProvider.Verify(provider => provider.IsExistingFile(path), Times.Once());
        }

        [Test]
        public void TestFileToEncodeWithInvalidPathProducesFalseAndparseException()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(false);

            string path = "file_that_doesnt_exist.zip";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());

            mockFileProvider.Verify(provider => provider.IsExistingFile(path), Times.Once());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;
    }
}