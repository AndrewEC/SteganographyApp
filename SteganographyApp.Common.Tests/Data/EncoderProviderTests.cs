namespace SteganographyApp.Common.Tests;

using System;
using System.Security.Cryptography;
using Moq;

using NUnit.Framework;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;

[TestFixture]
public class EncoderProviderTests
{
    private readonly Mock<IImageProxy> mockImageProxy = new(MockBehavior.Strict);

    private EncoderProvider encoderProvider;

    [SetUp]
    public void SetUp()
    {
        encoderProvider = new(mockImageProxy.Object);
    }

    [TestCase(ImageFormat.Png, typeof(PngEncoder))]
    [TestCase(ImageFormat.Webp, typeof(WebpEncoder))]
    public void TestProvideEncoder(ImageFormat imageFormat, Type encoderType)
    {
        IImageEncoder provider = encoderProvider.GetEncoder(imageFormat);
        if (encoderType == typeof(PngEncoder))
        {
            AssertPngEncoder(provider);
        }
        else
        {
            AssertWebpEncoder(provider);
        }
    }

    [TestCase("image/png", typeof(PngEncoder))]
    [TestCase("image/webp", typeof(WebpEncoder))]
    public void TestGetEncoderByPath(string format, Type encoder)
    {
        string path = "/image/path";
        mockImageProxy.Setup(imageProxy => imageProxy.GetImageMimeType(path)).Returns(format);

        Assert.That(encoderProvider.GetEncoder(path), Is.AssignableFrom(encoder));
    }

    private static void AssertPngEncoder(IImageEncoder encoder)
    {
        Assert.That(encoder, Is.AssignableFrom(typeof(PngEncoder)));

        PngEncoder pngEncoder = (PngEncoder)encoder;

        Assert.That(pngEncoder.CompressionLevel, Is.EqualTo(PngCompressionLevel.Level5));
    }

    private static void AssertWebpEncoder(IImageEncoder encoder)
    {
        Assert.That(encoder, Is.AssignableFrom(typeof(WebpEncoder)));

        WebpEncoder webpEncoder = (WebpEncoder)encoder;

        Assert.Multiple(() =>
        {
            Assert.That(webpEncoder.FileFormat, Is.EqualTo(WebpFileFormatType.Lossless));
            Assert.That(webpEncoder.Method, Is.EqualTo(WebpEncodingMethod.BestQuality));
            Assert.That(webpEncoder.UseAlphaCompression, Is.False);
            Assert.That(webpEncoder.EntropyPasses, Is.EqualTo(0));
            Assert.That(webpEncoder.SpatialNoiseShaping, Is.EqualTo(0));
            Assert.That(webpEncoder.FilterStrength, Is.EqualTo(0));
            Assert.That(webpEncoder.TransparentColorMode, Is.EqualTo(WebpTransparentColorMode.Preserve));
            Assert.That(webpEncoder.Quality, Is.EqualTo(100));
            Assert.That(webpEncoder.NearLossless, Is.False);
        });
    }
}