namespace SteganographyApp.Common.Injection.Proxies;

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
    /// <include file='../../docs.xml' path='docs/members[@name="ImageProxy"]/LoadImage/*' />
    IBasicImageInfo LoadImage(string pathToImage);

    /// <include file='../../docs.xml' path='docs/members[@name="ImageProxy"]/GetImageMimeType/*' />
    string GetImageMimeType(string pathToImage);
}

/// <summary>
/// The concrete implementation to allow proxying calls to the static Image.Load provided
/// in the image sharp API.
/// </summary>
[Injectable(typeof(IImageProxy))]
public class ImageProxy : IImageProxy
{
    /// <include file='../../docs.xml' path='docs/members[@name="ImageProxy"]/LoadImage/*' />
    public IBasicImageInfo LoadImage(string pathToImage) => new BasicImageInfo(pathToImage, Image.Load<Rgba32>(pathToImage));

    /// <include file='../../docs.xml' path='docs/members[@name="ImageProxy"]/GetImageMimeType/*' />
    public string GetImageMimeType(string pathToImage) => Image.DetectFormat(pathToImage).DefaultMimeType;
}