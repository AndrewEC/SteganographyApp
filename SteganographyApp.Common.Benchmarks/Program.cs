namespace SteganographyApp.Common.Benchmarks;

using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<EncodeBench>();
        BenchmarkRunner.Run<DecodeBench>();
    }
}