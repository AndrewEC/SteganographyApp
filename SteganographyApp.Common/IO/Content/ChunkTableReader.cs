namespace SteganographyApp.Common.IO
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Responsible for reading the content chunk table from the leading cover image.
    /// </summary>
    public class ChunkTableReader : AbstractChunkTableIO
    {
        private readonly ILogger log = new LazyLogger<ChunkTableReader>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="store">The image store instance.</param>
        /// <param name="arguments">The user provided arguments.</param>
        public ChunkTableReader(ImageStore store, IInputArguments arguments) : base(store, arguments) { }

        /// <summary>
        /// Reads in and returns an array in which each element represents the number of bits in a chunk.
        /// </summary>
        /// <returns>An immutable array in whcih each element specifies the number of bits per chunk saved in the
        /// cover images.</returns>
        public ImmutableArray<int> ReadContentChunkTable()
        {
            log.Trace("Reading content chunk table.");
            var randomizeUtil = Injector.Provide<IRandomizeUtil>();

            int chunkCount = ReadChunkCount(randomizeUtil);
            log.Debug("Chunk table contains [{0}] chunks.", chunkCount);

            return ReadTableChunkLengths(randomizeUtil, chunkCount);
        }

        private int ReadChunkCount(IRandomizeUtil randomizeUtil)
        {
            log.Trace("Reading chunk table header.");
            string headerBinary = ImageStoreIO.ReadContentChunkFromImage(Calculator.ChunkDefinitionBitSizeWithPadding);
            if (!string.IsNullOrEmpty(Arguments.RandomSeed))
            {
                headerBinary = randomizeUtil.ReorderBinaryString(headerBinary, Arguments.RandomSeed);
            }
            return Convert.ToInt32(headerBinary, 2);
        }

        private ImmutableArray<int> ReadTableChunkLengths(IRandomizeUtil randomizeUtil, int chunkCount)
        {
            log.Trace("Reading content of chunk table.");
            int chunkSize = Calculator.ChunkDefinitionBitSizeWithPadding * chunkCount;
            string tableBinary = ImageStoreIO.ReadContentChunkFromImage(chunkSize);
            if (!string.IsNullOrEmpty(Arguments.RandomSeed))
            {
                tableBinary = randomizeUtil.ReorderBinaryString(tableBinary, Arguments.RandomSeed);
            }

            return Enumerable.Range(0, chunkCount)
                .Select(i => NextBinaryChunk(i, tableBinary))
                .Select(BinaryStringToInt)
                .ToImmutableArray();
        }

        private string NextBinaryChunk(int index, string binaryString) => binaryString.Substring(index * Calculator.ChunkDefinitionBitSizeWithPadding, Calculator.ChunkDefinitionBitSizeWithPadding);

        private int BinaryStringToInt(string binary) => Convert.ToInt32(binary, 2);
    }
}