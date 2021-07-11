namespace SteganographyApp.Common.IO
{
    using System;
    using System.Linq;
    using System.Text;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.Injection;

    public interface IChunkTableHelper
    {
        string ConvertChunkTableToBinary(int[] chunkLengths, string randomSeed);

        int[] ConvertBinaryToChunkTable(string binary, int chunkCount, string randomSeed);
    }

    /// <summary>
    /// Utility class to help read and write a potentially randomized content chunk table.
    /// </summary>
    [Injectable(typeof(IChunkTableHelper))]
    public class ChunkTableHelper : IChunkTableHelper
    {
        private ILogger log;

        [PostConstruct]
        public void PostConstruct()
        {
            log = Injector.LoggerFor<ChunkTableHelper>();
        }

        /// <summary>
        /// Converts the lengths of all the chunks in the table to a binary string and,
        /// if the randomSeed is not null or blank, ranmizes that binary string.
        /// </summary>
        /// <param name="chunkLengths">The array of chunk lengths to write.</param>
        /// <param name="randomSeed">The seed to ranomize the binary string with.</param>
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

        /// <summary>
        /// Converts the raw binary string containing the content chunk table and converts it to an array
        /// of integers representing the lenghts of each entry in the chunk table.
        /// </summary>
        /// <param name="binary">The binary string containing the content chunk table to un-randomize</param>
        /// <param name="chunkCount">The number of expected chunks in the content chunk table.</param>
        /// <param name="randomSeed">The random seed required to re-order the binary string.</param>
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