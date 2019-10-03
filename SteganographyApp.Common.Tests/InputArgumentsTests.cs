using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using SteganographyApp.Common.Arguments;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class InputArgumentsTests
    {

        private String TestParseWithMissingArgumentsReturnsFalsePostValidator(InputArguments inputs)
        {
            return "Missing Stuff";
        }

        private string NullReturningPostValidator(InputArguments input)
        {
            return null;
        }

        #region General Parsing
        [TestMethod]
        public void TestParseNullArgumentsReturnsFalseAndProducesParseException()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(null, out InputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }

        [TestMethod]
        public void TestParseWithBadArgumentReturnsFalseAndProducesParseException()
        {
            string[] inputsArgs = new string[] { "test", "1" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputsArgs, out InputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }
        #endregion General Parsing

        #region Parsing action
        [DataTestMethod]
        [DataRow("encode", EncodeDecodeAction.Encode)]
        [DataRow("decode", EncodeDecodeAction.Decode)]
        [DataRow("clean", EncodeDecodeAction.Clean)]
        [DataRow("convert", EncodeDecodeAction.Convert)]
        [DataRow("calculate-storage-space", EncodeDecodeAction.CalculateStorageSpace)]
        [DataRow("calculate-encrypted-size", EncodeDecodeAction.CalculateEncryptedSize)]
        public void TestParseActionWithValidValuesProducesValidResult(string actionString, EncodeDecodeAction action)
        {
            string[] inputArgs = new string[] { "--action", actionString };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(action, arguments.EncodeOrDecode);
        }

        [TestMethod]
        public void TestParseActionWithInvalidActionReturnsFalseAndProducesParseException()
        {
            string[] inputArgs = new string[] { "--action", "invalid-action" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }
        #endregion Parsing action

        #region Parse input
        [TestMethod]
        public void TestFileToEncodeWithValidValueProducesValidResult() 
        {
            string path = "TestAssets/test.zip";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(path, arguments.FileToEncode);
        }

        [TestMethod]
        public void TestFileToEncodeWithInvalidPathProducesFalseAndparseException()
        {
            string path = "TestAssets/FileDoesntExist";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());
        }
        #endregion Parse input

        #region Parse enableCompression
        [TestMethod]
        public void TestEnableCompressionWithValidValueProducesValueResult()
        {
            string[] inputArgs = new string[] { "--enableCompression" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.UseCompression);
        }
        #endregion Parse input

        #region Parse printStack
        [TestMethod]
        public void TestPrintStackWithValidValueProducesValidResult()
        {
            string[] inputArgs = new string[] { "--printStack" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.PrintStack);
        }
        #endregion

        #region Parse images
        [TestMethod]
        public void TestParseImagesWithValidSingleValueProducesValidResult()
        {
            string image = "TestAssets/001.png";
            string[] inputArgs = new string[] { "--images", image };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            string[] images = arguments.CoverImages;
            Assert.AreEqual(1, images.Length);
            Assert.AreEqual(image, images[0]);
        }

        [TestMethod]
        public void TestParseImagesWithValidRegexProducesValidResult()
        {
            string expression = "[r]<^[\\w\\W]+\\.(png)$><./TestAssets>";
            string[] inputArgs = new string[] { "--images", expression };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);

            string[] images = arguments.CoverImages;
            Assert.AreEqual(2, images.Length);
            Assert.AreEqual("./TestAssets\\001.png", images[0]);
            Assert.AreEqual("./TestAssets\\002.png", images[1]);
        }

        [TestMethod]
        public void TestParseImagesWithInvalidSingleValuesReturnsFalseAndProducesParseException()
        {
            string[] inputArgs = new string[] { "--images", "missing-image" };    
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }
        #endregion

        #region Parse password
        [TestMethod]
        public void TestParsePasswordWithValidValueProducesValidResult()
        {
            string[] inputArgs = new string[] { "--password", "testing" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual("testing", arguments.Password);
        }
        #endregion

        #region Parse output
        [TestMethod]
        public void TestParseOutputFileWithValidValueProducesValidResult()
        {
            string[] inputArgs = new string[] { "--output", "testing.txt" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual("testing.txt", arguments.DecodedOutputFile);
        }
        #endregion

        #region Parsing chunkSize
        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10_000)]
        [DataRow(1_000_000)]
        public void TestParseChunkSizeWithValidValueProducesValidResult(int value)
        {
            string[] inputArgs = new string[] { "--chunkSize", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(value, arguments.ChunkByteSize);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow("test")]
        public void TestParseChunkSizeWithInvalidValueProducesFalseAndParseException(object value)
        {
            string[] inputArgs = new string[] { "--chunkSize", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());
        }
        #endregion Parsing chunkSize

        #region Parse randomSeed
        [DataTestMethod]
        [DataRow(3)]
        [DataRow(235)]
        public void TestParseRandomSeedWithValidValueProducesValidResult(int stringLength)
        {
            string seedValue = CreateString(stringLength);
            string[] inputArgs = new string[] { "--randomSeed", seedValue };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(seedValue, arguments.RandomSeed);
        }

        [DataTestMethod]
        [DataRow(2)]
        [DataRow(236)]
        public void TestParseRandomSeedWithBadStringLengthProducesFalseAndParseException(int stringLength)
        {
            string[] inputArgs = new string[] { "--randomSeed", CreateString(stringLength) };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual(typeof(ArgumentValueException), parser.LastError.InnerException.GetType());
        }

        private string CreateString(int stringLength)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < stringLength; i++)
            {
                builder.Append("0");
            }
            return builder.ToString();
        }
        #endregion Parse randomSeed

        #region Parsing enableDummies
        [TestMethod]
        public void TestParseInsertDummiesWithValidValueProducesValidResult()
        {
            string[] inputArgs = new string[] { "--enableDummies" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.InsertDummies);
        }
        #endregion Parsing enableDummies

        #region Parse deleteOriginals
        [TestMethod]
        public void TestParseDeleteOriginalsWithValidValueProducesValidResult()
        {
            string[] inputArgs = new string[] { "--deleteOriginals" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.IsTrue(arguments.DeleteAfterConversion);
        }
        #endregion Parse deleteOriginals

        #region Parse compressionLevel
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(5)]
        [DataRow(9)]
        public void TestCompressionLevelWithValidValueProducesValidResult(int value)
        {
            string[] inputArgs = new string[] { "--compressionLevel", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(value, arguments.CompressionLevel);
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(10)]
        public void TestCompressionLevelWithInvalidValueProducesFalseAndParseException(int value)
        {
            string[] inputArgs = new string[] { "--compressionLevel", value.ToString() };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out InputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }
        #endregion Parse compressionLevel
    }
}
