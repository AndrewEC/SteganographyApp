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
    /// <summary>
    /// Invokes the Image.Load from the image sharp API to load an image from the provided path.
    /// </summary>
    /// <param name="pathToImage">The absolute or relative path to the image to load.</param>
    /// <returns>A new IBasicImageInfo instance loaded from the specified path.</returns>
    IBasicImageInfo LoadImage(string pathToImage);

    /// <summary>
    /// Retrieves the format of the image located at the provided path.
    /// </summary>
    /// <param name="pathToImage">The absolute or relative path to the image.</param>
    /// <returns>The format of the image located at the input path.</returns>
    string GetImageMimeType(string pathToImage);
}

/// <summary>
/// The concrete implementation to allow proxying calls to the static Image.Load provided
/// in the image sharp API.
/// </summary>
public class ImageProxy : IImageProxy
{
    /// <inheritdoc/>
    public IBasicImageInfo LoadImage(string pathToImage) => new BasicImageInfo(pathToImage, Image.Load<Rgba32>(pathToImage));

    /// <inheritdoc/>
    public string GetImageMimeType(string pathToImage) => Image.DetectFormat(pathToImage).DefaultMimeType;
}