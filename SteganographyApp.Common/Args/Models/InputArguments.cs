namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Collections.Immutable;

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

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        int ChunkByteSize { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ImageFormat/*' />
        public ImageFormat ImageFormat { get; }
    }

    public interface IArgumentConverter
    {
        IInputArguments ToCommonArguments();
    }

    /// <summary>
    /// Immutable class containg the parsed argument values. All of the properties within this class
    /// are immutable and readonly.
    /// </summary>
    public sealed class CommonArguments : IInputArguments
    {
        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/Password/*' />
        public string Password { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/FileToEncode/*' />
        public string FileToEncode { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DecodedOutputFile/*' />
        public string DecodedOutputFile { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/CoverImages/*' />
        public ImmutableArray<string> CoverImages { get; set; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/UseCompression/*' />
        public bool UseCompression { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/RandomSeed/*' />
        public string RandomSeed { get; set; } = "";

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DummyCount/*' />
        public int DummyCount { get; set; } = 0;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/InsertDummies/*' />
        public bool InsertDummies { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DeleteAfterConversion/*' />
        public bool DeleteAfterConversion { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        public int ChunkByteSize { get; set; } = 131_072;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ImageFormat/*' />
        public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;
    }

    public sealed class ArgumentValueException : Exception
    {
        public ArgumentValueException(string message) : base(message) { }
    }
}