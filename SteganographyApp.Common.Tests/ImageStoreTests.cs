using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteganographyApp.Common.IO;
using System.Collections.Generic;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class ImageStoreTests
    {

        private ImageStore store;
        private InputArguments args;
        private ImageStore.ImageStoreWrapper wrapper;

        [TestInitialize]
        public void SetUp()
        {
            args = new InputArguments
            {
                FileToEncode = "TestAssets/test.zip",
                CoverImages = new string[] { "TestAssets/001.png", "TestAssets/002.png" }
            };
            store = new ImageStore(args);
            wrapper = store.CreateIOWrapper();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (store != null && args.CoverImages != null)
            {
                store.CleanAll();
            }
            wrapper.Dispose();
        }

        [TestMethod]
        public void TestReturnsProperCurrentImage()
        {
            Assert.AreEqual(args.CoverImages[0], store.CurrentImage);
            wrapper.Next();
            Assert.AreEqual(args.CoverImages[1], store.CurrentImage);

            wrapper.ResetTo(0);
            Assert.AreEqual(args.CoverImages[0], store.CurrentImage);
        }

        [TestMethod]
        [ExpectedException(typeof(ImageProcessingException), AllowDerivedTypes = false)]
        public void TestNotEnoughImagesProducesException()
        {
            wrapper.Next();
            wrapper.Next();
            wrapper.Next();
        }

        [TestMethod]
        public void TestRequiredChunkSizeMatchesExpected()
        {
            Assert.AreEqual(65, store.RequiredContentChunkTableBitSize);

            args = new InputArguments();
            args.FileToEncode = "TestAssets/001.png";
            store = new ImageStore(args);
            Assert.AreEqual(65, store.RequiredContentChunkTableBitSize);
        }

        [TestMethod]
        public void TestWrittenReadContentTableMatches()
        {
            var entries = new List<int>();
            entries.Add(3000);
            entries.Add(4000);
            store.WriteContentChunkTable(entries);
            wrapper.ResetTo(0);
            var read = store.ReadContentChunkTable();

            Assert.AreEqual(entries.Count, read.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                Assert.AreEqual(entries[i], read[i]);
            }
        }

        [TestMethod]
        public void TestWriteAndReadContentMatches()
        {
            string binary = "00101001110010100100011001101010";
            int written = wrapper.Write(binary);
            Assert.AreEqual(binary.Length, written);
            wrapper.Finish(true);
            wrapper.ResetTo(0);
            Assert.AreEqual(binary, wrapper.Read(binary.Length));
        }
    }
}
