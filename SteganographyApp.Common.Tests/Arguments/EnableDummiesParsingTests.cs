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
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(provider => provider.IsExistingFile(IsAny<string>())).Returns(true);
            Injector.UseInstance(mockFileProvider.Object);

            var mockImage = new Mock<IBasicImageInfo>();
            mockImage.Setup(image => image.Width).Returns(100);
            mockImage.Setup(image => image.Height).Returns(100);

            var mockImageProvider = new Mock<IImageProvider>();
            mockImageProvider.Setup(provider => provider.LoadImage(IsAny<string>())).Returns(mockImage.Object);
            Injector.UseInstance(mockImageProvider.Object);

            string imagePath = "Test001.png";
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