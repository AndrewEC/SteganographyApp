namespace SteganographyApp.Common.Tests
{
    using System.Collections.Immutable;

    using Moq;

    using NUnit.Framework;

    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Formats;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class ParserFunctionTests : FixtureWithTestObjects
    {
        [Mockup(typeof(IImageProxy))]
        public Mock<IImageProxy> mockImageProxy;

        [Mockup(typeof(IFileIOProxy))]
        public Mock<IFileIOProxy> mockFileProxy;

        [Test]
        public void ParseDummyCount()
        {
            var first = "image-path-1";
            var second = "image-path-2";
            var images = ImmutableArray.Create(first, second);

            mockImageProxy.Setup(proxy => proxy.LoadImage(first)).Returns(new ImageInfo());
            mockImageProxy.Setup(proxy => proxy.LoadImage(second)).Returns(new ImageInfo());

            int dummyCount = ParserFunctions.ParseDummyCount(true, images, "testing");

            Assert.NotZero(dummyCount);
        }

        [Test]
        public void ParseDummyCountWithInsertAsFalseReturnsZero()
        {
            int count = ParserFunctions.ParseDummyCount(false, ImmutableArray<string>.Empty, string.Empty);
            Assert.AreEqual(0, count);

            count = ParserFunctions.ParseDummyCount(true, ImmutableArray<string>.Empty, string.Empty);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void ParseFilePath()
        {
            string expected = "image-path";
            mockFileProxy.Setup(proxy => proxy.IsExistingFile(expected)).Returns(true);

            string actual = ParserFunctions.ParseFilePath(expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseFilePathThrowsExceptionWhenFileDoesNotExist()
        {
            string expected = "image-path";
            mockFileProxy.Setup(proxy => proxy.IsExistingFile(expected)).Returns(false);

            Assert.Throws(typeof(ArgumentValueException), () => ParserFunctions.ParseFilePath(expected));
        }
    }

    internal sealed class ImageInfo : IBasicImageInfo
    {
        public int Width => 10;

        public int Height => 10;

        public Rgba32 this[int x, int y] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void Dispose()
        {
        }

        public void Save(string pathToImage, IImageEncoder encoder)
        {
        }
    }
}