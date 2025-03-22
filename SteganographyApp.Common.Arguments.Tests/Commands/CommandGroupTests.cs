namespace SteganographyApp.Common.Arguments.Tests;

using Moq;
using NUnit.Framework;
using SteganographyApp.Common.Arguments.Commands;

[TestFixture]
public class CommandGroupTests
{
    [Test]
    public void TestCommandGroup()
    {
        string[] arguments = ["command_name", "command_argument"];

        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.GetName()).Returns("command_name");
        mockCommand.Setup(command => command.Execute(It.Is<string[]>(str => str.Length == 1 && str[0] == arguments[1])))
            .Verifiable();

        ICommand group = Commands.Group([mockCommand.Object]);

        group.Execute(arguments);
    }

    [Test]
    public void TestCommandGroupWithUnknownCommandNameThrowsException()
    {
        string[] arguments = ["command_name1", "command_argument"];

        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.GetName()).Returns("command_name");

        ICommand group = Commands.Group([mockCommand.Object]);

        CommandException actual = Assert.Throws<CommandException>(
            () => group.Execute(arguments));

        Assert.That(
            actual.Message,
            Contains.Substring("Could not find command with name [command_name1] to execute. Expected command to be one of: [command_name]"));
    }

    [Test]
    public void TestCommandGroupWithNoCommandNameInArgumentsThrowsException()
    {
        string[] arguments = [];

        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.GetName()).Returns("command_name");

        ICommand group = Commands.Group([mockCommand.Object]);

        CommandException actual = Assert.Throws<CommandException>(
            () => group.Execute(arguments));

        Assert.That(
            actual.Message,
            Contains.Substring("No command found. Expected one of: [command_name]"));
    }

    [Test]
    public void TestCommandGroupWithNoSubCommandsThrowsException()
    {
        ICommand[] command = [];
        CommandException actual = Assert.Throws<CommandException>(
            () => Commands.Group(command));

        Assert.That(
            actual.Message,
            Contains.Substring("At least one command must be registered in a command group."));
    }

    [Test]
    public void TestCommandGroupWithDuplicateSubCommandNamesThrowsException()
    {
        Mock<ICommand> mockCommand = new(MockBehavior.Strict);
        mockCommand.Setup(command => command.GetName()).Returns("command_name");

        CommandException actual = Assert.Throws<CommandException>(
            () => Commands.Group(mockCommand.Object, mockCommand.Object));

        Assert.That(
            actual.Message,
            Contains.Substring("All commands in a group must have a unique name."));
    }
}