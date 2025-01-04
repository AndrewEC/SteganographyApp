namespace SteganographyApp.Common.Integration.Tests;

using System;
using System.Collections.Immutable;
using System.IO;

using NUnit.Framework;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;

[TestFixture]
public class E2ETests
{
    private CommonArguments args;
    private ImageStore imageStore;

    [SetUp]
    public void E2ESetUp()
    {
        args = new CommonArguments()
        {
            FileToEncode = "TestAssets/test.zip",
            DecodedOutputFile = "TestAssets/testing.zip",
            CoverImages = ImmutableArray.Create(new string[] { "TestAssets/CoverImage.png" }),
            Password = "testing",
            UseCompression = true,
            DummyCount = 3,
            RandomSeed = "random-seed",
            BitsToUse = 1,
        };
        imageStore = new ImageStore(args);
    }

    [TearDown]
    public void E2ETearDown()
    {
        if (File.Exists(args.DecodedOutputFile))
        {
            File.Delete(args.DecodedOutputFile);
        }
    }

    [Test]
    public void TestFullWriteReadHappyPath()
    {
        // Store some data from the sample input file in the cover images.
        string content = string.Empty;
        var contentChunkSize = -1;
        using (var stream = imageStore.OpenStream())
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
            stream.SeekToPixel(requiredBitsForTable);
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadContentChunkFromFile();
                int written = stream.WriteContentChunkToImage(content);
                contentChunkSize = written;
                Assert.That(written, Is.EqualTo(content.Length));
            }
        }

        // Write the content chunk table to support the read operation.
        using (var writer = new ChunkTableWriter(imageStore, args))
        {
            writer.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
        }

        // Read the content previously written to the image and verify it matches the original input value.
        using (var stream = imageStore.OpenStream())
        {
            ImmutableArray<int> readTable = new ChunkTableReader(stream, args).ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = stream.ReadContentChunkFromImage(readTable[0]);
                Assert.That(binary, Is.EqualTo(content));
                writer.WriteContentChunkToFile(binary);
            }
            long target = new FileInfo(args.FileToEncode).Length;
            long actual = new FileInfo(args.DecodedOutputFile).Length;
            Assert.That(actual, Is.EqualTo(target));
        }
    }

    [Test]
    public void TestFullWriteReadHappyPathWith2Bits()
    {
        args.BitsToUse = 2;
        TestFullWriteReadHappyPath();
    }

    [Test]
    public void TestPasswordMismatchError()
    {
        // writing file content to image
        string content = string.Empty;
        var contentChunkSize = -1;
        using (var stream = imageStore.OpenStream())
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
            stream.SeekToPixel(requiredBitsForTable);
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadContentChunkFromFile();
                int written = stream.WriteContentChunkToImage(content);
                contentChunkSize = written;
                Assert.That(written, Is.EqualTo(content.Length));
            }
        }

        using (var tableWriter = new ChunkTableWriter(imageStore, args))
        {
            tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
        }

        // reading file content from image
        args.Password = "Wrong Password";
        using (var stream = imageStore.OpenStream())
        {
            var readTable = new ChunkTableReader(stream, args).ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = stream.ReadContentChunkFromImage(readTable[0]);
                Assert.Throws<TransformationException>(() => writer.WriteContentChunkToFile(binary));
            }
        }
    }

    [Test]
    public void TestDummyCountMissmatchProducesException()
    {
        // writing file content to image
        var contentChunkSize = -1;
        using (var stream = imageStore.OpenStream())
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
            stream.SeekToPixel(requiredBitsForTable);
            string content = string.Empty;
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadContentChunkFromFile();
                int written = stream.WriteContentChunkToImage(content);
                contentChunkSize = written;
                Assert.That(written, Is.EqualTo(content.Length));
            }
        }

        using (var tableWriter = new ChunkTableWriter(imageStore, args))
        {
            tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
        }

        // reading file content from image
        args.DummyCount = 5;
        using (var stream = imageStore.OpenStream())
        {
            var readTable = new ChunkTableReader(stream, args).ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = stream.ReadContentChunkFromImage(readTable[0]);
                long target = new FileInfo(args.FileToEncode).Length;
                long actual = new FileInfo(args.DecodedOutputFile).Length;
                Assert.That(actual, Is.Not.EqualTo(target));
            }
        }
    }

    [Test]
    public void TestCompressMismatchProducesBadFile()
    {
        // writing file content to image
        var contentChunkSize = -1;
        string content = string.Empty;
        using (var stream = imageStore.OpenStream())
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
            stream.SeekToPixel(requiredBitsForTable);
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadContentChunkFromFile();
                int written = stream.WriteContentChunkToImage(content);
                contentChunkSize = written;
                Assert.That(written, Is.EqualTo(content.Length));
            }
        }
        using (var tableWriter = new ChunkTableWriter(imageStore, args))
        {
            tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
        }

        // reading file content from image
        args.UseCompression = false;
        using (var stream = imageStore.OpenStream())
        {
            var readTable = new ChunkTableReader(stream, args).ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = stream.ReadContentChunkFromImage(readTable[0]);
                Assert.That(binary, Is.EqualTo(content));
                Assert.Throws(typeof(TransformationException), () => writer.WriteContentChunkToFile(binary));
            }
        }
    }

    [Test]
    public void TestRandomSeedMissmatchProducesCompressionException()
    {
        // writing file content to image
        string content = string.Empty;
        var contentChunkSize = -1;
        using (var stream = imageStore.OpenStream())
        {
            int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
            stream.SeekToPixel(requiredBitsForTable);
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadContentChunkFromFile();
                int written = stream.WriteContentChunkToImage(content);
                contentChunkSize = written;
                Assert.That(written, Is.EqualTo(content.Length));
            }
        }
        using (var tableWriter = new ChunkTableWriter(imageStore, args))
        {
            tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
        }

        // reading file content from image
        args.RandomSeed = string.Empty;
        using (var stream = imageStore.OpenStream())
        {
            Assert.Throws<OverflowException>(() => new ChunkTableReader(stream, args).ReadContentChunkTable());
        }
    }
}
