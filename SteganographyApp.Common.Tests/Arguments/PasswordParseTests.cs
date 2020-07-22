using Moq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Test;

namespace SteganographyApp.Common.Tests
{

    [TestClass]
    public class PasswordParseTests
    {
        [TestMethod]
        public void TestParsePasswordWithValidValue()
        {
            string[] inputArgs = new string[] { "--password", "testing" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual("testing", arguments.Password);
        }

        [TestMethod]
        public void TestParsePasswordWithInteractiveInput()
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

        private string NullReturningPostValidator(IInputArguments input)
        {
            return null;
        }

    }

}