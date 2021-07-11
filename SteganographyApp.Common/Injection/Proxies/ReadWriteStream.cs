namespace SteganographyApp.Common.Injection
{
    using System;
    using System.IO;

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

        public static ReadWriteStream CreateStreamForRead(string pathToFile) => new ReadWriteStream(File.OpenRead(pathToFile));

        public static ReadWriteStream CreateStreamForWrite(string pathToFile) => new ReadWriteStream(File.Open(pathToFile, FileMode.OpenOrCreate));

        public int Read(byte[] array, int offset, int count) => stream.Read(array, offset, count);

        public void Write(byte[] array, int offset, int count) => stream.Write(array, offset, count);

        public void Flush() => stream.Flush();

        public void Dispose() => stream.Dispose();
    }
}