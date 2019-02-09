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

        [TestMethod]
        public void TestInsertAndRemoveDummiesWithMatchingCountIsSuccessful()
        {
            string binary = "10010101101111100001101010101010111";
            int dummyCount = 3;
            string inserted = DataEncoderUtil.InsertDummies(dummyCount, binary);
            Assert.AreNotEqual(binary, inserted, "After inserting dummies the new binary value should be different than the original.");
            string removed = DataEncoderUtil.RemoveDummies(dummyCount, inserted);
            Assert.AreEqual(binary, removed, "After removing the dummies the new binary value should be the same as the original value.");
        }

        [TestMethod]
        public void TestInsertAndRemoveDummiesMissmatchCountReturnsBadResult()
        {
            string binary = "10010101101111100001101010101010111";
            string inserted = DataEncoderUtil.InsertDummies(3, binary);
            Assert.AreNotEqual(binary, inserted, "After inserting dummies the new binary value should be different than the original.");
            string removed = DataEncoderUtil.RemoveDummies(2, inserted);
            Assert.AreNotEqual(binary, removed, "After removing the dummies the new binary value should not be the same as the original value.");
        }

        [TestMethod]
        [ExpectedException(typeof(TransformationException), AllowDerivedTypes = false)]
        public void TestInsertAndRemoveWithMissmatchedCountThrowsException()
        {
            string binary = "10010101101111100001101010101010111";
            string inserted = DataEncoderUtil.InsertDummies(3, binary);
            Assert.AreNotEqual(binary, inserted, "After inserting dummies the new binary value should be different than the original.");
            string removed = DataEncoderUtil.RemoveDummies(30, inserted);
        }

    }
}
