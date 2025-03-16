namespace SteganographyApp.Common.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;

[SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 3)]
[MeanColumn, MinColumn, MaxColumn]
public class DecodeBench
{

    private const int RandomByteCount = 500_000;
    private const string Password = "Password123!@#";
    private const string RandomSeed = "randomSeed";
    private const int AdditionalHashIterations = 13;
    private const int DummyCount = 70;

    private string data;

    [GlobalSetup(Target=nameof(DecodeWithPassword))]
    public void DecodeWithPasswordSetup()
    {
        data = ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(GenerateRandomBytes(), Password, false, 0, "", AdditionalHashIterations);
    }

    [Benchmark]
    public byte[] DecodeWithPassword()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Decode(data, Password, false, 0, "", AdditionalHashIterations);
    }

    [GlobalSetup(Target=nameof(DecodeWithRandomization))]
    public void DecodeWithRandomizationSetup()
    {
        data = ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(GenerateRandomBytes(), Password, false, 0, "", AdditionalHashIterations);
    }

    [Benchmark]
    public byte[] DecodeWithRandomization()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Decode(data, "", false, 0, RandomSeed, 0);
    }

    [GlobalSetup(Target=nameof(DecodeWithDummies))]
    public void DecodeWithDummiesSetup()
    {
        data = ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(GenerateRandomBytes(), "", false, DummyCount, "", 0);
    }

    [Benchmark]
    public byte[] DecodeWithDummies()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Decode(data, "", false, DummyCount, "", 0);
    }

    [GlobalSetup(Target=nameof(DecodeWithCompression))]
    public void DecodeWithCompressionSetup()
    {
        data = ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(GenerateRandomBytes(), "", true, 0, "", 0);
    }

    [Benchmark]
    public byte[] DecodeWithCompression()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Decode(data, "", true, 0, "", 0);
    }

    private static byte[] GenerateRandomBytes()
    {
        byte[] bytes = new byte[RandomByteCount];
        new Random(100).NextBytes(bytes);
        return bytes;
    }
}