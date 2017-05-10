using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using SteganographyAppCommon.IO;
using SteganographyAppCommon.IO.Content;
using SteganographyAppCommon.Data;

namespace SteganographyAppCommon.Tests
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
            var writeTable = new List<int>(1);
            var writeContent = "";
            using (var reader = new ContentReader(args))
            {
                writeContent = reader.ReadNextChunk();
                int written = store.Write(writeContent);
                Assert.AreEqual(writeContent.Length, written);
                writeTable.Add(written);
                store.ResetTo(args.CoverImages[0]);
                store.WriteContentChunkTable(writeTable);
            }

            store.ResetTo(args.CoverImages[0]);
            var readTable = store.ReadContentChunkTable();
            Assert.AreEqual(writeTable.Count, readTable.Count);

            string readContent = store.Read(readTable[0]);
            Assert.AreEqual(writeContent, readContent);

            using (var writer = new ContentWriter(args))
            {
                writer.WriteChunk(readContent);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestPasswordMismatchError()
        {
            store.Next();
            store.Seek(store.RequiredContentChunkTableBitSize);
            var writeTable = new List<int>(1);
            var writeContent = "";
            using (var reader = new ContentReader(args))
            {
                writeContent = reader.ReadNextChunk();
                int written = store.Write(writeContent);
                Assert.AreEqual(writeContent.Length, written);
                writeTable.Add(written);
                store.ResetTo(args.CoverImages[0]);
                store.WriteContentChunkTable(writeTable);
            }

            args.Password = "testing1";
            store = new ImageStore(args);
            store.Next();
            var readTable = store.ReadContentChunkTable();
            Assert.AreEqual(writeTable.Count, readTable.Count);
            string readContent = store.Read(readTable[0]);
            Assert.AreEqual(writeContent, readContent);

            using (var writer = new ContentWriter(args))
            {
                writer.WriteChunk(readContent);
            }
        }

    }
}
