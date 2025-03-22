namespace SteganographyApp.Common.Arguments.Tests;

using System;
using Moq;
using NUnit.Framework;

using SteganographyApp.Common.Arguments.Commands;

[TestFixture]
public class CliProgramTests
{
    [Test]
    public void TestExecute()
    {
        string[] arguments = ["arg1"];

        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.Execute(arguments)).Verifiable();

        CliProgram.Create(mockCommand.Object).Execute(arguments);

        mockCommand.Verify(command => command.Execute(arguments), Times.Once());
    }

    [Test]
    public void TestTryExecute()
    {
        string[] arguments = ["arg1"];

        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.Execute(arguments)).Verifiable();

        CliProgram.Create(mockCommand.Object).TryExecute(arguments);

        mockCommand.Verify(command => command.Execute(arguments), Times.Once());
    }

    [Test]
    public void TestTryExecuteWithError()
    {
        string[] arguments = ["arg1"];

        Exception expected = new();
        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.Execute(arguments)).Throws(expected);

        Exception? actual = CliProgram.Create(mockCommand.Object).TryExecute(arguments);

        Assert.That(actual, Is.EqualTo(expected));
    }
}