namespace SteganographyApp.Common.Tests
{
    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    using static Moq.It;
    using static Moq.Times;

    [TestFixture]
    public class EnableDummiesParsingTests : FixtureWithMockConsoleReaderAndWriter
    {

        [Mockup(typeof(IFileIOProxy))]
        public Mock<IFileIOProxy> mockFileIOProxy;

        [Mockup(typeof(IBasicImageInfo))]
        public Mock<IBasicImageInfo> mockImage;

        [Mockup(typeof(IImageProxy))]
        public Mock<IImageProxy> mockImageProvider;

        [Test]
        public void TestParseInsertDummiesWithValidValue()
        {
            string[] inputArgs = new string[] { "--enableDummies" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, (IInputArguments input) => null));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.InsertDummies);
        }

        [Test]
        public void TestParseDummyCountIsParsedWhenDummiesAreEnabledAndCoverImagesAreProvided()
        {
            string imagePath = "Test001.png";

            mockFileIOProxy.Setup(provider => provider.IsExistingFile(imagePath)).Returns(true);

            mockImage.Setup(image => image.Width).Returns(100);
            mockImage.Setup(image => image.Height).Returns(100);

            mockImageProvider.Setup(provider => provider.LoadImage(IsAny<string>())).Returns(mockImage.Object);

            string[] inputArgs = new string[] { "--enableDummies", "--images", imagePath };
            var first = new ArgumentParser();

            string[] secondaryArgs = new string[] { "--enableDummies", "--images", imagePath, "--randomSeed", "randomSeedValue" };
            var second = new ArgumentParser();

            Assert.IsTrue(first.TryParse(inputArgs, out IInputArguments firstArguments, (IInputArguments arguments) => null));
            Assert.IsTrue(firstArguments.InsertDummies);
            Assert.AreNotEqual(0, firstArguments.DummyCount);

            Assert.IsTrue(second.TryParse(secondaryArgs, out IInputArguments secondArguments, (IInputArguments arguments) => null));
            Assert.IsTrue(secondArguments.InsertDummies);
            Assert.AreNotEqual(0, secondArguments.DummyCount);

            Assert.AreNotEqual(firstArguments.DummyCount, secondArguments.DummyCount);

            mockImageProvider.Verify(provider => provider.LoadImage(imagePath), Exactly(4));
        }
    }
}