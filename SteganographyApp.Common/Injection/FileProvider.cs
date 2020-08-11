using System;
using System.IO;

namespace SteganographyApp.Common.Injection
{

    /// <summary>
    /// Provides an interface wrapper for some of the basic
    /// read/write stream operations.
    /// </summary>
    public interface IReadWriteStream : IDisposable
    {
        int Read(byte[] array, int offset, int count);
        void Write(byte[] array, int offset, int count);
        void Flush();
    }

    public class ReadWriteStream : IReadWriteStream
    {

        private readonly FileStream stream;

        private ReadWriteStream(FileStream stream)
        {
            this.stream = stream;
        }

        public static ReadWriteStream CreateStreamForRead(string pathToFile)
        {
            var stream = File.OpenRead(pathToFile);
            return new ReadWriteStream(stream);
        }

        public static ReadWriteStream CreateStreamForWrite(string pathToFile)
        {
            var stream = File.Open(pathToFile, FileMode.OpenOrCreate);
            return new ReadWriteStream(stream);
        }

        public int Read(byte[] array, int offset, int count)
        {
            return stream.Read(array, offset, count);
        }

        public void Write(byte[] array, int offset, int count)
        {
            stream.Write(array, offset, count);
        }

        public void Flush()
        {
            stream.Flush();
        }

        public void Dispose()
        {
            stream.Dispose();
        }

    }

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