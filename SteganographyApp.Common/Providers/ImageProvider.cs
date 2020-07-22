using System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SteganographyApp.Common.Providers
{

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

    public interface IImageProvider
    {
        IBasicImageInfo LoadImage(string pathToImage);
    }

    public class ImageProvider : IImageProvider
    {

        public IBasicImageInfo LoadImage(string pathToImage)
        {
            return new BasicImageInfo(Image.Load(pathToImage));
        }

    }

}