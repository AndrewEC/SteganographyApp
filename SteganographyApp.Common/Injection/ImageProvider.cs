using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SteganographyApp.Common.Injection
{

    /// <summary>
    /// An interface wrapper for the ImageSharp Image class instance.
    /// </summary>
    public interface IBasicImageInfo : IDisposable
    {
        int Width { get; }
        int Height { get; }

        Rgba32 this[int x, int y]
        {
            get;
            set;
        }

        void Save(string pathToImage);

    }

    public class BasicImageInfo : IBasicImageInfo
    {

        private readonly Image<Rgba32> image;

        public int Width
        {
            get
            {
                return image.Width;
            }
        }

        public int Height
        {
            get
            {
                return image.Height;
            }
        }

        public BasicImageInfo(Image<Rgba32> image)
        {
            this.image = image;
        }

        public void Dispose()
        {
            image.Dispose();
        }

        public Rgba32 this[int x, int y]
        {
            get
            {
                return image[x, y];
            }

            set
            {
                image[x, y] = value;
            }
        }

        public void Save(string pathToImage)
        {
            image.Save(pathToImage);
        }

    }

    /// <summary>
    /// An interface wrapper for the SixLabors Image loader class.
    /// Intended to allow mocking of the load function so unit tests can
    /// test with a loaded image without the need for an actual image file
    /// to be present.
    /// </summary>
    public interface IImageProvider
    {
        IBasicImageInfo LoadImage(string pathToImage);
    }

    [Injectable(typeof(IImageProvider))]
    public class ImageProvider : IImageProvider
    {

        public IBasicImageInfo LoadImage(string pathToImage)
        {
            return new BasicImageInfo(Image.Load(pathToImage));
        }

    }

}