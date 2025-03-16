namespace SteganographyApp.Common.Tests;

using System;

using Moq;

using NUnit.Framework;

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
        Assert.That(encoderProvider.GetEncoder(imageFormat), Is.AssignableFrom(encoderType));
    }

    [TestCase("image/png", typeof(PngEncoder))]
    [TestCase("image/webp", typeof(WebpEncoder))]
    public void TestGetEncoderByPath(string format, Type encoder)
    {
        string path = "/image/path";
        mockImageProxy.Setup(imageProxy => imageProxy.GetImageMimeType(path)).Returns(format);

        Assert.That(encoderProvider.GetEncoder(path), Is.AssignableFrom(encoder));
    }
}