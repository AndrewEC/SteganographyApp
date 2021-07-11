namespace SteganographyApp.Common.Injection
{
    using System.IO;

    /// <summary>
    /// Provides an interface wrapper for some of the basic file IO operations
    /// so they can be stubbed out in unit tests.
    /// </summary>
    public interface IFileProvider
    {
        long GetFileSizeBytes(string pathToFile);

        bool IsExistingFile(string pathToFile);

        string[] GetFiles(string pathToDirectory);

        IReadWriteStream OpenFileForRead(string pathToFile);

        IReadWriteStream OpenFileForWrite(string pathToFile);

        void Delete(string pathToFile);

        string[] ReadAllLines(string pathToFile);
    }

    [Injectable(typeof(IFileProvider))]
    public class FileProvider : IFileProvider
    {
        public long GetFileSizeBytes(string pathToFile)
        {
            return new FileInfo(pathToFile).Length;
        }

        public bool IsExistingFile(string pathToFile)
        {
            return File.Exists(pathToFile) && !File.GetAttributes(pathToFile).HasFlag(FileAttributes.Directory);
        }

        public string[] GetFiles(string pathToDirectory)
        {
            return Directory.GetFiles(pathToDirectory);
        }

        public IReadWriteStream OpenFileForRead(string pathToFile)
        {
            return ReadWriteStream.CreateStreamForRead(pathToFile);
        }

        public IReadWriteStream OpenFileForWrite(string pathToFile)
        {
            return ReadWriteStream.CreateStreamForWrite(pathToFile);
        }

        public void Delete(string pathToFile)
        {
            File.Delete(pathToFile);
        }

        public string[] ReadAllLines(string pathToFile)
        {
            return File.ReadAllLines(pathToFile);
        }
    }
}