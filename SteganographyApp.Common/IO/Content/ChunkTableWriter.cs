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
    /// <param name="store">The image store instance.</param>
    /// <param name="arguments">The user provided arguments.</param>
    public sealed class ChunkTableWriter(ImageStore store, IInputArguments arguments) : AbstractChunkTableIO(store, arguments)
    {
        private readonly ILogger log = new LazyLogger<ChunkTableWriter>();

        /// <summary>
        /// Writes the content chunk table to the cover images starting from the first.
        /// </summary>
        /// <param name="chunkLengths">The list of chunks lengths to be written to the cover images.</param>
        public void WriteContentChunkTable(ImmutableArray<int> chunkLengths) => RunIfNotDisposed(() =>
        {
            log.Debug("Saving content chunk table with [{0}] chunks", chunkLengths.Length);
            string tableHeader = To18BitBinaryString((short)chunkLengths.Length);
            log.Debug("Chunk table header: [{0}]", tableHeader);

            var binary = new StringBuilder();
            foreach (int chunkLength in chunkLengths)
            {
                binary.Append(To33BitBinaryString(chunkLength));
            }

            var binaryString = binary.ToString();
            log.Debug("Chunk table binary: [{0}]", binaryString);

            if (!string.IsNullOrEmpty(Arguments.RandomSeed))
            {
                var randomizeUtil = Injector.Provide<IRandomizeUtil>();
                var binaryUtil = Injector.Provide<IBinaryUtil>();

                tableHeader = Randomize(randomizeUtil, binaryUtil, tableHeader);
                log.Debug("Randomized chunk table header to: [{0}]", tableHeader);

                binaryString = Randomize(randomizeUtil, binaryUtil, binaryString);
                log.Debug("Randomized remaining chunk table content to: [{0}]", binaryString);
            }

            string tableBinary = tableHeader + binaryString;

            ImageStoreIO.WriteContentChunkToImage(tableBinary);
            ImageStoreIO.EncodeComplete();
        });

        private string Randomize(IRandomizeUtil randomizeUtil, IBinaryUtil binaryUtil, string value)
        {
            byte[] valueBytes = binaryUtil.ToBytesDirect(value);
            byte[] randomized = randomizeUtil.Randomize(valueBytes, Arguments.RandomSeed, DummyCount, IterationMultiplier);
            return Injector.Provide<IBinaryUtil>().ToBinaryStringDirect(randomized);
        }

        private static string To33BitBinaryString(int value) => Convert.ToString(value, 2).PadLeft(Calculator.ChunkDefinitionBitSizeWithPadding, '0');

        private static string To18BitBinaryString(short value) => Convert.ToString(value, 2).PadLeft(Calculator.ChunkTableHeaderSizeWithPadding, '0');
    }
}