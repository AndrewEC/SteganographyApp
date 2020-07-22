using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Benchmarks
{

    [SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 10, targetCount: 10)]
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

    [SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 10, targetCount: 10)]
    [MeanColumn, MinColumn, MaxColumn]
    public class DecodeBench
    {

        private readonly string Password = "Password123!@#";
        private readonly int DummyCount = 10;

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

        [GlobalSetup(Target=nameof(DecodeWithPasswordAndCompressionAndDummies))]
        public void EncryptedAndCompressedAndDummiesSetup()
        {
            byte[] rawData = new byte[4_000_000];
            new Random(100).NextBytes(rawData);
            data = new DataEncoderUtil().Encode(rawData, Password, true, DummyCount, "");
        }

        [Benchmark]
        public byte[] DecodeWithPasswordAndCompressionAndDummies()
        {
            return new DataEncoderUtil().Decode(data, Password, true, DummyCount, "");
        }

        [GlobalSetup(Target=nameof(DecodeWithPasswordAndCompressionAndDummiesAndRandomization))]
        public void DecodeWithPasswordAndCompressionAndDummiesAndRandomizationSetup()
        {
            byte[] rawData = new byte[4_000_000];
            new Random(100).NextBytes(rawData);
            data = new DataEncoderUtil().Encode(rawData, Password, true, DummyCount, RandomSeed);
        }

        [Benchmark]
        public byte[] DecodeWithPasswordAndCompressionAndDummiesAndRandomization()
        {
            return new DataEncoderUtil().Decode(data, Password, true, DummyCount, RandomSeed);
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