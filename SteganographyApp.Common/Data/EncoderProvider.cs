namespace SteganographyApp.Common.Data
{
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Png;
    using SixLabors.ImageSharp.Formats.Webp;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    /// <summary>
    /// Contract for interacting with the underlying IEncoderProvider implementation.
    /// </summary>
    public interface IEncoderProvider
    {
        /// <include file='../docs.xml' path='docs/members[@name="EncoderProvider"]/GetEncoder/*' />
        public IImageEncoder GetEncoder(ImageFormat imageFormat);

        /// <include file='../docs.xml' path='docs/members[@name="EncoderProvider"]/GetEncoder2/*' />
        public IImageEncoder GetEncoder(string imagePath);
    }

    /// <summary>
    /// Provides a few method overloads with the end goal of retrieving the appropriate IImageEncoder instance
    /// based on the desired image type or file type.
    /// </summary>
    [Injectable(typeof(IEncoderProvider))]
    public class EncoderProvider : IEncoderProvider
    {
        private const string PngMimeType = "image/png";
        private const string WebpMimeType = "image/webp";

        /// <include file='../docs.xml' path='docs/members[@name="EncoderProvider"]/GetEncoder2/*' />
        public IImageEncoder GetEncoder(string imagePath)
        {
            var mimeType = Injector.Provide<IImageProxy>().GetImageMimeType(imagePath);
            switch (mimeType)
            {
                case PngMimeType:
                    return GetEncoder(ImageFormat.Png);
                case WebpMimeType:
                    return GetEncoder(ImageFormat.Webp);
                default:
                    throw new ArgumentValueException($"Could not find appropriate encoder for mime type: [{mimeType}]");
            }
        }

        /// <include file='../docs.xml' path='docs/members[@name="EncoderProvider"]/GetEncoder/*' />
        public IImageEncoder GetEncoder(ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ImageFormat.Png:
                    return new PngEncoder()
                    {
                        CompressionLevel = PngCompressionLevel.Level5,
                    };
                case ImageFormat.Webp:
                    return new WebpEncoder()
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
                    };
                default:
                    throw new ArgumentValueException($"Invalid image format provided. Could not find encoder for image format of: [{imageFormat}]");
            }
        }
    }
}