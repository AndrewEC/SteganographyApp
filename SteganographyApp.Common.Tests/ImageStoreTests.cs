using Microsoft.VisualStudio.TestTools.UnitTesting;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.IO;

using System.Linq;

namespace SteganographyApp.Common.Tests
{
    [TestClass]
    public class ImageStoreTests
    {

        private readonly int TestImagePixelCount = 1_064_000;
        private readonly int BitsPerPixel = 3;

        private ImageStore imageStore;
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
            imageStore = new ImageStore(args);
            wrapper = imageStore.CreateIOWrapper();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (imageStore != null && args.CoverImages != null)
            {
                imageStore.CleanImageLSBs();
            }
            wrapper.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ImageProcessingException), AllowDerivedTypes = false)]
        public void TestNotEnoughImagesProducesException()
        {
            int size = TestImagePixelCount * BitsPerPixel + 1;
            string binary = new string(Enumerable.Repeat('0', size).ToArray());
            wrapper.WriteContentChunkToImage(binary);
        }

        [TestMethod]
        public void TestWrittenReadContentTableMatches()
        {
            var entries = new int[] { 3000, 4000 };
            
            imageStore.WriteContentChunkTable(entries);
            wrapper.ResetToImage(0);
            var read = imageStore.ReadContentChunkTable();

            Assert.AreEqual(entries.Length, read.Length);
            Assert.AreEqual(read[0], entries[0]);
            Assert.AreEqual(read[1], entries[1]);
        }

        [TestMethod]
        public void TestWriteAndReadContentMatches()
        {
            string binary = "00101001110010100100011001101010";
            int written = -1;
            using(wrapper){
                written = wrapper.WriteContentChunkToImage(binary);
                wrapper.EncodeComplete();
            }
            Assert.AreEqual(binary.Length, written);
            Assert.AreEqual(binary, wrapper.ReadContentChunkFromImage(binary.Length));
        }
    }
}
