using BenchmarkDotNet.Running;

namespace SteganographyApp.Common.Benchmarks
{

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<EncodeBench>();
            BenchmarkRunner.Run<DecodeBench>();
        }
    }

}