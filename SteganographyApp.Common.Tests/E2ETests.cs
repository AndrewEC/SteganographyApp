namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.IO;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Data;
    using SteganographyApp.Common.IO;

    [TestFixture]
    public class E2ETests : FixtureWithRealObjects
    {
        private InputArguments args;
        private ImageStore imageStore;
        private ImageStoreIO wrapper;

        [SetUp]
        public void E2ESetUp()
        {
            GlobalCounter.Instance.Reset();

            args = new InputArguments()
            {
                FileToEncode = "TestAssets/test.zip",
                DecodedOutputFile = "TestAssets/testing.zip",
                CoverImages = ImmutableArray.Create(new string[] { "TestAssets/001.png" }),
                Password = "testing",
                UseCompression = true,
                DummyCount = 3,
                RandomSeed = "random-seed",
            };
            imageStore = new ImageStore(args);
            wrapper = imageStore.CreateIOWrapper();
        }

        [TearDown]
        public void E2ETearDown()
        {
            GlobalCounter.Instance.Reset();

            imageStore.CleanImageLSBs();
            wrapper.ResetToImage(0);
            if (File.Exists(args.DecodedOutputFile))
            {
                File.Delete(args.DecodedOutputFile);
            }
        }

        [Test]
        public void TestFullWriteReadHappyPath()
        {
            string content = string.Empty;
            var contentChunkSize = -1;
            using (wrapper)
            {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    contentChunkSize = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            using (var writer = new ChunkTableWriter(imageStore, args))
            {
                writer.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
            }

            GlobalCounter.Instance.Reset();

            using (var tableReader = new ChunkTableReader(imageStore, args))
            {
                var readTable = tableReader.ReadContentChunkTable();
                using (var writer = new ContentWriter(args))
                {
                    string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                    Assert.AreEqual(content, binary);
                    writer.WriteContentChunkToFile(binary);
                }
                long target = new FileInfo(args.FileToEncode).Length;
                long actual = new FileInfo(args.DecodedOutputFile).Length;
                Assert.AreEqual(target, actual);
            }
        }

        [Test]
        public void TestPasswordMismatchError()
        {
            // writing file content to image
            string content = string.Empty;
            var contentChunkSize = -1;
            using (wrapper)
            {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    contentChunkSize = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            using (var tableWriter = new ChunkTableWriter(imageStore, args))
            {
                tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
            }

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.Password = "Wrong Password";
            using (var tableReader = new ChunkTableReader(imageStore, args))
            {
                var readTable = tableReader.ReadContentChunkTable();
                using (var writer = new ContentWriter(args))
                {
                    string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                    Assert.Throws<TransformationException>(() => writer.WriteContentChunkToFile(binary));
                }
            }
        }

        [Test]
        public void TestDummyCountMissmatchProducesException()
        {
            // writing file content to image
            var contentChunkSize = -1;
            using (wrapper)
            {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                string content = string.Empty;
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    contentChunkSize = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            using (var tableWriter = new ChunkTableWriter(imageStore, args))
            {
                tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
            }

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.DummyCount = 5;
            using (var tableReader = new ChunkTableReader(imageStore, args))
            {
                var readTable = tableReader.ReadContentChunkTable();
                using (var writer = new ContentWriter(args))
                {
                    string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                    Assert.Throws<TransformationException>(() => writer.WriteContentChunkToFile(binary));
                }
            }
        }

        [Test]
        public void TestCompressMismatchProducesBadFile()
        {
            // writing file content to image
            var contentChunkSize = -1;
            string content = string.Empty;
            using (wrapper)
            {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    contentChunkSize = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            using (var tableWriter = new ChunkTableWriter(imageStore, args))
            {
                tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
            }

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.UseCompression = false;
            using (var tableReader = new ChunkTableReader(imageStore, args))
            {
                var readTable = tableReader.ReadContentChunkTable();
                using (var writer = new ContentWriter(args))
                {
                    string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                    Assert.AreEqual(content, binary);
                    writer.WriteContentChunkToFile(binary);
                }
                long target = new FileInfo(args.FileToEncode).Length;
                long actual = new FileInfo(args.DecodedOutputFile).Length;
                Assert.AreNotEqual(target, actual);
            }
        }

        [Test]
        public void TestRandomSeedMissmatchProducesCompressionException()
        {
            // writing file content to image
            string content = string.Empty;
            var contentChunkSize = -1;
            using (wrapper)
            {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    contentChunkSize = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            using (var tableWriter = new ChunkTableWriter(imageStore, args))
            {
                tableWriter.WriteContentChunkTable(ImmutableArray.Create(new int[] { contentChunkSize }));
            }

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.RandomSeed = string.Empty;
            using (var tableReader = new ChunkTableReader(imageStore, args))
            {
                Assert.Throws<OverflowException>(() => tableReader.ReadContentChunkTable());
            }
        }
    }
}
