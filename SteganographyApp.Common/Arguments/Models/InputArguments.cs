namespace SteganographyApp.Common.Arguments
{
    using System.Collections.Immutable;

    using SixLabors.ImageSharp.Formats.Png;

    /// <summary>
    /// Generic interface containing all the getter methods required for retrieving
    /// all the parsed input argument values.
    /// </summary>
    public interface IInputArguments
    {
        string Password { get; }

        string FileToEncode { get; }

        string DecodedOutputFile { get; }

        ImmutableArray<string> CoverImages { get; }

        ActionEnum EncodeOrDecode { get; }

        bool PrintStack { get; }

        bool UseCompression { get; }

        string RandomSeed { get; }

        int DummyCount { get; }

        bool InsertDummies { get; }

        bool DeleteAfterConversion { get; }

        PngCompressionLevel CompressionLevel { get; }

        int ChunkByteSize { get; }
    }

    public interface IImmutableFactory<T>
    {
        T ToImmutable();
    }

    /// <summary>
    /// Contains a number of properties that will contain values parsed from the user provided command line arguments.
    /// </summary>
    public sealed class InputArguments : IInputArguments, IImmutableFactory<IInputArguments>
    {
        public string Password { get; set; } = string.Empty;

        public string FileToEncode { get; set; } = string.Empty;

        public string DecodedOutputFile { get; set; } = string.Empty;

        public ImmutableArray<string> CoverImages { get; set; }

        public ActionEnum EncodeOrDecode { get; set; }

        public bool PrintStack { get; set; } = false;

        public bool UseCompression { get; set; } = false;

        public string RandomSeed { get; set; } = string.Empty;

        public int DummyCount { get; set; } = 0;

        public bool InsertDummies { get; set; } = false;

        public bool DeleteAfterConversion { get; set; } = false;

        public PngCompressionLevel CompressionLevel { get; set; } = PngCompressionLevel.Level5;

        /// <summary>
        /// Specifies the chunk size. I.e. the number of bytes to read, encode,
        /// and write at any given time.
        /// <para>Higher values will improve the time to encode a larger file and reduce
        /// ther overall encoded file size though values too high run the risk of
        /// having memory related errors.</para>
        /// <para>Default value of 131,072</para>
        /// </summary>
        public int ChunkByteSize { get; set; } = 131_072;

        public IInputArguments ToImmutable()
        {
            return new ImmutableInputArguments(this);
        }
    }

    /// <summary>
    /// Immutable class containg the parsed argument values.
    /// <para>Parsed arguments are made immutable to prevent any accidental mutations.</para>
    /// </summary>
    public sealed class ImmutableInputArguments : IInputArguments
    {
        public ImmutableInputArguments(InputArguments source)
        {
            Password = source.Password;
            FileToEncode = source.FileToEncode;
            DecodedOutputFile = source.DecodedOutputFile;
            CoverImages = source.CoverImages;
            EncodeOrDecode = source.EncodeOrDecode;
            PrintStack = source.PrintStack;
            UseCompression = source.UseCompression;
            RandomSeed = source.RandomSeed;
            DummyCount = source.DummyCount;
            InsertDummies = source.InsertDummies;
            DeleteAfterConversion = source.DeleteAfterConversion;
            CompressionLevel = source.CompressionLevel;
            ChunkByteSize = source.ChunkByteSize;
        }

        public string Password { get; }

        public string FileToEncode { get; }

        public string DecodedOutputFile { get; }

        public ImmutableArray<string> CoverImages { get; }

        public ActionEnum EncodeOrDecode { get; }

        public bool PrintStack { get; }

        public bool UseCompression { get; }

        public string RandomSeed { get; }

        public int DummyCount { get; }

        public bool InsertDummies { get; }

        public bool DeleteAfterConversion { get; }

        public PngCompressionLevel CompressionLevel { get; }

        public int ChunkByteSize { get; }

        public bool EnableLogs { get; }
    }
}