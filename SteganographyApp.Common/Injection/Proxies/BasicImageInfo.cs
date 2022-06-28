namespace SteganographyApp.Common.Injection
{
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
    public class BasicImageInfo : IBasicImageInfo
    {
        private readonly Image<Rgba32> image;

        /// <summary>
        /// Initialize the BasicImageInfo instance using the image data loaded by the image sharp API.
        /// </summary>
        /// <param name="image">The image data.</param>
        public BasicImageInfo(Image<Rgba32> image)
        {
            this.image = image;
        }

        /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Width/*' />
        public int Width
        {
            get
            {
                return image.Width;
            }
        }

         /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Height/*' />
        public int Height
        {
            get
            {
                return image.Height;
            }
        }

         /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Accessor/*' />
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

        /// <summary>
        /// Proxies the call to the Dispose method of the image managed by this class.
        /// </summary>
        public void Dispose()
        {
            image.Dispose();
        }

         /// <include file='../../docs.xml' path='docs/members[@name="BasicImageInfo"]/Save/*' />
        public void Save(string pathToImage, IImageEncoder encoder) => image.Save(pathToImage, encoder);
    }
}