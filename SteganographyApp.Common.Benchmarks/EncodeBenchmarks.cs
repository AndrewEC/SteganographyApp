using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Benchmarks
{

    [SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 3, targetCount: 3)]
    [MeanColumn, MinColumn, MaxColumn]
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
            return new DataEncoderUtil().Encode(data, "", false, 0, "");
        }

        [Benchmark]
        public string EncodeWithPassword()
        {
            return new DataEncoderUtil().Encode(data, "Password123!@#", false, 0, "");
        }

        [Benchmark]
        public string EncodeWithCompression()
        {
            return new DataEncoderUtil().Encode(data, "", true, 0, "");
        }

        [Benchmark]
        public string EncodeWithDummies()
        {
            return new DataEncoderUtil().Encode(data, "", false, 10, "");
        }

        [Benchmark]
        public string EncodeWithPasswordAndCompression()
        {
            return new DataEncoderUtil().Encode(data, "Password123!@#", true, 0, "");
        }

        [Benchmark]
        public string EncodeWithPasswordAndCompressionAndDummies()
        {
            return new DataEncoderUtil().Encode(data, "Password123!@#", true, 10, "");
        }

        [Benchmark]
        public string EncodeWithPasswordAndCompressionAndDummiesAndRandomization()
        {
            return new DataEncoderUtil().Encode(data, "Password123!@#", true, 10, "randomSeed");
        }

    }

}