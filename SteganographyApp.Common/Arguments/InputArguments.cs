using System;

namespace SteganographyApp.Common.Arguments
{

    /// <summary>
    /// Specifies that an exception occured while trying to read and parse the command line arguments
    /// or that certain required arguments were not present.
    /// </summary>
    public class ArgumentParseException : Exception
    {
        public ArgumentParseException(string message) : base(message) { }
        public ArgumentParseException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Specifies that a value provided for a particular argument was not valid or could not be properly
    /// parsed into the required data type.
    /// </summary>
    public class ArgumentValueException : Exception
    {
        public ArgumentValueException(string message) : base(message) { }
        public ArgumentValueException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Takes in a value retrieved from an associated key, parses it, and sets the
    /// relevant property value in the InputArguments instance
    /// </summary>
    /// <param name="args">The InputArguments param to modify.</param>
    /// <param name="value">The value of the key/value pair from the array of arguments.</param>
    public delegate void ValueParser(InputArguments args, string value);

    /// <summary>
    /// Takes in the collected set of argument/value pairs and performs a final validation
    /// on them.
    /// <para>If the string returned is neither null or empty than then the validation is treated
    /// as a failure.</para>
    /// </summary>
    /// <param name="args">The InputArguments and all their associated values.</param>
    public delegate string PostValidation(IInputArguments args);

    /// <summary>
    /// Encapsulates information about an argument that the user can specify when invoking the
    /// tool.
    /// </summary>
    public sealed class Argument
    {
        public string Name { get; private set; }
        public string ShortName { get; private set; }
        public ValueParser Parser { get; private set; }
        public bool IsFlag { get; private set; }

        public Argument(string name, string shortName, ValueParser parser, bool flag=false)
        {
            Name = name;
            ShortName = shortName;
            Parser = parser;
            IsFlag = flag;
        }
    }

    /// <summary>
    /// Enum specifying whether the program is to attempt to proceed with encoding a file
    /// or decoding to an output file.
    /// </summary>
    public enum ActionEnum
    {
        Encode,
        Decode,
        Clean,
        CalculateStorageSpace,
        CalculateEncryptedSize,
        Convert
    }

    /// <summary>
    /// Contains a number of properties that will contain values parsed from the user provided command line arguments.
    /// </summary>
    public sealed class InputArguments : IInputArguments, IImmutableFactory<IInputArguments>
    {
        public string Password { get; set; } = "";
        public string FileToEncode { get; set; } = "";
        public string DecodedOutputFile { get; set; } = "";
        public string[] CoverImages { get; set; }
        public ActionEnum EncodeOrDecode { get; set; }
        public bool PrintStack { get; set; } = false;
        public bool UseCompression { get; set; } = false;
        public string RandomSeed { get; set; } = "";
        public int DummyCount { get; set; } = 0;
        public bool InsertDummies { get; set; } = false;
        public bool DeleteAfterConversion { get; set; } = false;
        public int CompressionLevel { get; set; } = 6;

        /// <summary>
        /// Specifies the chunk size. I.e. the number of bytes to read, encode,
        /// and write at any given time.
        /// <para>Higher values will improve the time to encode a larger file and reduce
        /// ther overall encoded file size though values too high run the risk of
        /// having memory related errors.</para>
        /// <para>Value of 131,072</para>
        /// </summary>
        public int ChunkByteSize { get; set; } = 131_072;

        public IInputArguments ToImmutable()
        {
            return new ImmutableInputArguments(Password, FileToEncode, DecodedOutputFile, CoverImages, EncodeOrDecode, PrintStack,
                UseCompression, RandomSeed, DummyCount, InsertDummies, DeleteAfterConversion, CompressionLevel, ChunkByteSize);
        }
    }

    /// <summary>
    /// Immutable class containg the parsed argument values.
    /// <para>Parsed arguments are made immutable to prevent any accidental mutations.</para>
    /// </summary>
    public sealed class ImmutableInputArguments : IInputArguments
    {

        public ImmutableInputArguments(string password, string fileToEncode, string decodedOutputFile, string[] coverImages,
            ActionEnum encodeOrDecode, bool printStack, bool useCompression, string randomSeed, int dummyCount,
            bool insertDummies, bool deleteAfterConversion, int compressionLevel, int chunkByteSize)
        {
            Password = password;
            FileToEncode = fileToEncode;
            DecodedOutputFile = decodedOutputFile;
            CoverImages = coverImages;
            EncodeOrDecode = encodeOrDecode;
            PrintStack = printStack;
            UseCompression = useCompression;
            RandomSeed = randomSeed;
            DummyCount = dummyCount;
            InsertDummies = insertDummies;
            DeleteAfterConversion = deleteAfterConversion;
            CompressionLevel = compressionLevel;
            ChunkByteSize = chunkByteSize;
        }

        public string Password { get; }
        public string FileToEncode { get; }
        public string DecodedOutputFile { get; }
        public string[] CoverImages { get; }
        public ActionEnum EncodeOrDecode { get; }
        public bool PrintStack { get; }
        public bool UseCompression { get; }
        public string RandomSeed { get; }
        public int DummyCount { get; }
        public bool InsertDummies { get; }
        public bool DeleteAfterConversion { get; }
        public int CompressionLevel { get; }
        public int ChunkByteSize { get; }
    }

    /// <summary>
    /// Generic interface containing all the getter methods required for retrieving
    /// all the parsed input argument values.
    /// </summary>
    public interface IInputArguments
    {
        string Password { get; }
        string FileToEncode { get; }
        string DecodedOutputFile { get; }
        string[] CoverImages { get; }
        ActionEnum EncodeOrDecode { get; }
        bool PrintStack { get; }
        bool UseCompression { get; }
        string RandomSeed { get; }
        int DummyCount { get; }
        bool InsertDummies { get; }
        bool DeleteAfterConversion { get; }
        int CompressionLevel { get; }
        int ChunkByteSize { get; }
    }

    public interface IImmutableFactory<T>
    {
        T ToImmutable();
    }

}