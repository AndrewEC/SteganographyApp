namespace SteganographyApp.Common.Data
{
    using System.IO;
    using System.IO.Compression;

    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Contract for interacting with the CompressionUtil instance.
    /// </summary>
    public interface ICompressionUtil
    {
        /// <include file='../docs.xml' path='docs/members[@name="CompressionUtil"]/Compress/*' />
        byte[] Compress(byte[] fileBytes);

        /// <include file='../docs.xml' path='docs/members[@name="CompressionUtil"]/Decompress/*' />
        byte[] Decompress(byte[] readBytes);
    }

    /// <summary>
    /// Utility class for compressing and decrompressing content using GZip compression.
    /// </summary>
    [Injectable(typeof(ICompressionUtil))]
    public class CompressionUtil : ICompressionUtil
    {
        private ILogger log;

        /// <summary>
        /// Post construct method for initializing the logger.
        /// </summary>
        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<CompressionUtil>();
        }

        /// <include file='../docs.xml' path='docs/members[@name="CompressionUtil"]/Compress/*' />
        public byte[] Compress(byte[] fileBytes)
        {
            log.Debug("Compressing [{0}] bytes", fileBytes.Length);
            using (var msi = new MemoryStream(fileBytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        /// <include file='../docs.xml' path='docs/members[@name="CompressionUtil"]/Decompress/*' />
        public byte[] Decompress(byte[] readBytes)
        {
            log.Debug("Decompressing [{0}] bytes.", readBytes.Length);
            using (var msi = new MemoryStream(readBytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }

        private void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[2048];
            int read = 0;
            while ((read = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, read);
            }
        }
    }
}