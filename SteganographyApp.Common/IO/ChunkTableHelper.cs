namespace SteganographyApp.Common.IO
{
    using System;
    using System.Linq;
    using System.Text;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;
    using SteganographyApp.Common.Logging;

    /// <summary>
    /// The interface for interacting with the ChunkTableHelper.
    /// </summary>
    public interface IChunkTableHelper
    {
        /// <include file='../docs.xml' path='docs/members[@name="ChunkTableHelper"]/ConvertChunkTableToBinary/*' />
        string ConvertChunkTableToBinary(int[] chunkLengths, string randomSeed);

        /// <include file='../docs.xml' path='docs/members[@name="ChunkTableHelper"]/ConvertBinaryToChunkTable/*' />
        int[] ConvertBinaryToChunkTable(string binary, int chunkCount, string randomSeed);
    }

    /// <summary>
    /// Utility class to help read and write a potentially randomized content chunk table.
    /// </summary>
    [Injectable(typeof(IChunkTableHelper))]
    public class ChunkTableHelper : IChunkTableHelper
    {
        private ILogger log;

        /// <summary>
        /// Post construct method to initialize the logger.
        /// </summary>
        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<ChunkTableHelper>();
        }

        /// <include file='../docs.xml' path='docs/members[@name="ChunkTableHelper"]/ConvertChunkTableToBinary/*' />
        public string ConvertChunkTableToBinary(int[] chunkLengths, string randomSeed)
        {
            log.Debug("Converting [{0}] table entries to binary using random seed [{1}]", chunkLengths.Length, randomSeed);
            var tableHeader = To33BitBinaryString(chunkLengths.Length);
            var binary = new StringBuilder();
            foreach (int chunkLength in chunkLengths)
            {
                binary.Append(To33BitBinaryString(chunkLength));
            }

            var binaryString = binary.ToString();

            if (!Checks.IsNullOrEmpty(randomSeed))
            {
                binaryString = Injector.Provide<IRandomizeUtil>().RandomizeBinaryString(binaryString, randomSeed);
            }

            return tableHeader + binaryString;
        }

        /// <include file='../docs.xml' path='docs/members[@name="ChunkTableHelper"]/ConvertBinaryToChunkTable/*' />
        public int[] ConvertBinaryToChunkTable(string binary, int chunkCount, string randomSeed)
        {
            log.Debug("Converting binary to chunk table with count of [{0}] and random seed [{1}]", chunkCount, randomSeed);
            var binaryString = binary;
            if (!Checks.IsNullOrEmpty(randomSeed))
            {
                binaryString = Injector.Provide<IRandomizeUtil>().ReorderBinaryString(binaryString, randomSeed);
            }

            return Enumerable.Range(0, chunkCount)
                .Select(i => NextBinaryChunk(i, binaryString))
                .Select(BinaryStringToInt)
                .ToArray();
        }

        private string NextBinaryChunk(int index, string binaryString) => binaryString.Substring(index * Calculator.ChunkDefinitionBitSizeWithPadding, Calculator.ChunkDefinitionBitSize);

        private int BinaryStringToInt(string binary) => Convert.ToInt32(binary, 2);

        private string To33BitBinaryString(int value) => Convert.ToString(value, 2).PadLeft(Calculator.ChunkDefinitionBitSize, '0') + "0";
    }
}