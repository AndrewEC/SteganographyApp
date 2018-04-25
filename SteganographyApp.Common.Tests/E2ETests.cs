using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class E2ETests
    {

        private InputArguments args;
        private ImageStore store;

        [TestInitialize]
        public void SetUp()
        {
            args = new InputArguments()
            {
                FileToEncode = "TestAssets/test.zip",
                DecodedOutputFile = "TestAssets/testing.zip",
                CoverImages = new string[] { "TestAssets/001.png" },
                Password = "testing",
                UseCompression = true
            };
            store = new ImageStore(args);
        }

        [TestCleanup]
        public void TearDown()
        {
            store.ResetTo("TestAssets/001.png");
            store.CleanAll(null);
            if(File.Exists(args.DecodedOutputFile))
            {
                File.Delete(args.DecodedOutputFile);
            }
        }

        [TestMethod]
        public void TestFullWriteReadHappyPath()
        {
            store.Next();
            store.Seek(store.RequiredContentChunkTableBitSize);
            string content = "";
            var table = new List<int>();
            using(var reader = new ContentReader(args))
            {
                content = reader.ReadNextChunk();
                int written = store.Write(content);
                table.Add(written);
                Assert.AreEqual(content.Length, written);
            }
            store.Finish(true);
            store.ResetTo(args.CoverImages[0]);
            store.WriteContentChunkTable(table);

            store.ResetTo(args.CoverImages[0]);
            var readTable = store.ReadContentChunkTable();
            using(var writer = new ContentWriter(args))
            {
                string binary = store.Read(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteChunk(binary);
            }
            long target = new FileInfo(args.FileToEncode).Length;
            long actual = new FileInfo(args.DecodedOutputFile).Length;
            Assert.AreEqual(target, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestPasswordMismatchError()
        {
            store.Next();
            store.Seek(store.RequiredContentChunkTableBitSize);
            string content = "";
            var table = new List<int>();
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadNextChunk();
                int written = store.Write(content);
                table.Add(written);
                Assert.AreEqual(content.Length, written);
            }
            store.Finish(true);
            store.ResetTo(args.CoverImages[0]);
            store.WriteContentChunkTable(table);
            args.Password = "Wrong Password";
            store.ResetTo(args.CoverImages[0]);
            var readTable = store.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = store.Read(readTable[0]);
                writer.WriteChunk(binary);
            }
            long target = new FileInfo(args.FileToEncode).Length;
            long actual = new FileInfo(args.DecodedOutputFile).Length;
            Assert.AreEqual(target, actual);
        }

        [TestMethod]
        public void TestCompressMismatchProducesBadFile()
        {
            store.Next();
            store.Seek(store.RequiredContentChunkTableBitSize);
            string content = "";
            var table = new List<int>();
            using (var reader = new ContentReader(args))
            {
                content = reader.ReadNextChunk();
                int written = store.Write(content);
                table.Add(written);
                Assert.AreEqual(content.Length, written);
            }
            store.Finish(true);
            store.ResetTo(args.CoverImages[0]);
            store.WriteContentChunkTable(table);
            args.UseCompression = false;
            store.ResetTo(args.CoverImages[0]);
            var readTable = store.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = store.Read(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteChunk(binary);
            }
            long target = new FileInfo(args.FileToEncode).Length;
            long actual = new FileInfo(args.DecodedOutputFile).Length;
            Assert.AreNotEqual(target, actual);
        }

    }
}
