namespace SteganographyApp.Common;

/// <summary>
/// When using the Converter this specifies the format the image will be converted to.
/// </summary>
public enum ImageFormat
{
    /// <summary>Indicates the image(s) should be converted to a png file.</summary>
    Png,

    /// <summary>Indicates the image(s) should be converted to a webp file.</summary>
    Webp,
}