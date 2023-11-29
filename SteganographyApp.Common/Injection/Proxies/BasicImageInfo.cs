namespace SteganographyApp.Common.Injection;

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
public class BasicImageInfo(Image<Rgba32> image) : AbstractDisposable, IBasicImageInfo
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
    /// <param name="disposing">Specifies if this method is being invoked from the main Dispose method or the finalizer.</param>
    protected override void Dispose(bool disposing) => RunIfNotDisposed(() =>
    {
        if (!disposing)
        {
            return;
        }
        image.Dispose();
    });
}