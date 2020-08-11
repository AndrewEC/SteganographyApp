using Moq;
using NUnit.Framework;

using System;
using System.Collections.Generic;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class InputArgumentsTests : FixtureWithMockConsoleReaderAndWriter
    {

        [Test]
        public void TestPrintCommonMessageOutputsCorrectMessagesToWriter()
        {
            var lines = new List<string>();
            var mockWriter = new Mock<IConsoleWriter>();
            mockWriter.Setup(writer => writer.WriteLine(It.IsAny<string>())).Callback<string>(line => lines.Add(line));

            Injector.UseInstance(mockWriter.Object);
            Injector.UseInstance<IConsoleReader>(new NullReader());

            var inputs = new string[] { "--chunkSize", "abc" };
            var parser = new ArgumentParser();
            
            Assert.IsFalse(parser.TryParse(inputs, out IInputArguments inputArguments, (IInputArguments arguments) => null));
            parser.PrintCommonErrorMessage();

            Assert.AreEqual(4, lines.Count);
            Assert.IsTrue(lines[1].Contains("chunk size"));
        }

        [Test]
        public void TestParseNullArgumentsReturnsFalseAndProducesParseException()
        {
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(null, out IInputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.AreEqual("No arguments provided to parse.", parser.LastError.Message);
        }

        [Test]
        public void TestParseWithBadArgumentReturnsFalseAndProducesParseException()
        {
            string[] inputsArgs = new string[] { "test", "1" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputsArgs, out IInputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsTrue(parser.LastError.Message.Contains("test"));
        }

        [Test]
        public void TestParseArgumentWithMissingAssociatedValuesProducesParseException()
        {
            string[] inputArgs = new string[] { "--action" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputArgs, out IInputArguments arguments, null));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
        }

        [Test]
        public void TestParseWithPostValidationMessageProducesException()
        {
            string[] inputs = new string[] {  "--action", "encode" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputs, out IInputArguments arguments, PostValidationWithMessage));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ArgumentParseException), parser.LastError.GetType());
            Assert.IsTrue(parser.LastError.Message.Contains("Failed"));
        }

        [Test]
        public void TestParseWithPostValidationErrorProducesException()
        {
            string[] inputs = new string[] { "--action", "encode" };
            var parser = new ArgumentParser();
            Assert.IsFalse(parser.TryParse(inputs, out IInputArguments arguments, PostValidationWithError));
            Assert.IsNotNull(parser.LastError);
            Assert.AreEqual(typeof(ValidationException), parser.LastError.GetType());
            Assert.IsTrue(parser.LastError.Message.Contains("Failed Null"));
        }

        private string PostValidationWithMessage(IInputArguments input) => "Failed";

        private string PostValidationWithError(IInputArguments input)
        {
            throw new NullReferenceException("Failed Null");
        }

        private class NullReader : IConsoleReader
        {
            public ConsoleKeyInfo ReadKey(bool trap)
            {
                return new ConsoleKeyInfo('*', ConsoleKey.Backspace, false, false, false);
            }
        }

    }
}
