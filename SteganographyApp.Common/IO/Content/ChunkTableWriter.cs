namespace SteganographyApp.Common.IO
{
    using System;
    using System.Collections.Immutable;
    using System.Text;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// Responsible for writing the content chunk table to the cover images.
    /// </summary>
    public sealed class ChunkTableWriter : AbstractChunkTableIO
    {
        private readonly ILogger log = new LazyLogger<ChunkTableWriter>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="store">The image store instance.</param>
        /// <param name="arguments">The user provided arguments.</param>
        public ChunkTableWriter(ImageStore store, IInputArguments arguments) : base(store, arguments) { }

        /// <summary>
        /// Writes the content chunk table to the cover images starting from the first.
        /// </summary>
        /// <param name="chunkLengths">The list of chunks lengths to be written to the cover images.</param>
        public void WriteContentChunkTable(ImmutableArray<int> chunkLengths)
        {
            log.Debug("Saving content chunk table with [{0}] chunks", chunkLengths.Length);
            string tableHeader = To18BitBinaryString((short)chunkLengths.Length);

            var binary = new StringBuilder();
            foreach (int chunkLength in chunkLengths)
            {
                binary.Append(To33BitBinaryString(chunkLength));
            }

            var binaryString = binary.ToString();

            if (!string.IsNullOrEmpty(Arguments.RandomSeed))
            {
                var randomize = Injector.Provide<IRandomizeUtil>();
                log.Trace("Randomizing chunk table header.");
                tableHeader = randomize.RandomizeBinaryString(tableHeader, Arguments.RandomSeed);
                log.Trace("Randomizing remaining chunk table content.");
                binaryString = randomize.RandomizeBinaryString(binaryString, Arguments.RandomSeed);
            }

            string tableBinary = tableHeader + binaryString;

            ImageStoreIO.WriteContentChunkToImage(tableBinary);
            ImageStoreIO.EncodeComplete();
        }

        private string To33BitBinaryString(int value) => Convert.ToString(value, 2).PadLeft(Calculator.ChunkDefinitionBitSizeWithPadding, '0');

        private string To18BitBinaryString(short value) => Convert.ToString(value, 2).PadLeft(Calculator.ChunkTableHeaderSizeWithPadding, '0');
    }
}