using System.IO;

namespace SteganographyApp.Common.Providers
{

    public interface IFileProvider
    {
        long GetFileSizeBytes(string pathToFile);
        bool IsExistingFile(string pathToFile);
        string[] GetFiles(string pathToDirectory);
    }

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

    }

}