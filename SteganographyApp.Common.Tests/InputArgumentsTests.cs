using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Test;
using Moq;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class InputArgumentsTests
    {

        private string NullReturningPostValidator(IInputArguments input)
        {
            return null;
        }

        #region Error Messaging Output
        [TestMethod]
        public void TestPrintCommonMessageOutputsCorrectMessagesToWriter()
        {
            var lines = new List<string>();
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(writer => writer.WriteLine(It.IsAny<string>())).Callback<string>(line => lines.Add(line));

            var inputs = new string[] { "--chunkSize", "abc" };
            var parser = new ArgumentParser(new NullReader(), mockWriter.Object);
            Assert.IsFalse(parser.TryParse(inputs, out IInputArguments inputArguments, NullReturningPostValidator));
            parser.PrintCommonErrorMessage();

            Assert.AreEqual(4, lines.Count);
            Assert.IsTrue(lines[1].Contains("chunk size"));
        }

        private class NullReader : IReader
        {
            public ConsoleKeyInfo ReadKey(bool trap)
            {
                return new ConsoleKeyInfo('*', ConsoleKey.Backspace, false, false, false);
            }
        }
        #endregion

        #region General Parsing
        [TestMethod]
        public void TestParseNullArgumentsReturnsFalseAndProducesParseException()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(null, out IInputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.AreEqual("No arguments provided to parse.", parser.LastError.Message);
        }

        [TestMethod]
        public void TestParseWithBadArgumentReturnsFalseAndProducesParseException()
        {
            string[] inputsArgs = new string[] { "test", "1" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputsArgs, out IInputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsTrue(parser.LastError.Message.Contains("test"));
        }

        [TestMethod]
        public void TestParseArgumentWithMissingAssociatedValuesProducesParseException()
        {
            string[] inputArgs = new string[] { "--action" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }

        [TestMethod]
        public void TestParseWithPostValidationMessageProducesException()
        {
            string[] inputs = new string[] {  "--action", "encode" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputs, out IInputArguments arguments, PostValidationWithMessage));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsTrue(parser.LastError.Message.Contains("Failed"));
        }

        [TestMethod]
        public void TestParseWithPostValidationErrorProducesException()
        {
            string[] inputs = new string[] { "--action", "encode" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputs, out IInputArguments arguments, PostValidationWithError));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ValidationException), parser.LastError.GetType());
            Assert.IsTrue(parser.LastError.Message.Contains("Failed Null"));
        }

        private string PostValidationWithMessage(IInputArguments input)
        {
            return "Failed";
        }

        private string PostValidationWithError(IInputArguments input)
        {
            throw new NullReferenceException("Failed Null");
        }
        #endregion General Parsing

        #region Parsing action
        [DataTestMethod]
        [DataRow("encode", ActionEnum.Encode)]
        [DataRow("decode", ActionEnum.Decode)]
        [DataRow("clean", ActionEnum.Clean)]
        [DataRow("convert", ActionEnum.Convert)]
        [DataRow("calculate-storage-space", ActionEnum.CalculateStorageSpace)]
        [DataRow("calculate-encrypted-size", ActionEnum.CalculateEncryptedSize)]
        public void TestParseActionWithValidValuesProducesValidResult(string actionString, ActionEnum action)
        {
            string[] inputArgs = new string[] { "--action", actionString };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(action, arguments.EncodeOrDecode);
        }

        [TestMethod]
        public void TestParseActionWithInvalidActionReturnsFalseAndProducesParseException()
        {
            string[] inputArgs = new string[] { "--action", "invalid-action" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(path, arguments.FileToEncode);
        }

        [TestMethod]
        public void TestFileToEncodeWithInvalidPathProducesFalseAndparseException()
        {
            string path = "TestAssets/FileDoesntExist";
            string[] inputArgs = new string[] { "--input", path };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual("testing", arguments.Password);
        }

        [TestMethod]
        public void TestParsePasswordWithInteractiveInputProducesValidResult()
        {
            string[] inputArgs = new string[] { "--password", "?" };

            var queue = CreateUserInputQueue();

            var mockReader = new Mock<IReader>();
            mockReader.Setup(reader => reader.ReadKey(It.IsAny<bool>())).Returns<bool>((intercept) => queue.Dequeue());

            var parser = new ArgumentParser(mockReader.Object, new NullWriter());
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments inputArguments, NullReturningPostValidator));

            mockReader.Verify(reader => reader.ReadKey(It.IsAny<bool>()), Times.Exactly(9));
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual("Testin", inputArguments.Password);
        }

        private Queue<ConsoleKeyInfo> CreateUserInputQueue()
        {
            var queue = new Queue<ConsoleKeyInfo>();
            queue.Enqueue(new ConsoleKeyInfo('T', ConsoleKey.T, true, false, false));
            queue.Enqueue(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('n', ConsoleKey.N, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('g', ConsoleKey.G, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('*', ConsoleKey.Backspace, false, false, false));
            queue.Enqueue(new ConsoleKeyInfo('*', ConsoleKey.Enter, false, false, false));
            return queue;
        }

        public class NullWriter : IWriter
        {
            public void Write(string line) {}
            public void WriteLine(string line) {}
        }
        #endregion

        #region Parse output
        [TestMethod]
        public void TestParseOutputFileWithValidValueProducesValidResult()
        {
            string[] inputArgs = new string[] { "--output", "testing.txt" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
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
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }
        #endregion Parse compressionLevel
    }
}
