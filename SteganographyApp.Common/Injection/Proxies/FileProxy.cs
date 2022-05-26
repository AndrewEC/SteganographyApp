namespace SteganographyApp.Common.Injection
{
    using System.IO;

    /// <summary>
    /// A proxy interface to interacting with some of the common file IO related functions.
    /// </summary>
    public interface IFileIOProxy
    {
        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/GetFileSizeBytes/*' />
        long GetFileSizeBytes(string pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/IsExistingFile/*' />
        bool IsExistingFile(string pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/GetFiles/*' />
        string[] GetFiles(string pathToDirectory);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/OpenFileForRead/*' />
        IReadWriteStream OpenFileForRead(string pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/OpenFileForWrite/*' />
        IReadWriteStream OpenFileForWrite(string pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/Delete/*' />
        void Delete(string pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/ReadAllLines/*' />
        string[] ReadAllLines(string pathToFile);
    }

    /// <summary>
    /// Concrete implementation of the file IO proxy. Used to proxy calls to static IO based methods
    /// so that such calls can be better mocked in unit tests.
    /// </summary>
    [Injectable(typeof(IFileIOProxy))]
    public class FileIOProxy : IFileIOProxy
    {
        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/GetFileSizeBytes/*' />
        public long GetFileSizeBytes(string pathToFile) => new FileInfo(pathToFile).Length;

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/IsExistingFile/*' />
        public bool IsExistingFile(string pathToFile) => File.Exists(pathToFile) && !File.GetAttributes(pathToFile).HasFlag(FileAttributes.Directory);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/GetFiles/*' />
        public string[] GetFiles(string pathToDirectory) => Directory.GetFiles(pathToDirectory);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/OpenFileForRead/*' />
        public IReadWriteStream OpenFileForRead(string pathToFile) => ReadWriteStream.CreateStreamForRead(pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/OpenFileForWrite/*' />
        public IReadWriteStream OpenFileForWrite(string pathToFile) => ReadWriteStream.CreateStreamForWrite(pathToFile);

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/Delete/*' />
        public void Delete(string pathToFile)
        {
            File.Delete(pathToFile);
        }

        /// <include file='../../docs.xml' path='docs/members[@name="FileIOProxy"]/ReadAllLines/*' />
        public string[] ReadAllLines(string pathToFile) => File.ReadAllLines(pathToFile);
    }
}