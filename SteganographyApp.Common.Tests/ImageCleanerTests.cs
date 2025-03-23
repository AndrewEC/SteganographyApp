namespace SteganographyApp.Common.Tests;

using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO;

[TestFixture]
public class ImageCleanerTests
{
    private const string ImagePath = "CoverImagePath";

    [Test]
    public void TestCleanImages()
    {
        IInputArguments arguments = BuildArguments();
        Mock<IImageStore> mockStore = new(MockBehavior.Strict);
        Mock<IImageStoreStream> mockStream = new(MockBehavior.Strict);

        mockStream.Setup(stream => stream.WriteContentChunkToImage(It.IsAny<string>()))
            .Returns(1);

        mockStore.Setup(store => store.OpenStream(StreamMode.Write))
            .Returns(mockStream.Object);

        mockStore.Setup(store => store.CurrentImage)
            .Returns(BuildMockImage());

        mockStream.Setup(stream => stream.Dispose()).Verifiable();

        new ImageCleaner(arguments, mockStore.Object, ServiceContainer.GetService<ICalculator>()).CleanImages();

        mockStream.Verify(stream => stream.WriteContentChunkToImage(It.IsAny<string>()), Times.Exactly(2));
        mockStream.Verify(stream => stream.Dispose(), Times.Exactly(2));
    }

    private static IBasicImageInfo BuildMockImage()
    {
        Mock<IBasicImageInfo> mockImage = new(MockBehavior.Strict);
        mockImage.Setup(image => image.Width).Returns(10);
        mockImage.Setup(image => image.Height).Returns(12);
        mockImage.Setup(image => image.Path).Returns(ImagePath);
        return mockImage.Object;
    }

    private static IInputArguments BuildArguments() => new CommonArguments
    {
        CoverImages = [ImagePath],
        BitsToUse = 2,
    };
}