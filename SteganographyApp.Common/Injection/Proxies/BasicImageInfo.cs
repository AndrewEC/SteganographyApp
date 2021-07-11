namespace SteganographyApp.Common.Injection
{
    using System;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

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

        public BasicImageInfo(Image<Rgba32> image)
        {
            this.image = image;
        }

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

        public void Dispose()
        {
            image.Dispose();
        }

        public void Save(string pathToImage) => image.Save(pathToImage);
    }
}