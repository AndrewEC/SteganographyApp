using Moq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Providers;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class ParseImagesTests
    {

        private Mock<IFileProvider> mockFileProvider;

        [TestInitialize]
        public void Initialize()
        {
            mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(true);
            Injector.UseProvider<IFileProvider>(mockFileProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Injector.ResetProviders();
        }

        [TestMethod]
        public void TestParseImagesWithValidSingleValue()
        {
            string image = "TestAssets/001.png";
            string[] inputArgs = new string[] { "--images", image };
            var parser = new ArgumentParser();

            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);

            string[] images = arguments.CoverImages;
            Assert.AreEqual(1, images.Length);
            Assert.AreEqual(image, images[0]);

            mockFileProvider.Verify(provider => provider.IsExistingFile(image), Times.Once());
        }

        [TestMethod]
        public void TestParseImagesWithValidRegex()
        {
            mockFileProvider.Setup(provider => provider.GetFiles(It.IsAny<string>())).Returns(new string[] { "./TestAssets\\001.png", "./TestAssets\\002.png" });

            string expression = "[r]<^[\\w\\W]+\\.(png)$><./TestAssets>";
            string[] inputArgs = new string[] { "--images", expression };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);

            string[] images = arguments.CoverImages;
            Assert.AreEqual(2, images.Length);
            Assert.AreEqual("./TestAssets\\001.png", images[0]);
            Assert.AreEqual("./TestAssets\\002.png", images[1]);

            mockFileProvider.Verify(provider => provider.IsExistingFile(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TestParseImagesWithInvalidSingleValuesReturnsFalseAndProducesParseException()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(It.IsAny<string>())).Returns(false);

            string[] inputArgs = new string[] { "--images", "missing-image" };    
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());

            mockFileProvider.Verify(provider => provider.IsExistingFile("missing-image"), Times.Once());
        }

        private string NullReturningPostValidator(IInputArguments input)
        {
            return null;
        }

    }

}