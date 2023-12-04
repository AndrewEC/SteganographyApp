namespace SteganographyApp.Common.Tests;

using System;
using NUnit.Framework;
using SteganographyApp.Common.Data;

[TestFixture]
public class CompressionUtilTests
{
    [Test]
    public void TestCompressAndDecompress()
    {
        var compression = new CompressionUtil();

        byte[] original = new byte[1024];
        new Random().NextBytes(original);

        byte[] compressed = compression.Compress(original);
        Assert.That(compressed, Is.Not.EqualTo(original));

        byte[] decompressed = compression.Decompress(compressed);
        Assert.That(decompressed, Is.EqualTo(original));
    }
}