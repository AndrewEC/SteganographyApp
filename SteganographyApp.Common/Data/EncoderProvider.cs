namespace SteganographyApp.Common.Data;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Contract for interacting with the underlying IEncoderProvider implementation.
/// </summary>
public interface IEncoderProvider
{
    /// <summary>
    /// Attempts to instantiate and return an IImageEncoder instance associated with the desired image format.
    /// </summary>
    /// <param name="imageFormat">The desired image format.</param>
    /// <returns>An IImageEncoder instance that corresponds to the requested image format.</returns>
    /// <exception cref="ArgumentValueException">Thrown if there is no encoder associated
    /// with the input image format.</exception>
    public IImageEncoder GetEncoder(ImageFormat imageFormat);

    /// <summary>
    /// Attempts to instantiate and return an IImageEncoder instance associated with the image format.
    /// </summary>
    /// <param name="imagePath">The path to the image from which the extension will be pulled and used to determine the correct
    /// encoder instance.</param>
    /// <returns>An IImageEncoder instance that corresponds to the requested image format.</returns>
    /// <exception cref="ArgumentValueException">Thrown if there is no encoder associated with the input image format.</exception>
    public IImageEncoder GetEncoder(string imagePath);
}

/// <summary>
/// Provides a few method overloads with the end goal of retrieving the appropriate IImageEncoder instance
/// based on the desired image type or file type.
/// </summary>
public class EncoderProvider : IEncoderProvider
{
    private const string PngMimeType = "image/png";
    private const string WebpMimeType = "image/webp";

    private readonly IImageProxy imageProxy;

#pragma warning disable CS1591, SA1600
    public EncoderProvider(IImageProxy imageProxy)
    {
        this.imageProxy = imageProxy;
    }
#pragma warning restore CS1591, SA1600

    /// <inheritdoc/>
    public IImageEncoder GetEncoder(string imagePath) => imageProxy.GetImageMimeType(imagePath) switch
    {
        PngMimeType => GetEncoder(ImageFormat.Png),
        WebpMimeType => GetEncoder(ImageFormat.Webp),
        _ => throw new ArgumentValueException($"Could not find appropriate encoder for file: [{imagePath}]"),
    };

    /// <inheritdoc/>
    public IImageEncoder GetEncoder(ImageFormat imageFormat) => imageFormat switch
    {
        ImageFormat.Png => new PngEncoder() { CompressionLevel = PngCompressionLevel.Level5, },
        ImageFormat.Webp => new WebpEncoder()
        {
            FileFormat = WebpFileFormatType.Lossless,
            Method = WebpEncodingMethod.BestQuality,
            UseAlphaCompression = false,
            EntropyPasses = 0,
            SpatialNoiseShaping = 0,
            FilterStrength = 0,
            TransparentColorMode = WebpTransparentColorMode.Preserve,
            Quality = 100,
            NearLossless = false,
            NearLosslessQuality = 100,
        },
        _ => throw new ArgumentValueException($"Invalid image format provided. Could not find encoder for image format of: [{imageFormat}]"),
    };
}