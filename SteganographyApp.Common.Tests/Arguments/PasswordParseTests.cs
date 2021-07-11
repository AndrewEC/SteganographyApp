namespace SteganographyApp.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Moq;

    using NUnit.Framework;

    using SteganographyApp.Common.Arguments;
    using SteganographyApp.Common.Injection;

    using static Moq.Times;

    [TestFixture]
    public class PasswordParseTests : FixtureWithMockConsoleReaderAndWriter
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
            ('0', ConsoleKey.Enter),
        }.ToImmutableList();

        [Test]
        public void TestParsePasswordWithValidValue()
        {
            string[] inputArgs = new string[] { "--password", "testing" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments arguments, NullReturningPostValidator));
            Assert.IsNull(parser.LastError);
            Assert.AreEqual("testing", arguments.Password);
        }

        [Test]
        public void TestParsePasswordWithInteractiveInput()
        {
            var queue = MockInput.CreateInputQueue(TestingInputMapping);
            var mockReader = new Mock<IConsoleReader>();
            mockReader.Setup(reader => reader.ReadKey(true)).Returns<bool>((intercept) => queue.Dequeue());

            Injector.UseInstance(mockReader.Object);
            Injector.UseInstance(new Mock<IConsoleWriter>().Object);

            string[] inputArgs = new string[] { "--password", "?" };
            var parser = new ArgumentParser();
            Assert.IsTrue(parser.TryParse(inputArgs, out IInputArguments inputArguments, NullReturningPostValidator));

            mockReader.Verify(reader => reader.ReadKey(true), Exactly(TestingInputMapping.Count));
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual("Testin", inputArguments.Password);
        }

        private string NullReturningPostValidator(IInputArguments input) => null;
    }
}