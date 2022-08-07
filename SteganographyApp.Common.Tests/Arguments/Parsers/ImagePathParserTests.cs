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

            ImmutableArray<string> images = ImagePathParser.ParseImages(image);

            Assert.AreEqual(1, images.Length);
            Assert.AreEqual(image, images[0]);

            mockFileIOProxy.Verify(provider => provider.IsExistingFile(image), Once());
        }

        [Test]
        public void TestParseImagesWithValidRegex()
        {
            var firstPath = "./Test001.png";
            var secondPath = "./Test002.png";
            mockFileIOProxy.Setup(provider => provider.GetFiles("./TestAssets")).Returns(new string[] { firstPath, secondPath });
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(firstPath)).Returns(true);
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(secondPath)).Returns(true);

            string expression = "[r]<^[\\w\\W]+\\.(png)$><./TestAssets>";
            ImmutableArray<string> images = ImagePathParser.ParseImages(expression);

            Assert.AreEqual(2, images.Length);
            Assert.AreEqual(firstPath, images[0]);
            Assert.AreEqual(secondPath, images[1]);

            mockFileIOProxy.Verify(provider => provider.IsExistingFile(IsAny<string>()), Exactly(2));
        }

        [Test]
        public void TestParseImagesWithInvalidSingleValuesReturnsFalseAndProducesParseException()
        {
            var missingPath = "missing-image";
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(missingPath)).Returns(false);
            Assert.Throws(typeof(ArgumentValueException), () => ImagePathParser.ParseImages(missingPath));
        }

        protected override void SetupMocks()
        {
            mockFileIOProxy.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(true);
        }
    }
}