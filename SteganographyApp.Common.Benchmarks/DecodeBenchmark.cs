using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Benchmarks
{

    [SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 3, targetCount: 3)]
    [MeanColumn, MinColumn, MaxColumn]
    public class DecodeBench
    {

        private readonly string Password = "Password123!@#";

        private readonly string RandomSeed = "randomSeed";

        private string data;

        [GlobalSetup(Target=nameof(DecodeWithPassword))]
        public void EncryptedSetup()
        {
            byte[] rawData = new byte[4_000_000];
            new Random(100).NextBytes(rawData);
            data = new DataEncoderUtil().Encode(rawData, Password, false, 0, "");
        }

        [Benchmark]
        public byte[] DecodeWithPassword()
        {
            return new DataEncoderUtil().Decode(data, Password, false, 0, "");
        }

        [GlobalSetup(Target=nameof(DecodeWithPasswordAndCompression))]
        public void EncryptedAndCompressedSetup()
        {
            byte[] rawData = new byte[4_000_000];
            new Random(100).NextBytes(rawData);
            data = new DataEncoderUtil().Encode(rawData, Password, true, 0, "");
        }

        [Benchmark]
        public byte[] DecodeWithPasswordAndCompression()
        {
            return new DataEncoderUtil().Decode(data, Password, true, 0, "");
        }

        [GlobalSetup(Target=nameof(DecodeWithPasswordAndCompressionAndRandomization))]
        public void DecodeWithPasswordAndCompressionAndRandomizationSetup()
        {
            byte[] rawData = new byte[4_000_000];
            new Random(100).NextBytes(rawData);
            data = new DataEncoderUtil().Encode(rawData, Password, true, 0, RandomSeed);
        }

        [Benchmark]
        public byte[] DecodeWithPasswordAndCompressionAndRandomization()
        {
            return new DataEncoderUtil().Decode(data, Password, true, 0, RandomSeed);
        }

    }

}