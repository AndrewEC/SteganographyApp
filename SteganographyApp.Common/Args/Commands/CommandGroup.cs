namespace SteganographyApp.Common.Arguments.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public interface ICommandGroup : ICommand
    {
        public ICommand[] SubCommands();
    }

    public abstract class BaseCommandGroup : ICommandGroup
    {
        public abstract ICommand[] SubCommands();

        public void Execute(CliProgram program, string[] args)
        {
            ICommand[] subCommands = GetSubCommands();

            if (args.Length == 0)
            {
                string expectedList = FormExpectedCommandNameList(subCommands);
                throw new CommandException($"No command found. Expected to one of: [{expectedList}]");
            }

            string nextCommandName = GetNameOfNextCommand(subCommands, args);
            ICommand? nextCommand = GetNextCommand(subCommands, nextCommandName);

            string[] nextArgs = args.Skip(1).ToArray();
            if (nextCommand == null)
            {
                string expectedList = FormExpectedCommandNameList(subCommands);
                throw new CommandException($"Could not find command with name [{nextCommandName}] to execute. Expected command to be one of: [{expectedList}]");
            }

            nextCommand.Execute(program, nextArgs);
        }

        private ICommand GetNextCommand(ICommand[] subCommands, string nextCommandName)
        {
            ICommand? nextCommand = subCommands.Where(command => command.GetName().ToLowerInvariant() == nextCommandName).FirstOrDefault();
            if (nextCommand == null)
            {
                string expectedList = FormExpectedCommandNameList(subCommands);
                throw new CommandException($"Could not find command with name [{nextCommandName}] to execute. Expected command to be one of: [{expectedList}]");
            }
            return nextCommand!;
        }

        private string GetNameOfNextCommand(ICommand[] subCommands, string[] args)
        {
            string nextCommandName = args[0].ToLowerInvariant();
            if (nextCommandName == "-h" || nextCommandName == "--help")
            {
                string expectedList = FormExpectedCommandNameList(subCommands);
                Console.WriteLine($"Specify one of the following subcommands to execute: [{expectedList}]");
                System.Environment.Exit(0);
            }
            return nextCommandName;
        }

        private ICommand[] GetSubCommands()
        {
            ICommand[] subCommands = SubCommands();
            if (subCommands.Length == 0)
            {
                throw new ParseException($"The command group [{GetName()}] could not be executed because the group does not contain any commands.");
            }

            ValidateCommandNames(subCommands);

            return subCommands;
        }

        private void ValidateCommandNames(ICommand[] commands)
        {
            List<string> names = new List<string>();
            foreach (ICommand command in commands)
            {
                string name = command.GetName();
                if (names.Contains(name))
                {
                    throw new CommandException($"Two or more commands attempted to register with the same name. Found at least two commands with the name: [{name}]");
                }
                names.Add(name);
            }
        }

        private string FormExpectedCommandNameList(ICommand[] subCommands) => string.Join(", ", subCommands.Select(command => command.GetName().ToLowerInvariant()));

        public abstract string GetName();
    }

    public class GenericCommandGroup : BaseCommandGroup
    {
        private readonly ImmutableArray<ICommand> commands;
        private readonly string name;

        public GenericCommandGroup(ImmutableArray<ICommand> commands, string? name = null)
        {
            this.commands = commands;
            this.name = name ?? GetType().Name.ToLowerInvariant();
        }    

        public override ICommand[] SubCommands() => commands.ToArray();

        public override string GetName() => name;
    }

    public static partial class Command
    {
        public static ICommand Group(params ICommand[] commands) => new GenericCommandGroup(commands.ToImmutableArray());
        public static ICommand Group(string name, params ICommand[] commands) => new GenericCommandGroup(commands.ToImmutableArray(), name);
    }
}