namespace SteganographyApp.Common.Tests
{
    using System;

    using Moq;

    using NUnit.Framework;

    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.Formats.Webp;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

    [TestFixture]
    public class EncoderProviderTests : FixtureWithTestObjects
    {
        [Mockup(typeof(IImageProxy))]
        public Mock<IImageProxy> mockImageProxy;

        [TestCase(ImageFormat.Png, typeof(PngEncoder))]
        [TestCase(ImageFormat.Webp, typeof(WebpEncoder))]
        public void TestProvideEncoder(ImageFormat imageFormat, Type encoderType)
        {
            Assert.IsAssignableFrom(encoderType, new EncoderProvider().GetEncoder(imageFormat));
        }

        [TestCase("image/png", typeof(PngEncoder))]
        [TestCase("image/webp", typeof(WebpEncoder))]
        public void TestGetEncoderByPath(string format, Type encoder)
        {
            string path = "/image/path";
            mockImageProxy.Setup(proxy => proxy.GetImageMimeType(path)).Returns(format);

            Assert.IsAssignableFrom(encoder, new EncoderProvider().GetEncoder(path));
        }
    }
}