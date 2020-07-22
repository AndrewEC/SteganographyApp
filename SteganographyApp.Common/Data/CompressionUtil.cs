using System.IO;
using System.IO.Compression;

namespace SteganographyApp.Common.Data
{

    public interface ICompressionUtil
    {
        byte[] Compress(byte[] fileBytes);
        byte[] Decompress(byte[] readBytes);
    }

    public class CompressionUtil : ICompressionUtil
    {

        /// <summary>
        /// Copies all the data from the source stream into the destination stream
        /// </summary>
        /// <param name="src">The source stream where the data is coming from.</param>
        /// <param name="dest">The destination stream the data is being written to.</param>
        private void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[2048];
            int read = 0;
            while ((read = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, read);
            }
        }

        /// <summary>
        /// Compresses the raw file bytes using standard gzip compression.
        /// </summary>
        /// <param name="fileBytes">The array of bytes read from the input file.</param>
        /// <returns>The gzip compressed array of bytes.</returns>
        public byte[] Compress(byte[] fileBytes)
        {
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

        /// <summary>
        /// Decompresses the bytes read and decoded from the cover image(s) using standard
        /// gzip compression.
        /// </summary>
        /// <param name="readBytes">The array of bytes read and decoded from the
        /// cover images.</param>
        /// <returns>A byte array after being decompressed using standard gzip compression.</returns>
        public byte[] Decompress(byte[] readBytes)
        {
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

    }

}