namespace SteganographyApp.Common.Tests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments;

[TestFixture]
public class SecureParserTests : FixtureWithMockConsoleReaderAndWriter
{
    [Test]
    public void ReadUserInput()
    {
        var keys = ImmutableList.Create(
            ('T', ConsoleKey.T),
            ('e', ConsoleKey.E),
            ('s', ConsoleKey.S),
            ('t', ConsoleKey.T),
            ('i', ConsoleKey.I),
            ('n', ConsoleKey.N),
            ('g', ConsoleKey.G),
            ('*', ConsoleKey.Backspace),
            ('*', ConsoleKey.Enter));

        Queue<ConsoleKeyInfo> inputQueue = MockInput.CreateInputQueue(keys);

        mockConsoleReader.Setup(reader => reader.ReadKey(true)).Returns(() => inputQueue.Dequeue());
        mockConsoleWriter.Setup(writer => writer.Write(It.IsAny<string>())).Verifiable();
        mockConsoleWriter.Setup(writer => writer.WriteLine(It.IsAny<string>())).Verifiable();

        string actual = SecureParser.ReadUserInput(string.Empty, "?");

        Assert.That(actual, Is.EqualTo("Testin"));
        mockConsoleReader.Verify(reader => reader.ReadKey(true), Times.Exactly(keys.Count));
    }

    [Test]
    public void ReadUserInputWithNonSecureResponse()
    {
        string actual = SecureParser.ReadUserInput(string.Empty, "testing");
        Assert.That(actual, Is.EqualTo("testing"));
    }
}