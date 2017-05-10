using System;
using System.IO;
using SteganographyAppCommon.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteganographyAppCommon.Tests
{
    [TestClass]
    public class DataEncoderUtilTests
    {

        private readonly string OutputFilePath = "TestAssets/output.zip";
        private readonly string InputFile = "TestAssets/test.zip";
        private readonly string Password = "testPassword";

        [TestCleanup]
        public void TearDown()
        {
            if (File.Exists(OutputFilePath))
            {
                File.Delete(OutputFilePath);
            }
        }

        [TestMethod]
        public void TestEncodeFileWithoutPassHappyPath()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, "", false);
            Assert.IsTrue(binary.Length > 0 && binary.Length % 8 == 0, "Binary string length needs to be greater than zero and evenly divisble by 8.");
            foreach (char c in binary)
            {
                Assert.IsTrue(c == '0' || c == '1', "All characters in binary string should be either 1 or 0.");
            }
        }

        [TestMethod]
        public void TestEncodeFileWithPasswordHappyPath()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, Password, false);
            Assert.IsTrue(binary.Length > 0 && binary.Length % 8 == 0, "Binary string length needs to be greater than zero and evenly divisble by 8.");
            foreach (char c in binary)
            {
                Assert.IsTrue(c == '0' || c == '1', "All characters in binary string should be either 1 or 0.");
            }
        }

        [TestMethod]
        public void TestEncodeAndDecodeMatch()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, Password, false);
            byte[] bytes = DataEncoderUtil.Decode(binary, Password, false);

            Assert.AreEqual(fileBytes.Length, bytes.Length, "Expected length of byte arrays to match.");
            for (int i = 0; i < fileBytes.Length; i++)
            {
                Assert.AreEqual(fileBytes[i], bytes[i], "Expected all read and decoded bytes to match");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestMismatchPasswordEncodeDecodeException()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, Password, false);
            byte[] bytes = DataEncoderUtil.Decode(binary, "testPasswords", false);
        }

        [TestMethod]
        public void TestEncodeAndDecodeCompressionMatch()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, Password, true);
            byte[] bytes = DataEncoderUtil.Decode(binary, Password, true);

            Assert.AreEqual(fileBytes.Length, bytes.Length, "Encoded/Decoded byte length should match original file byte length.");
            for (int i = 0; i < fileBytes.Length; i++)
            {
                Assert.AreEqual(fileBytes[i], bytes[i], "Expected all read and decoded bytes to match");
            }
        }

        [TestMethod]
        public void TestCompressedIsLessThanUncompressed()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string compressed = DataEncoderUtil.Encode(fileBytes, Password, true);
            string uncompressed = DataEncoderUtil.Encode(fileBytes, Password, false);
            Assert.IsTrue(compressed.Length < uncompressed.Length, "Compressed file data should be less than uncompressed data.");

            compressed = DataEncoderUtil.Encode(fileBytes, "", true);
            uncompressed = DataEncoderUtil.Encode(fileBytes, "", false);
            Assert.IsTrue(compressed.Length < uncompressed.Length, "Compressed file data should be less than uncompressed data.");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestCompressUncompressMismatchThrowsException()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string compressed = DataEncoderUtil.Encode(fileBytes, Password, true);
            byte[] uncompressed = DataEncoderUtil.Decode(InputFile, Password, false);
        }

    }
}
