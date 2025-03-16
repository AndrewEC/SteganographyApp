namespace SteganographyApp.Common.Tests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.Parsers;

[TestFixture]
public class SecureParserTests
{
    private readonly Mock<IConsoleReader> mockConsoleReader = new(MockBehavior.Strict);
    private readonly Mock<IConsoleWriter> mockConsoleWriter = new(MockBehavior.Strict);

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

        Queue<ConsoleKeyInfo> inputQueue = CreateInputQueue(keys);

        mockConsoleReader.Setup(reader => reader.ReadKey(true)).Returns(() => inputQueue.Dequeue());
        mockConsoleWriter.Setup(writer => writer.Write(It.IsAny<string>())).Verifiable();
        mockConsoleWriter.Setup(writer => writer.WriteLine(It.IsAny<string>())).Verifiable();

        string actual = SecureParser.ReadUserInput(string.Empty, "?", mockConsoleReader.Object, mockConsoleWriter.Object);

        Assert.That(actual, Is.EqualTo("Testin"));
        mockConsoleReader.Verify(reader => reader.ReadKey(true), Times.Exactly(keys.Count));
    }

    [Test]
    public void ReadUserInputWithNonSecureResponse()
    {
        string actual = SecureParser.ReadUserInput(string.Empty, "testing");
        Assert.That(actual, Is.EqualTo("testing"));
    }

    private static Queue<ConsoleKeyInfo> CreateInputQueue(ImmutableList<(char KeyChar, ConsoleKey Key)> inputMapping)
    {
        var queue = new Queue<ConsoleKeyInfo>();
        for (int i = 0; i < inputMapping.Count; i++)
        {
            (char keyChar, ConsoleKey key) = inputMapping[i];
            if (keyChar >= 65)
            {
                queue.Enqueue(new ConsoleKeyInfo(keyChar, key, true, false, false));
                continue;
            }

            queue.Enqueue(new ConsoleKeyInfo(keyChar, key, false, false, false));
        }

        return queue;
    }
}