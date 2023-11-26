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

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DeleteAfterConversion/*' />
        bool DeleteAfterConversion { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        int ChunkByteSize { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ImageFormat/*' />
        ImageFormat ImageFormat { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/AdditionalPasswordHashIterations/*' />
        int AdditionalPasswordHashIterations { get; }

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/BitsToUse/*' />
        int BitsToUse { get; }
    }

    /// <summary>
    /// Convert a given set of user provided arguments into an IInputArguments instance containing a collection
    /// of the arguments required throughout various parts of the application.
    /// </summary>
    public interface IArgumentConverter
    {
        /// <summary>
        /// Convert a given set of user provided arguments into an IInputArguments instance containing a collection
        /// of the arguments required throughout various parts of the application.
        /// </summary>
        /// <returns>A fully formed IInputArguments instance containing definitions for the common arguments
        /// use throughout the application.</returns>
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
        public string RandomSeed { get; set; } = string.Empty;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DummyCount/*' />
        public int DummyCount { get; set; } = 0;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/DeleteAfterConversion/*' />
        public bool DeleteAfterConversion { get; set; } = false;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ChunkByteSize/*' />
        public int ChunkByteSize { get; set; } = 131_072;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/ImageFormat/*' />
        public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/AdditionalPasswordHashIterations/*' />
        public int AdditionalPasswordHashIterations { get; set; } = 0;

        /// <include file='../../docs.xml' path='docs/members[@name="InputArguments"]/BitsToUse/*' />
        public int BitsToUse { get; set; } = 1;
    }

    /// <summary>
    /// The exceptio thrown when an error occurs while trying to parse a command line argument value.
    /// </summary>
    /// <remarks>
    /// Initializes the exception instance.
    /// </remarks>
    /// <param name="message">The message to initialize the exception with.</param>
    public sealed class ArgumentValueException(string message) : Exception(message) { }
}