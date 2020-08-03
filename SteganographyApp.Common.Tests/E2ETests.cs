using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class E2ETests
    {

        private InputArguments args;
        private ImageStore imageStore;
        private ImageStore.ImageStoreWrapper wrapper;

        [TestInitialize]
        public void SetUp()
        {
            GlobalCounter.Instance.Reset();

            args = new InputArguments()
            {
                FileToEncode = "TestAssets/test.zip",
                DecodedOutputFile = "TestAssets/testing.zip",
                CoverImages = new string[] { "TestAssets/001.png" },
                Password = "testing",
                UseCompression = true,
                DummyCount = 3,
                RandomSeed = "random-seed"
            };
            imageStore = new ImageStore(args);
            wrapper = imageStore.CreateIOWrapper();
        }

        [TestCleanup]
        public void TearDown()
        {
            GlobalCounter.Instance.Reset();

            imageStore.CleanImageLSBs();
            wrapper.ResetToImage(0);
            if(File.Exists(args.DecodedOutputFile))
            {
                File.Delete(args.DecodedOutputFile);
            }
        }

        [TestMethod]
        public void TestFullWriteReadHappyPath()
        {
            string content = "";
            var table = new int[1];
            using (wrapper)
            {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using(var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    table[0] = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            imageStore.WriteContentChunkTable(table);

            GlobalCounter.Instance.Reset();

            wrapper.ResetToImage(0);
            var readTable = imageStore.ReadContentChunkTable();
            using(var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteContentChunkToFile(binary);
            }
            long target = new FileInfo(args.FileToEncode).Length;
            long actual = new FileInfo(args.DecodedOutputFile).Length;
            Assert.AreEqual(target, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestPasswordMismatchError()
        {
            // writing file content to image
            string content = "";
            var table = new int[1];
            using(wrapper){
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    table[0] = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            imageStore.WriteContentChunkTable(table);

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.Password = "Wrong Password";
            wrapper.ResetToImage(0);
            var readTable = imageStore.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                writer.WriteContentChunkToFile(binary);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestDummyCountMissmatchProducesException()
        {
            // writing file content to image
            var table = new int[1];
            using(wrapper) {
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                string content = "";
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    table[0] = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            imageStore.WriteContentChunkTable(table);

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.DummyCount = 5;
            wrapper.ResetToImage(0);
            var readTable = imageStore.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                writer.WriteContentChunkToFile(binary);
            }
        }

        [TestMethod]
        public void TestCompressMismatchProducesBadFile()
        {
            // writing file content to image
            var table = new int[1];
            string content = "";
            using(wrapper){
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    table[0] = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            imageStore.WriteContentChunkTable(table);

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.UseCompression = false;
            wrapper.ResetToImage(0);
            var readTable = imageStore.ReadContentChunkTable();
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


        [TestMethod]
        [ExpectedException(typeof(ImageProcessingException), AllowDerivedTypes = false)]
        public void TestRandomSeedMissmatchProducesCompressionException()
        {
            // writing file content to image
            string content = "";
            var table = new int[1];
            using(wrapper){
                int requiredBitsForTable = Calculator.CalculateRequiredBitsForContentTable(args.FileToEncode, args.ChunkByteSize);
                wrapper.SeekToPixel(requiredBitsForTable);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunkFromFile();
                    int written = wrapper.WriteContentChunkToImage(content);
                    table[0] = written;
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.EncodeComplete();
            }
            imageStore.WriteContentChunkTable(table);

            GlobalCounter.Instance.Reset();

            // reading file content from image
            args.RandomSeed = "";
            wrapper.ResetToImage(0);
            var readTable = imageStore.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadContentChunkFromImage(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteContentChunkToFile(binary);
            }
        }

    }
}
