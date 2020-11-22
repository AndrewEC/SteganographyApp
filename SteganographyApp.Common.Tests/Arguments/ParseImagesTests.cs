using System.Collections.Immutable;

using Moq;

using NUnit.Framework;

using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Arguments;

using static Moq.Times;
using static Moq.It;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class ParseImagesTests : FixtureWithMockConsoleReaderAndWriter
    {

        [Mockup(typeof(IFileProvider))]
        public Mock<IFileProvider> mockFileProvider;

        protected override void SetupMocks()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(true);
        }

        [Test]
        public void TestParseImagesWithValidSingleValue()
        {
            string image = "Test001.png";
            string[] inputArgs = new string[] { "--images", image };
            var parser = new ArgumentParser();

            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);

            ImmutableArray<string> images = arguments.CoverImages;
            Assert.AreEqual(1, images.Length);
            Assert.AreEqual(image, images[0]);

            mockFileProvider.Verify(provider => provider.IsExistingFile(image), Once());
        }

        [Test]
        public void TestParseImagesWithValidRegex()
        {
            mockFileProvider.Setup(provider => provider.GetFiles(IsAny<string>())).Returns(new string[] { "./Test001.png", "./Test002.png" });

            string expression = "[r]<^[\\w\\W]+\\.(png)$><./TestAssets>";
            string[] inputArgs = new string[] { "--images", expression };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);

            ImmutableArray<string> images = arguments.CoverImages;
            Assert.AreEqual(2, images.Length);
            Assert.AreEqual("./Test001.png", images[0]);
            Assert.AreEqual("./Test002.png", images[1]);

            mockFileProvider.Verify(provider => provider.IsExistingFile(IsAny<string>()), Exactly(2));
        }

        [Test]
        public void TestParseImagesWithInvalidSingleValuesReturnsFalseAndProducesParseException()
        {
            mockFileProvider.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(false);

            string[] inputArgs = new string[] { "--images", "missing-image" };    
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());

            mockFileProvider.Verify(provider => provider.IsExistingFile("missing-image"), Once());
        }

        private string NullReturningPostValidator(IInputArguments input) => null;

    }

}