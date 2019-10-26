using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Benchmarks
{

    [SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 10, targetCount: 10)]
    public class EncodeBench
    {

        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[4_000_000];
            new Random(100).NextBytes(data);
        }

        [Benchmark]
        public string Encode()
        {
            return DataEncoderUtil.Encode(data, "", false, 0);
        }

        [Benchmark]
        public string EncodeWithPassword()
        {
            return DataEncoderUtil.Encode(data, "Password123!@#", false, 0);
        }

        [Benchmark]
        public string EncodeWithCompression()
        {
            return DataEncoderUtil.Encode(data, "", true, 0);
        }

        [Benchmark]
        public string EncodeWithDummies()
        {
            return DataEncoderUtil.Encode(data, "", false, 10);
        }

        [Benchmark]
        public string EncodeWithPasswordAndCompression()
        {
            return DataEncoderUtil.Encode(data, "Password123!@#", true, 0);
        }

        [Benchmark]
        public string EncodeWithPasswordAndCompressionAndDummies()
        {
            return DataEncoderUtil.Encode(data, "Password123!@#", true, 10);
        }

    }

    [SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 10, targetCount: 10)]
    public class DecodeBench
    {

        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[4_000_000];
            new Random(100).NextBytes(data);
        }

        [Benchmark]
        public string Decode()
        {
            return DataEncoderUtil.Decode(data, "", false, 0);
        }

        [Benchmark]
        public string DecodeWithPassword()
        {
            return DataEncoderUtil.Decode(data, "Password123!@#", false, 0);
        }

        [Benchmark]
        public string DecodeWithCompression()
        {
            return DataEncoderUtil.Decode(data, "", true, 0);
        }

        [Benchmark]
        public string DecodeWithDummies()
        {
            return DataEncoderUtil.Decode(data, "", false, 10);
        }

        [Benchmark]
        public string DecodeWithPasswordAndCompression()
        {
            return DataEncoderUtil.Decode(data, "Password123!@#", true, 0);
        }

        [Benchmark]
        public string DecodeWithPasswordAndCompressionAndDummies()
        {
            return DataEncoderUtil.Decode(data, "Password123!@#", true, 10);
        }

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<EncodeBench>();
            BenchmarkRunner.Run<DecodeBench>();
        }
    }

}