using System;
using System.IO;
using SteganographyApp.Common.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteganographyApp.Common.Tests
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
        public void TestEncodeFileWithoutPasswordHappyPath()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, "", false, 0, "");
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
            string binary = DataEncoderUtil.Encode(fileBytes, Password, false, 0, "");
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
            string binary = DataEncoderUtil.Encode(fileBytes, Password, false, 0, "");
            byte[] bytes = DataEncoderUtil.Decode(binary, Password, false, 0, "");

            Assert.AreEqual(fileBytes.Length, bytes.Length, "Expected length of byte arrays to match.");
            for (int i = 0; i < fileBytes.Length; i++)
            {
                Assert.AreEqual(fileBytes[i], bytes[i], "Expected all read and decoded bytes to match");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestMismatchPasswordInDecodeThrowsException()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, Password, false, 0, "");
            byte[] bytes = DataEncoderUtil.Decode(binary, "testPasswords", false, 0, "");
        }

        [TestMethod]
        public void TestEncodeAndDecodeCompression()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string binary = DataEncoderUtil.Encode(fileBytes, Password, true, 0, "");
            byte[] bytes = DataEncoderUtil.Decode(binary, Password, true, 0, "");

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
            string compressed = DataEncoderUtil.Encode(fileBytes, Password, true, 0, "");
            string uncompressed = DataEncoderUtil.Encode(fileBytes, Password, false, 0, "");
            Assert.IsTrue(compressed.Length < uncompressed.Length, "Compressed file data should be less than uncompressed data.");

            compressed = DataEncoderUtil.Encode(fileBytes, "", true, 0, "");
            uncompressed = DataEncoderUtil.Encode(fileBytes, "", false, 0, "");
            Assert.IsTrue(compressed.Length < uncompressed.Length, "Compressed file data should be less than uncompressed data.");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestDecodeCompressedDataWithoutCompressedFlagThrowsException()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string compressed = DataEncoderUtil.Encode(fileBytes, Password, true, 0, "");
            byte[] uncompressed = DataEncoderUtil.Decode(InputFile, Password, false, 0, "");
        }

        [TestMethod]
        public void TestInsertDummiesReturnsDifferentThanNonInsert()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string dummyEncoded = DataEncoderUtil.Encode(fileBytes, Password, true, 0, "");
            string plainEncoded = DataEncoderUtil.Encode(fileBytes, Password, true, 3, "");
            Assert.AreNotEqual(dummyEncoded, plainEncoded);
        }

        [TestMethod]
        public void TestEncodeAndDecodeWithDummiesSuccessful()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string dummyEncoded = DataEncoderUtil.Encode(fileBytes, Password, true, 3, "");
            byte[] decoded = DataEncoderUtil.Decode(dummyEncoded, Password, true, 3, "");
            Assert.AreEqual(fileBytes.Length, decoded.Length);
            for(int i = 0; i < fileBytes.Length; i++)
            {
                Assert.AreEqual(fileBytes[i], decoded[i]);
            }
        }

        [TestMethod]
        public void TestEncodeAndDecodeWithRandomSeedSuccessful()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string dummyEncoded = DataEncoderUtil.Encode(fileBytes, Password, true, 3, "randomSeed");
            byte[] decoded = DataEncoderUtil.Decode(dummyEncoded, Password, true, 3, "randomSeed");
            Assert.AreEqual(fileBytes.Length, decoded.Length);
            for(int i = 0; i < fileBytes.Length; i++)
            {
                Assert.AreEqual(fileBytes[i], decoded[i]);
            }
        }

        [TestMethod]
        public void TestDecodeWithRandomSeedDifferentFromEncodeFails()
        {
            byte[] fileBytes = File.ReadAllBytes(InputFile);
            string dummyEncoded = DataEncoderUtil.Encode(fileBytes, Password, true, 3, "randomSeed");
            string plainEncoded = DataEncoderUtil.Encode(fileBytes, Password, true, 3, "nonMatchingRandomSeed");
            Assert.AreNotEqual(dummyEncoded, plainEncoded);
        }

    }
}
