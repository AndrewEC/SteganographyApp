using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class InputArgumentsTests
    {

        private readonly string ExceptionShouldHaveBeenThrownByParse = "An exception should have been thrown by the parse method call.";
        private readonly string ExceptionWasNotArgumentParse = "Exception thrown should be of type ArgumentParseException.";
        private readonly string MisMatchExceptionmessage = "Exception message was not what was expected.";
        private readonly string InnerExceptionShouldNotBeNull = "Inner exception should not have been a null value.";
        private readonly string MisMatchInnerExceptionMessage = "Inner exception message was not what was expected.";

        [TestMethod]
        public void TestParseNullArgsReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(null, out InputArguments arguments));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
        }

        [TestMethod]
        public void TestParseWithInvalidKeyReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "test", "1" }, out InputArguments arguments));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.AreEqual("An unrecognized argument was provided: test", parser.LastError.Message, MisMatchExceptionmessage);
        }

        [TestMethod]
        public void TestParseWithBadActionArgumentReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--action", "whatever" }, out InputArguments arguments));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("Invalid value for action argument. Expected 'encode', 'decode', 'clean', 'calculate-storage-space', or 'calculate-encrypted-size' got whatever", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestParseWithMissingArgumentsReturnsFalse()
        {

            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--action", "encode" }, out InputArguments arguments));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.AreEqual("Missing required values. Specified encode action but no file to encode was provided in arguments.", parser.LastError.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestParseWithBadInputFileReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--input", "test!@#.png" }, out InputArguments arguments));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual("File to decode could not be found at test!@#.png", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestWithInvalidCoverImagePathReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--images", "test!@#.png" }, out InputArguments arguments));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("Image could not be found at test!@#.png", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestWithRegexGivesValidImages()
        {
            new ArgumentParser().TryParse(new string[] {
                "--images", "[r]<^.*\\.(png|PNG)><./TestAssets/>",
                "--action", "encode",
                "--input", "./TestAssets/test.zip"
            }, out InputArguments arguments);
            Assert.AreEqual(2, arguments.CoverImages.Length, "Parsing regular expression should have returned 2 images.");
        }

        [TestMethod]
        public void TestWithRegexReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--images", "[r]<><>" }, out InputArguments arguments));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("The value supplied for the --images key is invalid. Expected the format [r]<regex><directory>", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestWithRegexReturnsNoImages()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--images", "[r]<^.*\\.(png|PNG)><.>" }, out InputArguments arguments));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("The provided regex expression returned 0 usable files in the directory .", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }
    }
}
