using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;

using System.Linq;
using System.Collections.Generic;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class ImageStoreTests
    {

        private readonly int TestImagePixelCount = 1_064_000;
        private readonly int BitsPerPixel = 3;

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
                wrapper.CleanImageLSBs();
            }
            wrapper.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ImageProcessingException), AllowDerivedTypes = false)]
        public void TestNotEnoughImagesProducesException()
        {
            int size = TestImagePixelCount * BitsPerPixel + 1;
            string binary = new string(Enumerable.Repeat('0', size).ToArray());
            wrapper.WriteBinaryChunk(binary);
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
            wrapper.WriteContentChunkTable(entries);
            wrapper.ResetToImage(0);
            var read = wrapper.ReadContentChunkTable();

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
            int written = -1;
            using(wrapper){
                written = wrapper.WriteBinaryChunk(binary);
                wrapper.Complete();
            }
            Assert.AreEqual(binary.Length, written);
            Assert.AreEqual(binary, wrapper.ReadBinaryChunk(binary.Length));
        }
    }
}
