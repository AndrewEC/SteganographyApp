using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

using Moq;
using static Moq.Times;

using NUnit.Framework;

using SteganographyApp.Common.Arguments;
using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common.Tests
{

    [TestFixture]
    public class RandomSeedParseTests : FixtureWithMockConsoleReaderAndWriter
    {

        private static readonly ImmutableList<ValueTuple<char, ConsoleKey>> TestingInputMapping = new List<ValueTuple<char, ConsoleKey>>()
        {
            ('T', ConsoleKey.T),
            ('e', ConsoleKey.E),
            ('s', ConsoleKey.S),
            ('t', ConsoleKey.T),
            ('i', ConsoleKey.I),
            ('n', ConsoleKey.N),
            ('g', ConsoleKey.G),
            ('[', ConsoleKey.Backspace),
            (']', ConsoleKey.Enter),

            ('T', ConsoleKey.T),
            ('e', ConsoleKey.E),
            ('s', ConsoleKey.S),
            ('t', ConsoleKey.T),
            ('i', ConsoleKey.I),
            ('n', ConsoleKey.N),
            ('g', ConsoleKey.G),
            ('0', ConsoleKey.Backspace),
            ('0', ConsoleKey.Enter)
        }.ToImmutableList();

        [TestCase(3)]
        [TestCase(235)]
        public void TestParseRandomSeedWithValidValue(int stringLength)
        {
            string seedValue = CreateString(stringLength);
            string[] inputArgs = new string[] { "--randomSeed", seedValue };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual(seedValue, arguments.RandomSeed);
        }

        [TestCase(2)]
        [TestCase(236)]
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

        [Test]
        public void TestParseRandomSeedWithInteractiveInput()
        {
            var queue = MockInput.CreateInputQueue(TestingInputMapping);
            var mockReader = new Mock<IConsoleReader>();
            mockReader.Setup(reader => reader.ReadKey(true)).Returns<bool>((intercept) => queue.Dequeue());

            Injector.UseInstance(mockReader.Object);
            Injector.UseInstance(new Mock<IConsoleWriter>().Object);

            string[] inputArgs = new string[] { "--randomSeed", "?" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments inputArguments, NullReturningPostValidator));

            mockReader.Verify(reader => reader.ReadKey(true), Exactly(TestingInputMapping.Count));
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual("Testin", inputArguments.RandomSeed);
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

        private string NullReturningPostValidator(IInputArguments input) => null;

    }

}