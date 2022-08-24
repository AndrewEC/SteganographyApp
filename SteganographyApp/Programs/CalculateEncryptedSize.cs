// <auto-generated/>
namespace SteganographyApp
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;

    using SteganographyApp.Common;
    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Arguments.Commands;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.IO;
    using SteganographyApp.Common.Logging;

    [ProgramDescriptor("Calculates the approximate size of an input file if it were to be encrypted.")]
    internal sealed class CalculateEncryptedSizeArguments : IArgumentConverter
    {
        [Argument("--coverImages", "-c", true, helpText: "The images where the input file will be encoded and written to.")]
        public ImmutableArray<string> CoverImages;

        [Argument("--password", "-p", helpText: "The optional password used to encrypt the input file contents.")]
        public string Password = string.Empty;

        [Argument("--file", "-f", true, helpText: "The path to the file to encode and write to the cover images.", parser: nameof(ParseFilePath))]
        public string InputFile = string.Empty;

        [Argument("--randomSeed", "-r", helpText: "The optional value to determine how the contents of the input file will be randomized before writing them.")]
        public string RandomSeed = string.Empty;

        [Argument("--insertDummies", "-i", helpText: "Choose whether dummy bytes should be inserted into the file contents before being randomized.")]
        public bool InsertDummies = false;

        [Argument("--chunkByteSize", "-cs", helpText: "The number of bytes to read and encode from the input file during each iteration.")]
        public int ChunkByteSize = 131_072;

        [Argument("--logLevel", "-l", helpText: "The log level to determine which logs will feed into the log file.")]
        public LogLevel LogLevel = LogLevel.None;

        public static object ParseFilePath(object? target, string value) => ParserFunctions.ParseFilePath(value);

        public IInputArguments ToCommonArguments()
        {
            RootLogger.Instance.EnableLoggingAtLevel(LogLevel);
            var arguments = new CommonArguments
            {
                CoverImages = CoverImages,
                Password = Password,
                FileToEncode = InputFile,
                RandomSeed = RandomSeed,
                InsertDummies = InsertDummies,
                ChunkByteSize = ChunkByteSize,
                DummyCount = ParserFunctions.ParseDummyCount(InsertDummies, CoverImages, RandomSeed)
            };
            Injector.LoggerFor<EncodeArguments>().Debug("Using input arguments: [{0}]", () => new[] { JsonSerializer.Serialize(arguments) });
            return arguments;
        }
    }

    internal sealed class CalculateEncryptedSizeCommand : BaseCommand<CalculateEncryptedSizeArguments>
    {
        public override string GetName() => "encrypted-size";

        public override void Execute(CalculateEncryptedSizeArguments args)
        {
            var arguments = args.ToCommonArguments();
            Console.WriteLine("Calculating encypted size of file {0}.", arguments.FileToEncode);
            try
            {
                int singleChunkSize = CalculateChunkLength(arguments);
                int numberOfChunks = Calculator.CalculateRequiredNumberOfWrites(arguments.FileToEncode, arguments.ChunkByteSize);

                // Plus 1 because we need an additional entry in the chunk table to indicate the number of entries in the table
                int chunkTableSize = Calculator.CalculateRequiredBitsForContentTable(numberOfChunks);
                double size = ((double)singleChunkSize * (double)numberOfChunks) + (double)chunkTableSize;

                Console.WriteLine("\nEncrypted file size is:");
                PrintSize(size);

                Console.WriteLine("\n# of images required to store this file at common resolutions:");
                PrintComparison(size);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while encoding file: {0}", e.Message);
            }
        }

        /// <summary>
        /// Read in a chunk of the file to encode and pass the read in bytes to the
        /// encoder util.
        /// </summary>
        /// <param name="chunkId">Used to specify where the thread will start and stop reading.
        /// The thread will use the equation chunkId * arguments.ChunkByteSize
        /// to determine where to start reading the file to encode from.</param>
        private static int CalculateChunkLength(IInputArguments arguments)
        {
            using (var contentReader = new ContentReader(arguments))
            {
                return contentReader.ReadContentChunkFromFile()!.Length;
            }
        }

        /// <summary>
        /// Prints out the binary size of an encypted file or storage space in bits, bytes,
        /// megabytes and gigabytes.
        /// </summary>
        /// <param name="size">The size in bits to print out.</param>
        private static void PrintSize(double size)
        {
            Console.WriteLine("\t{0} bits", size);
            Console.WriteLine("\t{0} bytes", size / 8);
            Console.WriteLine("\t{0} KB", size / 8 / 1024);
            Console.WriteLine("\t{0} MB", size / 8 / 1024 / 1024);
        }

        /// <summary>
        /// Prints how many images at common resolutions it would take to store this content.
        /// </summary>
        /// <param name="size">The size of the encoded file in bits.</param>
        private static void PrintComparison(double size)
        {
            Console.WriteLine("\tAt 360p: \t{0}", size / CommonResolutionStorageSpace.P360);
            Console.WriteLine("\tAt 480p: \t{0}", size / CommonResolutionStorageSpace.P480);
            Console.WriteLine("\tAt 720p: \t{0}", size / CommonResolutionStorageSpace.P720);
            Console.WriteLine("\tAt 1080p: \t{0}", size / CommonResolutionStorageSpace.P1080);
            Console.WriteLine("\tAt 1440p: \t{0}", size / CommonResolutionStorageSpace.P1440);
            Console.WriteLine("\tAt 4K (2160p): \t{0}", size / CommonResolutionStorageSpace.P2160);
        }
    }
}