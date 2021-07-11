namespace SteganographyApp.Common.Tests
{
    using System.Collections.Immutable;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    using static Moq.It;
    using static Moq.Times;

    [TestFixture]
    public class ParseImagesTests : FixtureWithMockConsoleReaderAndWriter
    {
        [Mockup(typeof(IFileIOProxy))]
        public Mock<IFileIOProxy> mockFileIOProxy;

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

            mockFileIOProxy.Verify(provider => provider.IsExistingFile(image), Once());
        }

        [Test]
        public void TestParseImagesWithValidRegex()
        {
            mockFileIOProxy.Setup(provider => provider.GetFiles(IsAny<string>())).Returns(new string[] { "./Test001.png", "./Test002.png" });

            string expression = "[r]<^[\\w\\W]+\\.(png)$><./TestAssets>";
            string[] inputArgs = new string[] { "--images", expression };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);

            ImmutableArray<string> images = arguments.CoverImages;
            Assert.AreEqual(2, images.Length);
            Assert.AreEqual("./Test001.png", images[0]);
            Assert.AreEqual("./Test002.png", images[1]);

            mockFileIOProxy.Verify(provider => provider.IsExistingFile(IsAny<string>()), Exactly(2));
        }

        [Test]
        public void TestParseImagesWithInvalidSingleValuesReturnsFalseAndProducesParseException()
        {
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(false);

            string[] inputArgs = new string[] { "--images", "missing-image" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());

            mockFileIOProxy.Verify(provider => provider.IsExistingFile("missing-image"), Once());
    }

        protected override void SetupMocks()
        {
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(true);
        }

        private string NullReturningPostValidator(IInputArguments input) => null;
    }
}