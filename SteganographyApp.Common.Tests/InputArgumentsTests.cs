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
        public void TestParseNullArgsThrowsException()
        {
            try
            {
                ArgumentParser.Parse(null);
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.AreEqual("No arguments provided to parse.", e.Message, MisMatchExceptionmessage);
            }
        }

        [TestMethod]
        public void TestParseWithInvalidKeyThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "test=1" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.AreEqual("An invalid key value was provided: test", e.Message, MisMatchExceptionmessage);
            }
        }

        [TestMethod]
        public void TestParseWithMissingValueThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "--name=" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.AreEqual("Value for argument --name was empty. A value must be provided for all specified arguments.", e.Message, MisMatchExceptionmessage);
            }
        }

        [TestMethod]
        public void TestParseWithMissingEqualsInArgumentThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "--name" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.AreEqual("Invalid argument --name. Did not contain required = sign.", e.Message, MisMatchExceptionmessage);
            }
        }

        [TestMethod]
        public void TestParseWithBadActionArgumentThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "--action=whatever" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.IsNotNull(e.InnerException, InnerExceptionShouldNotBeNull);
                Assert.AreEqual("Invalid value for action argument. Expected 'encode', 'decode', 'clean', 'calculate-storage-space', or 'calculate-encrypted-size' got whatever", e.InnerException.Message, MisMatchInnerExceptionMessage);
            }
        }

        [TestMethod]
        public void TestParseWithMissingArgumentsThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "--action=encode" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.AreEqual("Missing required values. Specified encode action but no file to encode was provided in arguments.", e.Message, MisMatchInnerExceptionMessage);
            }
        }

        [TestMethod]
        public void TestParseWithBadInputFileThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "--input=test!@#.png" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.AreEqual("File to decode could not be found at test!@#.png", e.Message, MisMatchInnerExceptionMessage);
            }
        }

        [TestMethod]
        public void TestWithInvalidCoverImagePathThrowsException()
        {
            try
            {
                ArgumentParser.Parse(new string[] { "--images=test!@#.png" });
                Assert.Fail(ExceptionShouldHaveBeenThrownByParse);
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ArgumentParseException), e.GetType(), ExceptionWasNotArgumentParse);
                Assert.IsNotNull(e.InnerException, InnerExceptionShouldNotBeNull);
                Assert.AreEqual("Image could not be found at test!@#.png", e.InnerException.Message, MisMatchInnerExceptionMessage);
            }
        }
    }
}
