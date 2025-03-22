namespace SteganographyApp.Common.Arguments.Tests;

using NUnit.Framework;
using SteganographyApp.Common.Arguments.Commands;

[TestFixture]
public class LazyCommandTest
{
    [Test]
    public void TestLazyCommand()
    {
        string[] arguments = ["arguments"];

        ICommand command = Commands.Lazy<StubCommand>();

        command.Execute(arguments);

        Assert.That(command.GetName(), Is.EqualTo(StubCommand.Name));
        Assert.That(command.GetHelpDescription(), Is.EqualTo(StubCommand.HelpDescription));
        Assert.That(StubCommand.ExecutedCalledWith, Is.EqualTo(arguments));
    }

    internal sealed class StubCommand : ICommand
    {
        public static readonly string Name = "stub_command_name";

        public static readonly string HelpDescription = "stub_help_description";

        public static string[]? ExecutedCalledWith;

        public void Execute(string[] args) => ExecutedCalledWith = args;

        public string GetHelpDescription() => HelpDescription;

        public string GetName() => Name;
    }
}