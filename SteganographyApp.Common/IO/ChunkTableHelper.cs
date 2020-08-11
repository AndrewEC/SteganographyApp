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

    [Injectable(typeof(IChunkTableHelper))]
    public class ChunkTableHelper : IChunkTableHelper
    {

        private static readonly int ChunkSizeAndPadding = Calculator.ChunkDefinitionBitSize + 1;

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