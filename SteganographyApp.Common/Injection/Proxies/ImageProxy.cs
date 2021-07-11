namespace SteganographyApp.Common.Injection
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    /// <summary>
    /// An interface wrapper for the SixLabors Image loader class.
    /// Intended to allow mocking of the load function so unit tests can
    /// test with a loaded image without the need for an actual image file
    /// to be present.
    /// </summary>
    public interface IImageProxy
    {
        IBasicImageInfo LoadImage(string pathToImage);
    }

    [Injectable(typeof(IImageProxy))]
    public class ImageProxy : IImageProxy
    {
        public IBasicImageInfo LoadImage(string pathToImage) => new BasicImageInfo(Image.Load<Rgba32>(pathToImage));
    }
}