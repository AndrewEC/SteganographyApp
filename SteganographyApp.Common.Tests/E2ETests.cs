﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
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
            wrapper.ResetToImage(0);
            wrapper.CleanImageLSBs();
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
            var table = new LinkedList<int>();
            using (wrapper)
            {
                wrapper.SeekToPixel(imageStore.RequiredContentChunkTableBitSize);
                using(var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunk();
                    int written = wrapper.WriteBinaryChunk(content);
                    table.AddFirst(new LinkedListNode<int>(written));
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.Complete();
            }
            imageStore.WriteContentChunkTable(table);

            wrapper.ResetToImage(0);
            var readTable = wrapper.ReadContentChunkTable();
            using(var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadBinaryChunk(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteContentChunk(binary);
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
            var table = new LinkedList<int>();
            using(wrapper){
                wrapper.SeekToPixel(imageStore.RequiredContentChunkTableBitSize);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunk();
                    int written = wrapper.WriteBinaryChunk(content);
                    table.AddFirst(new LinkedListNode<int>(written));
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.Complete();
            }
            imageStore.WriteContentChunkTable(table);

            // reading file content from image
            args.Password = "Wrong Password";
            wrapper.ResetToImage(0);
            var readTable = wrapper.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadBinaryChunk(readTable[0]);
                writer.WriteContentChunk(binary);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestDummyCountMissmatchProducesException()
        {
            // writing file content to image
            var table = new LinkedList<int>();
            using(wrapper) {
                wrapper.SeekToPixel(imageStore.RequiredContentChunkTableBitSize);
                string content = "";
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunk();
                    int written = wrapper.WriteBinaryChunk(content);
                    table.AddFirst(new LinkedListNode<int>(written));
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.Complete();
            }
            imageStore.WriteContentChunkTable(table);

            // reading file content from image
            args.DummyCount = 5;
            wrapper.ResetToImage(0);
            var readTable = wrapper.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadBinaryChunk(readTable[0]);
                writer.WriteContentChunk(binary);
            }
        }

        [TestMethod]
        public void TestCompressMismatchProducesBadFile()
        {
            // writing file content to image
            var table = new LinkedList<int>();
            string content = "";
            using(wrapper){
                wrapper.SeekToPixel(imageStore.RequiredContentChunkTableBitSize);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunk();
                    int written = wrapper.WriteBinaryChunk(content);
                    table.AddFirst(new LinkedListNode<int>(written));
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.Complete();
            }
            imageStore.WriteContentChunkTable(table);

            // reading file content from image
            args.UseCompression = false;
            wrapper.ResetToImage(0);
            var readTable = wrapper.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadBinaryChunk(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteContentChunk(binary);
            }
            long target = new FileInfo(args.FileToEncode).Length;
            long actual = new FileInfo(args.DecodedOutputFile).Length;
            Assert.AreNotEqual(target, actual);
        }


        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestRandomSeedMissmatchProducesCompressionException()
        {
            // writing file content to image
            string content = "";
            var table = new LinkedList<int>();
            using(wrapper){
                wrapper.SeekToPixel(imageStore.RequiredContentChunkTableBitSize);
                using (var reader = new ContentReader(args))
                {
                    content = reader.ReadContentChunk();
                    int written = wrapper.WriteBinaryChunk(content);
                    table.AddFirst(new LinkedListNode<int>(written));
                    Assert.AreEqual(content.Length, written);
                }
                wrapper.Complete();
            }
            imageStore.WriteContentChunkTable(table);

            // reading file content from image
            args.RandomSeed = "";
            wrapper.ResetToImage(0);
            var readTable = wrapper.ReadContentChunkTable();
            using (var writer = new ContentWriter(args))
            {
                string binary = wrapper.ReadBinaryChunk(readTable[0]);
                Assert.AreEqual(content, binary);
                writer.WriteContentChunk(binary);
            }
        }

    }
}
