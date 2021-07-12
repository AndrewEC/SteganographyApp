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
        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/Password/*' />
        string Password { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/FileToEncode/*' />
        string FileToEncode { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DecodedOutputFile/*' />
        string DecodedOutputFile { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CoverImages/*' />
        ImmutableArray<string> CoverImages { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/EncodeOrDecode/*' />
        ActionEnum EncodeOrDecode { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/PrintStack/*' />
        bool PrintStack { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/UseCompression/*' />
        bool UseCompression { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/RandomSeed/*' />
        string RandomSeed { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DummyCount/*' />
        int DummyCount { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/InsertDummies/*' />
        bool InsertDummies { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DeleteAfterConversion/*' />
        bool DeleteAfterConversion { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CompressionLevel/*' />
        PngCompressionLevel CompressionLevel { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        int ChunkByteSize { get; }
    }

    /// <summary>
    /// A contract for converting a concrete mutatble instance to an immutable representation of the same instance.
    /// </summary>
    /// <typeparam name="T">The immutable type that will be returned by the ToImmutable invocation.</typeparam>
    public interface IImmutableFactory<T>
    {
        /// <summary>
        /// Convert the underlying concrete instance to an immutable representation.
        /// </summary>
        /// <returns>A preferrably immutable instance that conforms to type T.</returns>
        T ToImmutable();
    }

    /// <summary>
    /// Contains a number of properties that will contain values parsed from the user provided command line arguments.
    /// Should not be used outside of the argument parser. Outside the argument parser the immutable version should be
    /// used instead.
    /// </summary>
    /// <see cref="ImmutableInputArguments"></see>
    public sealed class InputArguments : IInputArguments, IImmutableFactory<IInputArguments>
    {
        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/Password/*' />
        public string Password { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/FileToEncode/*' />
        public string FileToEncode { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DecodedOutputFile/*' />
        public string DecodedOutputFile { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CoverImages/*' />
        public ImmutableArray<string> CoverImages { get; set; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/EncodeOrDecode/*' />
        public ActionEnum EncodeOrDecode { get; set; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/PrintStack/*' />
        public bool PrintStack { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/UseCompression/*' />
        public bool UseCompression { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/RandomSeed/*' />
        public string RandomSeed { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DummyCount/*' />
        public int DummyCount { get; set; } = 0;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/InsertDummies/*' />
        public bool InsertDummies { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DeleteAfterConversion/*' />
        public bool DeleteAfterConversion { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CompressionLevel/*' />
        public PngCompressionLevel CompressionLevel { get; set; } = PngCompressionLevel.Level5;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        public int ChunkByteSize { get; set; } = 131_072;

        /// <summary>
        /// Converts the current InputArguments instance into an ImmutableInputArguments instance with
        /// readonly properties.
        /// </summary>
        /// <returns>An ImmutableInputArguments instance created using this InputArguments instance as input.</returns>
        public IInputArguments ToImmutable()
        {
            return new ImmutableInputArguments(this);
        }
    }

    /// <summary>
    /// Immutable class containg the parsed argument values. All of the properties within this class
    /// are immutable and readonly.
    /// </summary>
    public sealed class ImmutableInputArguments : IInputArguments
    {
        /// <summary>
        /// Initializes an ImmutableInputArguments instance using the values pulled from the given
        /// InputArguments instance.
        /// </summary>
        /// <param name="source">The InputArguments instance from which all properties will be set from.</param>
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

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/Password/*' />
        public string Password { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/FileToEncode/*' />
        public string FileToEncode { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DecodedOutputFile/*' />
        public string DecodedOutputFile { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CoverImages/*' />
        public ImmutableArray<string> CoverImages { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/EncodeOrDecode/*' />
        public ActionEnum EncodeOrDecode { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/PrintStack/*' />
        public bool PrintStack { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/UseCompression/*' />
        public bool UseCompression { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/RandomSeed/*' />
        public string RandomSeed { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DummyCount/*' />
        public int DummyCount { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/InsertDummies/*' />
        public bool InsertDummies { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DeleteAfterConversion/*' />
        public bool DeleteAfterConversion { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CompressionLevel/*' />
        public PngCompressionLevel CompressionLevel { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        public int ChunkByteSize { get; }
    }
}