namespace SteganographyApp.Common.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection;

[SimpleJob(RunStrategy.Monitoring, launchCount: 3, warmupCount: 3)]
[MeanColumn, MinColumn, MaxColumn]
public class EncodeBench
{
    private readonly int AdditionalHashIterations = 13;

    private byte[] data;

    [GlobalSetup]
    public void Setup()
    {
        data = new byte[500_000];
        new Random(100).NextBytes(data);
    }

    [Benchmark]
    public string Encode()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(data, "", false, 0, "", 0);
    }

    [Benchmark]
    public string EncodeWithPassword()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(data, "Password123!@#", false, 0, "", AdditionalHashIterations);
    }

    [Benchmark]
    public string EncodeWithCompression()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(data, "", true, 0, "", 0);
    }

    [Benchmark]
    public string EncodeWithDummies()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(data, "", false, 375, "", 0);
    }

    [Benchmark]
    public string EncodeWithRandomization()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(data, "", false, 0, "12345", 0);
    }

    [Benchmark]
    public string EncodeWithPasswordAndCompressionAndDummiesAndRandomization()
    {
        return ServiceContainer.GetService<IDataEncoderUtil>()
            .Encode(data, "Password123!@#", true, 375, "randomSeed", AdditionalHashIterations);
    }

}