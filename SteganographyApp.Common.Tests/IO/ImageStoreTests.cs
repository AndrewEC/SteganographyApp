namespace SteganographyApp.Common.Tests;

using System;
using System.Collections.Immutable;
using System.Text;

using Moq;

using NUnit.Framework;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO;

[TestFixture]
public class ImageStoreTests : FixtureWithTestObjects
{
    [Mockup(typeof(IImageProxy))]
    public Mock<IImageProxy> mockImageProxy = new();

    [Mockup(typeof(IEncoderProvider))]
    public Mock<IEncoderProvider> mockEncoderProvider = new();

    private const int BinaryStringLength = 100_000;
    private const string ImagePath = "./test001.png";
    private static readonly IInputArguments Arguments = new CommonArguments
    {
        CoverImages = ImmutableArray.Create(new string[] { ImagePath }),
    };

    [Test]
    public void TestOpenMultipleStreamsThrowsException()
    {
        var mockImage = GenerateMockImage(100, 100);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        var imageStore = new ImageStore(Arguments);
        imageStore.OpenStream(StreamMode.Read);
        Assert.Throws<ImageStoreException>(() => imageStore.OpenStream(StreamMode.Read));
    }

    [Test]
    public void TestWriteToImageWhenNotEnoughImageSpaceAvailableThrowsImageStoreException()
    {
        var mockImage = GenerateMockImage(100, 100);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        mockEncoderProvider.Setup(encoderProvider => encoderProvider.GetEncoder(ImagePath)).Returns(new PngEncoder());

        string binaryString = GenerateBinaryString(BinaryStringLength);

        using (var stream = new ImageStore(Arguments).OpenStream(StreamMode.Write))
        {
            var exception = Assert.Throws<ImageStoreException>(() => stream.WriteContentChunkToImage(binaryString));
            Assert.That(
                exception?.Message ?? "No Message",
                Is.EqualTo("Cannot load next image because there are no remaining cover images left to load."));
        }

        Assert.That(mockImage.DisposeCalled, Is.True);
        Assert.That(mockImage.SaveCalledWith, Is.EqualTo(Arguments.CoverImages[0]));
    }

    [Test]
    public void TestReadFromImageWhenNotEnoughImageSpaceAvailableThrowsImageStoreException()
    {
        var mockImage = GenerateMockImage(1, 1);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        using (var stream = new ImageStore(Arguments).OpenStream(StreamMode.Read))
        {
            var exception = Assert.Throws<ImageStoreException>(() => stream.ReadContentChunkFromImage(1000));
            Assert.That(
                exception?.Message ?? "No Message",
                Is.EqualTo("Cannot load next image because there are no remaining cover images left to load."));
        }

        Assert.That(mockImage.DisposeCalled, Is.True);
    }

    [Test]
    public void TestSeekToImage()
    {
        var mockImage = GenerateMockImage(100, 100);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        var store = new ImageStore(Arguments);
        using (var stream = store.OpenStream(StreamMode.Read))
        {
            stream.SeekToImage(0);
            Assert.Throws<ImageStoreException>(() => stream.SeekToImage(2));
            Assert.Throws<ImageStoreException>(() => stream.SeekToImage(-1));
        }
    }

    [Test]
    public void TestSeekToPixelOutsideAvailablePixelRangeThrowsException()
    {
        var mockImage = GenerateMockImage(1, 1);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        using (var stream = new ImageStore(Arguments).OpenStream(StreamMode.Read))
        {
            Assert.Throws<ImageStoreException>(() => stream.SeekToPixel(100_000));
        }
    }

    private static MockBasicImageInfo GenerateMockImage(int width, int height)
    {
        var pixels = new Rgba32[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                pixels[i, j] = new Rgba32(8);
            }
        }

        return new MockBasicImageInfo(width, height, ImagePath, pixels);
    }

    private static string GenerateBinaryString(int length)
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
}

internal class MockBasicImageInfo(int width, int height, string path, Rgba32[,] pixels) : IBasicImageInfo
{
    private readonly Rgba32[,] pixels = pixels;

    public int Width { get; set; } = width;

    public int Height { get; set; } = height;

    public string Path { get; set; } = path;

    public string SaveCalledWith { get; private set; } = string.Empty;

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

    public void Save(string pathToImage, IImageEncoder encoder)
    {
        SaveCalledWith = pathToImage;
    }

    public void Dispose()
    {
        DisposeCalled = true;
    }
}