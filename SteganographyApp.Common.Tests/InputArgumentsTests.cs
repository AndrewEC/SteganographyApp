using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class InputArgumentsTests
    {

        private readonly string ExceptionWasNotArgumentParse = "Exception thrown should be of type ArgumentParseException.";
        private readonly string MisMatchExceptionmessage = "Exception message was not what was expected.";
        private readonly string InnerExceptionShouldNotBeNull = "Inner exception should not have been a null value.";
        private readonly string MisMatchInnerExceptionMessage = "Inner exception message was not what was expected.";
        private readonly string FalseValidatorMessage = "Invalid arguments provided. Missing Stuff";

        [TestMethod]
        public void TestParseNullArgsReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(null, out InputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
        }

        [TestMethod]
        public void TestParseWithInvalidKeyReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "test", "1" }, out InputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.AreEqual("An unrecognized argument was provided: test", parser.LastError.Message, MisMatchExceptionmessage);
        }

        [TestMethod]
        public void TestParseWithBadActionArgumentReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--action", "whatever" }, out InputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("Invalid value for action argument. Expected 'encode', 'decode', 'clean', 'calculate-storage-space', or 'calculate-encrypted-size' got whatever", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestParseWithMissingArgumentsReturnsFalse()
        {

            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--action", "encode" }, out InputArguments arguments, TestParseWithMissingArgumentsReturnsFalsePostValidator));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.AreEqual(FalseValidatorMessage, parser.LastError.Message, MisMatchInnerExceptionMessage);
        }

        private String TestParseWithMissingArgumentsReturnsFalsePostValidator(InputArguments inputs)
        {
            return "Missing Stuff";
        }

        [TestMethod]
        public void TestParseWithBadInputFileReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--input", "test!@#.png" }, out InputArguments arguments, null));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException);
            Assert.AreEqual("File to decode could not be found at test!@#.png", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestWithInvalidCoverImagePathReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--images", "test!@#.png" }, out InputArguments arguments, null));
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
            }, out InputArguments arguments, TestWithRegexGivesValidImages);
            Assert.AreEqual(2, arguments.CoverImages.Length, "Parsing regular expression should have returned 2 images.");
        }

        private string TestWithRegexGivesValidImages(InputArguments input)
        {
            return null;
        }

        [TestMethod]
        public void TestWithRegexReturnsFalse()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--images", "[r]<><>" }, out InputArguments arguments, null));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("The value supplied for the --images key is invalid. Expected the format [r]<regex><directory>", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }

        [TestMethod]
        public void TestWithRegexReturnsNoImages()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(new string[] { "--images", "[r]<^.*\\.(png|PNG)><.>" }, out InputArguments arguments, null));
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType(), ExceptionWasNotArgumentParse);
            Assert.IsNotNull(parser.LastError.InnerException, InnerExceptionShouldNotBeNull);
            Assert.AreEqual("The provided regex expression returned 0 usable files in the directory .", parser.LastError.InnerException.Message, MisMatchInnerExceptionMessage);
        }
    }
}
