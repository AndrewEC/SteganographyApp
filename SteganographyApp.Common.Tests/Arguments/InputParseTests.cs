using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Providers;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class InputParseTests
    {

        private Mock<IFileProvider> mockFileProvider;

        [TestInitialize]
        public void Initialize()
        {
            mockFileProvider = new Mock<IFileProvider>();
            Injector.UseProvider<IFileProvider>(mockFileProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Injector.ResetProviders();
        }

        [TestMethod]
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

        [TestMethod]
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

        private string NullReturningPostValidator(IInputArguments input)
        {
            return null;
        }

    }

}