namespace SteganographyApp.Common.Arguments.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// An ICommand that is responsible for looking up and executing a single name pulled
    /// from a list of available sub-commands.
    /// </summary>
    public interface ICommandGroup : ICommand
    {
        /// <summary>
        /// Gets a list of available sub-commands to execute.
        /// </summary>
        /// <returns>The array of sub-commands to be executed.</returns>
        public ICommand[] SubCommands();
    }

    /// <summary>
    /// Provides some utility methods to allow you to more easily and concisely initialize a CliProgram.
    /// </summary>
    public static partial class Command
    {
        /// <summary>
        /// Creates a generic GenericCommandGroup with a default name of genericcommandgroup. Useful if using a command
        /// group as the root command of a CliProgram.
        /// </summary>
        /// <param name="commands">The array of sub-commands to be selectively executed by the GenericCommandGroup command.</param>
        /// <returns>A new GenericCommandGroup instance with the default name.</returns>
        public static ICommand Group(params ICommand[] commands) => new GenericCommandGroup(commands.ToImmutableArray());

        /// <summary>
        /// Creates a GenericCommandGroup with a specified name.
        /// </summary>
        /// <returns>A new GenericCommandGroup instance with the specified name and sub-commands.</returns>
        public static ICommand Group(string name, params ICommand[] commands) => new GenericCommandGroup(commands.ToImmutableArray(), name);
    }

    /// <summary>
    /// The basic abstract ICommandGroup definition that provides some reasonable default logic for validating and
    /// executing a command group.
    /// </summary>
    public abstract class BaseCommandGroup : ICommandGroup
    {
        /// <summary>
        /// Gets a list of available sub-commands to execute.
        /// </summary>
        /// <returns>The array of sub-commands to be selectively executed.</returns>
        public abstract ICommand[] SubCommands();

        /// <summary>
        /// Executes the command group. This will effectively lookup the SubCommands, determing which command needs to
        /// be executed, and provide the appropriate parameters to the command.
        /// </summary>
        /// <param name="program">The CliProgram being executed.</param>
        /// <param name="args">The array of user provided command line arguments.</param>
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

        /// <summary>
        /// Gets the name of the command. Determines what entry the user must provide for this command group to be executed.
        /// </summary>
        /// <returns>The name of the command.</returns>
        public abstract string GetName();

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
    }

    /// <summary>
    /// A generic ICommandGroup initialized fom a specified name and an array of the commands to potentially be executed.
    /// </summary>
    public class GenericCommandGroup : BaseCommandGroup
    {
        private readonly ImmutableArray<ICommand> commands;
        private readonly string name;

        /// <summary>
        /// Initializes the GenericCommandGroup.
        /// </summary>
        /// <param name="commands">The array of commands to be grouped and accessed under this command.</param>
        /// <param name="name">An optional name to register this group command under. If no name is provided this will
        /// default to genericcommandgroup.</param>
        public GenericCommandGroup(ImmutableArray<ICommand> commands, string? name = null)
        {
            this.commands = commands;
            this.name = name ?? GetType().Name.ToLowerInvariant();
        }

        /// <summary>
        /// Returns the commands provided during initialization as an array.
        /// </summary>
        /// <returns>The array of sub-command to execute.</returns>
        public override ICommand[] SubCommands() => commands.ToArray();

        /// <summary>
        /// Returns the name provided during initialization.
        /// </summary>
        /// <returns>The name of the group command provided during initialization.</returns>
        public override string GetName() => name;
    }
}