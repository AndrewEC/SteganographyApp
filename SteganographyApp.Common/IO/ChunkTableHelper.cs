using System;
using System.Text;
using System.Linq;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.IO
{

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

        private static readonly int ChunkSizeAndPadding = Calculator.ChunkDefinitionBitSize + 1;

        /// <summary>
        /// Converts the lengths of all the chunks in the table to a binary string and,
        /// if the randomSeed is not null or blank, ranmizes that binary string.
        /// </summary>
        /// <param name="chunkLengths">The array of chunk lengths to write.</param>
        /// <param name="randomSeed">The seed to ranomize the binary string with.</param>
        public string ConvertChunkTableToBinary(int[] chunkLengths, string randomSeed)
        {
            var tableHeader = to33BitBinaryString(chunkLengths.Length);
            var binary = new StringBuilder();
            foreach (int chunkLength in chunkLengths)
            {
                binary.Append(to33BitBinaryString(chunkLength));
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

            var binaryString = binary;
            if (!Checks.IsNullOrEmpty(randomSeed))
            {
                binaryString = Injector.Provide<IRandomizeUtil>().ReorderBinaryString(binaryString, randomSeed);
            }

            return Enumerable.Range(0, chunkCount)
                .Select(i => binaryString.Substring(i * ChunkSizeAndPadding, Calculator.ChunkDefinitionBitSize))
                .Select(BinaryStringToInt)
                .ToArray();
        }

        private int BinaryStringToInt(string binary)
        {
            return Convert.ToInt32(binary, 2);
        }

        private string to33BitBinaryString(int value)
        {
            return Convert.ToString(value, 2).PadLeft(Calculator.ChunkDefinitionBitSize, '0') + "0";
        }

    }

}