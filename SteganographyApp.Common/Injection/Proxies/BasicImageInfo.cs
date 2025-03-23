namespace SteganographyApp.Common.Injection.Proxies;

using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// An interface wrapper for the ImageSharp Image class instance.
/// </summary>
public interface IBasicImageInfo : IDisposable
{
    /// <summary>Gets the width of the currently loaded image.</summary>
    int Width { get; }

    /// <summary>Gets the height of the currently loaded image.</summary>
    int Height { get; }

    /// <summary>Gets the absolute path to the image file at the time it was loaded.</summary>
    string Path { get; }

    /// <summary>
    /// Accessor to lookup the colour of a pixel at the specified position.
    /// </summary>
    /// <param name="x">The x position of the pixel.</param>
    /// <param name="y">The y position of the pixel.</param>
    /// <returns>The <see cref="Rgba32"/> pixel at the specified position.</returns>
    Rgba32 this[int x, int y]
    {
        get;
        set;
    }

    /// <summary>
    /// Save the currently loaded image to the specified location.
    /// </summary>
    /// <param name="pathToImage">The absolute or relative file path where the image should be written to.</param>
    /// <param name="encoder">The encoding options to save the image with.</param>
    void Save(string pathToImage, IImageEncoder encoder);
}

/// <summary>
/// The concrete implementation of IBasicImageInfo. Used to manipulate images loaded
/// via the image sharp library.
/// </summary>
/// <remarks>
/// Initialize the BasicImageInfo instance using the image data loaded by the image sharp API.
/// </remarks>
/// <param name="image">The image data.</param>
/// <param name="path">The absolute path the image at the time it was loaded from disk.</param>
public class BasicImageInfo(string path, Image<Rgba32> image) : AbstractDisposable, IBasicImageInfo
{
    private readonly Image<Rgba32> image = image;

    /// <inheritdoc/>
    public int Width
    {
        get => RunIfNotDisposedWithResult(() => image.Width);
    }

    /// <inheritdoc/>
    public int Height
    {
        get => RunIfNotDisposedWithResult(() => image.Height);
    }

    /// <inheritdoc/>
    public string Path
    {
        get => path;
    }

    /// <inheritdoc/>
    public Rgba32 this[int x, int y]
    {
        get => RunIfNotDisposedWithResult(() => image[x, y]);

        set => RunIfNotDisposed(() => image[x, y] = value);
    }

    /// <inheritdoc/>
    public void Save(string pathToImage, IImageEncoder encoder) => RunIfNotDisposed(() => image.Save(pathToImage, encoder));

    /// <summary>
    /// Proxies the call to the Dispose method of the image managed by this class.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() => image.Dispose());
}