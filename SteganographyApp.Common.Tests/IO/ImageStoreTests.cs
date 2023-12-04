namespace SteganographyApp.Common.Tests;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Moq;

using NUnit.Framework;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.IO;

[TestFixture]
public class ImageStoreTests : FixtureWithRealObjects
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
    public void TestWriteToImageWhenNotEnoughImageSpacesThrowsImageProcessingException()
    {
        var mockImage = GenerateMockImage(100, 100);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        mockEncoderProvider.Setup(encoderProvider => encoderProvider.GetEncoder(ImagePath)).Returns(new PngEncoder());

        string binaryString = GenerateBinaryString(BinaryStringLength);

        var imageStore = new ImageStore(Arguments);

        using (var wrapper = imageStore.CreateIOWrapper())
        {
            var exception = Assert.Throws<ImageProcessingException>(() => wrapper.WriteContentChunkToImage(binaryString));
            Assert.That(exception.Message, Is.EqualTo("Cannot load next image because there are no remaining cover images left to load."));
        }

        Assert.That(mockImage.DisposeCalled, Is.True);
        Assert.That(mockImage.SaveCalledWith, Is.EqualTo(Arguments.CoverImages[0]));
    }

    [Test]
    public void TestReadAndWriteContentChunkTable()
    {
        var mockImage = GenerateMockImage(1000, 1000);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);
        mockEncoderProvider.Setup(encoderProvider => encoderProvider.GetEncoder(ImagePath)).Returns<IEncoderProvider>(null);

        var chunkTableWrite = ImmutableArray.Create(new int[] { 100, 200, 300 });
        var imageStore = new ImageStore(Arguments);
        imageStore.SeekToImage(0);
        using (var tableWriter = new ChunkTableWriter(imageStore, Arguments))
        {
            tableWriter.WriteContentChunkTable(chunkTableWrite);
        }
        imageStore.SeekToImage(0);

        using (var reader = new ChunkTableReader(imageStore, Arguments))
        {
            var chunkTableRead = reader.ReadContentChunkTable();
            Assert.That(chunkTableRead, Has.Length.EqualTo(chunkTableWrite.Length));
            for (int i = 0; i < chunkTableWrite.Length; i++)
            {
                Assert.That(chunkTableRead[i], Is.EqualTo(chunkTableWrite[i]));
            }
        }
    }

    [Test]
    public void TestWriteContentChunkTableWithNotEnoughSpaceInImageThrowsImageProcessingException()
    {
        var mockImage = GenerateMockImage(1, 1);
        mockImageProxy.Setup(imageProxy => imageProxy.LoadImage(ImagePath)).Returns(mockImage);

        mockEncoderProvider.Setup(encoderProvider => encoderProvider.GetEncoder(ImagePath)).Returns(new PngEncoder());

        var imageStore = new ImageStore(Arguments);
        var chunkTable = Enumerable.Range(0, 100).ToImmutableArray();

        imageStore.SeekToImage(0);
        using (var writer = new ChunkTableWriter(imageStore, Arguments))
        {
            Assert.Throws<ImageProcessingException>(() => writer.WriteContentChunkTable(chunkTable));
        }

        Assert.That(mockImage.DisposeCalled, Is.True);
        Assert.That(mockImage.SaveCalledWith, Is.EqualTo(Arguments.CoverImages[0]));
    }

    private static MockBasicImageInfo GenerateMockImage(int width, int height)
    {
        var pixels = new Rgba32[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                pixels[i, j] = new Rgba32((byte)8);
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