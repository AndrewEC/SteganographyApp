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
    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Width/*' />
    int Width { get; }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Height/*' />
    int Height { get; }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Path/*' />
    string Path { get; }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Accessor/*' />
    Rgba32 this[int x, int y]
    {
        get;
        set;
    }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Save/*' />
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

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Width/*' />
    public int Width
    {
        get => RunIfNotDisposedWithResult(() => image.Width);
    }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Height/*' />
    public int Height
    {
        get => RunIfNotDisposedWithResult(() => image.Height);
    }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Path/*' />
    public string Path
    {
        get => path;
    }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Accessor/*' />
    public Rgba32 this[int x, int y]
    {
        get => RunIfNotDisposedWithResult(() => image[x, y]);

        set => RunIfNotDisposed(() => image[x, y] = value);
    }

    /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Save/*' />
    public void Save(string pathToImage, IImageEncoder encoder) => RunIfNotDisposed(() => image.Save(pathToImage, encoder));

    /// <summary>
    /// Proxies the call to the Dispose method of the image managed by this class.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() => image.Dispose());
}