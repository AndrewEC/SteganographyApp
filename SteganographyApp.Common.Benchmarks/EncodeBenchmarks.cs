namespace SteganographyApp.Common.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using SteganographyApp.Common.Data;

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
        return new DataEncoderUtil().Encode(data, "", false, 0, "", 0);
    }

    [Benchmark]
    public string EncodeWithPassword()
    {
        return new DataEncoderUtil().Encode(data, "Password123!@#", false, 0, "", AdditionalHashIterations);
    }

    [Benchmark]
    public string EncodeWithCompression()
    {
        return new DataEncoderUtil().Encode(data, "", true, 0, "", 0);
    }

    [Benchmark]
    public string EncodeWithDummies()
    {
        return new DataEncoderUtil().Encode(data, "", false, 375, "", 0);
    }

    [Benchmark]
    public string EncodeWithRandomization()
    {
        return new DataEncoderUtil().Encode(data, "", false, 0, "12345", 0);
    }

    [Benchmark]
    public string EncodeWithPasswordAndCompressionAndDummiesAndRandomization()
    {
        return new DataEncoderUtil().Encode(data, "Password123!@#", true, 375, "randomSeed", AdditionalHashIterations);
    }

}