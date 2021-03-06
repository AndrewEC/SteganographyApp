namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;

    using Moq;

    using NUnit.Framework;

    using SixLabors.ImageSharp.PixelFormats;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;

    using static Moq.It;
    using static Moq.Times;

    [TestFixture]
    public class ImageStoreTests : FixtureWithRealObjects
    {
        private static readonly int BinaryStringLength = 100_000;
        private static readonly IInputArguments Arguments = new InputArguments()
        {
            CoverImages = ImmutableArray.Create(new string[] { "test001.png" }),
        }
        .ToImmutable();

        private string imageLoadededEventPath;

        private Mock<IImageProxy> mockImageProxy;

        [SetUp]
        public void Initialize()
        {
            mockImageProxy = new Mock<IImageProxy>();
            Injector.UseInstance(mockImageProxy.Object);
        }

        [Test]
        public void TestCleanImages()
        {
            var mockImage = GenerateMockImage(100, 100);
            mockImageProxy.Setup(provider => provider.LoadImage(IsAny<string>())).Returns(mockImage);

            var imageStore = new ImageStore(Arguments);
            imageStore.OnNextImageLoaded += OnNextImageLoaded;
            imageStore.CleanImageLSBs();

            Assert.AreEqual(Arguments.CoverImages[0], imageLoadededEventPath);
            Assert.IsTrue(mockImage.DisposeCalled);
            Assert.AreEqual(Arguments.CoverImages[0], mockImage.SaveCalledWith);
            mockImageProxy.Verify(provider => provider.LoadImage(Arguments.CoverImages[0]), Once());
        }

        [Test]
        public void TestWriteToImageWhenNotEnoughImageSpacesThrowsImageProcessingException()
        {
            var mockImage = GenerateMockImage(100, 100);
            mockImageProxy.Setup(provider => provider.LoadImage(IsAny<string>())).Returns(mockImage);

            string binaryString = GenerateBinaryString(BinaryStringLength);

            var imageStore = new ImageStore(Arguments);

            using (var wrapper = imageStore.CreateIOWrapper())
            {
                var exception = Assert.Throws<ImageProcessingException>(() => wrapper.WriteContentChunkToImage(binaryString));
                Assert.AreEqual("There is not enough available storage space in the provided images to continue.", exception.Message);
            }

            Assert.IsTrue(mockImage.DisposeCalled);
            Assert.AreEqual(Arguments.CoverImages[0], mockImage.SaveCalledWith);
        }

        [Test]
        public void TestReadAndWriteContentChunkTable()
        {
            var mockImage = GenerateMockImage(1000, 1000);
            mockImageProxy.Setup(provider => provider.LoadImage(IsAny<string>())).Returns(mockImage);

            var chunkTableWrite = ImmutableArray.Create(new int[] { 100, 200, 300 });
            var imageStore = new ImageStore(Arguments);
            imageStore.ResetToImage(0);
            imageStore.WriteContentChunkTable(chunkTableWrite);
            imageStore.ResetToImage(0);
            var chunkTableRead = imageStore.ReadContentChunkTable();

            Assert.AreEqual(chunkTableWrite.Length, chunkTableRead.Length);
            for (int i = 0; i < chunkTableWrite.Length; i++)
            {
                Assert.AreEqual(chunkTableWrite[i], chunkTableRead[i]);
            }
        }

        [Test]
        public void TestWriteContentChunkTableWithNotEnoughSpaceInImageThrowsImageProcessingException()
        {
            var mockImage = GenerateMockImage(1, 1);
            mockImageProxy.Setup(provider => provider.LoadImage(IsAny<string>())).Returns(mockImage);

            var imageStore = new ImageStore(Arguments);
            var chunkTable = Enumerable.Range(0, 100).ToImmutableArray();

            imageStore.ResetToImage(0);
            Assert.Throws<ImageProcessingException>(() => imageStore.WriteContentChunkTable(chunkTable));

            Assert.IsTrue(mockImage.DisposeCalled);
            Assert.AreEqual(Arguments.CoverImages[0], mockImage.SaveCalledWith);
        }

        private string GenerateBinaryString(int length)
        {
            var random = new Random();
            var builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                string nextBit = ((int)Math.Round(random.NextDouble())).ToString();
                builder.Append(nextBit);
            }
            return builder.ToString();
        }

        private void OnNextImageLoaded(object sender, NextImageLoadedEventArgs args)
        {
            imageLoadededEventPath = args.ImageName;
        }

        private MockBasicImageInfo GenerateMockImage(int width, int height)
        {
            var pixels = new Rgba32[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixels[i, j] = new Rgba32((byte)8);
                }
            }

            return new MockBasicImageInfo(width, height, pixels);
        }
    }

    internal class MockBasicImageInfo : IBasicImageInfo
    {
        private Rgba32[,] pixels;

        public MockBasicImageInfo(int width, int height, Rgba32[,] pixels)
        {
            Width = width;
            Height = height;
            this.pixels = pixels;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public string SaveCalledWith { get; private set; }

        public bool DisposeCalled { get; private set; }

        public Rgba32 this[int x, int y]
        {
            get
            {
                return pixels[x, y];
            }

            set
            {
                pixels[x, y] = value;
            }
        }

        public void Save(string pathToImage)
        {
            SaveCalledWith = pathToImage;
        }

        public void Dispose()
        {
            DisposeCalled = true;
        }
    }
}