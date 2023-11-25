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
        public IImageEncoder GetEncoder(string imagePath) => Injector.Provide<IImageProxy>().GetImageMimeType(imagePath) switch
        {
            PngMimeType => GetEncoder(ImageFormat.Png),
            WebpMimeType => GetEncoder(ImageFormat.Webp),
            _ => throw new ArgumentValueException($"Could not find appropriate encoder for file: [{imagePath}]")
        };

        /// <include file='../docs.xml' path='docs/members[@name="EncoderProvider"]/GetEncoder/*' />
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
            _ => throw new ArgumentValueException($"Invalid image format provided. Could not find encoder for image format of: [{imageFormat}]")
        };
    }
}