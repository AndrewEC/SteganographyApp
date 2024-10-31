namespace SteganographyApp.Common.Arguments.Commands;

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
public static partial class Commands
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
    /// <param name="name">The name of the group command.</param>
    /// <param name="commands">The array of sub-commands to selectively execute.</param>
    /// <returns>A new GenericCommandGroup instance with the specified name and sub-commands.</returns>
    public static ICommand Group(string name, params ICommand[] commands) => new GenericCommandGroup(commands.ToImmutableArray(), name);

    /// <summary>
    /// Creates a GenericCommandGroup with a specified name and help text.
    /// </summary>
    /// <param name="name">The name of the group command.</param>
    /// <param name="helpText">The help text description of the command.</param>
    /// <param name="commands">The array of sub-commands to selectively execute.</param>
    /// <returns>A new GenericCommandGroup instance with the specified name and sub-commands.</returns>
    public static ICommand Group(string name, string helpText, params ICommand[] commands) => new GenericCommandGroup(commands.ToImmutableArray(), name, helpText);
}

/// <summary>
/// The basic abstract ICommandGroup definition that provides some reasonable default logic for validating and
/// executing a command group.
/// </summary>
public abstract class BaseCommandGroup : ICommandGroup
{
    /// <summary>
    /// The array of commands to conditionally execute.
    /// </summary>
    private readonly ICommand[] subCommands;

    /// <summary>
    /// Initializes the command group with the array of subcommands. This will
    /// also validate that the subcommands contain the correct names (none can be duplicates).
    /// </summary>
    /// <param name="subCommands">The array of commands to conditionally execute.</param>
    protected BaseCommandGroup(ICommand[] subCommands)
    {
        ValidateCommandNames(subCommands);
        this.subCommands = subCommands;
    }

    /// <summary>
    /// Gets a list of available sub-commands to execute.
    /// </summary>
    /// <returns>The array of sub-commands to be executed.</returns>
    public ICommand[] SubCommands() => (ICommand[])subCommands.Clone();

    /// <summary>
    /// Executes the command group. This will effectively lookup the SubCommands, determing which command needs to
    /// be executed, and provide the appropriate parameters to the command.
    /// </summary>
    /// <param name="program">The CliProgram being executed.</param>
    /// <param name="args">The array of user provided command line arguments.</param>
    public void Execute(CliProgram program, string[] args)
    {
        if (args.Length == 0)
        {
            string expectedList = FormExpectedCommandNameList(subCommands);
            throw new CommandException($"No command found. Expected one of: [{expectedList}]");
        }

        ICommand nextCommand = GetNextCommand(args);

        string[] nextArgs = args.Skip(1).ToArray();

        nextCommand.Execute(program, nextArgs);
    }

    /// <summary>
    /// Gets the name of the command. Determines what entry the user must provide for this command group to be executed.
    /// </summary>
    /// <returns>The name of the command.</returns>
    public abstract string GetName();

    /// <summary>
    /// Returns a default empty string.
    /// </summary>
    /// <returns>An empty string.</returns>
    public virtual string GetHelpDescription() => string.Empty;

    private static string FormExpectedCommandNameList(ICommand[] subCommands) => string.Join(", ", subCommands.Select(command => command.GetName().ToLowerInvariant()));

    private static bool WasHelpRequested(string nextCommandName) => nextCommandName == "-h" || nextCommandName == "--help";

    private ICommand GetNextCommand(string[] args)
    {
        string nextCommandName = args[0].ToLowerInvariant();
        if (WasHelpRequested(nextCommandName))
        {
            PrintHelpText();
        }

        ICommand? nextCommand = subCommands.Where(command => command.GetName().Equals(nextCommandName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        if (nextCommand == null)
        {
            string expectedList = FormExpectedCommandNameList(subCommands);
            throw new CommandException($"Could not find command with name [{nextCommandName}] to execute. Expected command to be one of: [{expectedList}]");
        }
        return nextCommand!;
    }

    private void PrintHelpText()
    {
        Console.WriteLine("Commands:");
        foreach (ICommand command in subCommands)
        {
            string helpText = command.GetHelpDescription();
            if (helpText == string.Empty)
            {
                Console.WriteLine($"  {command.GetName()}");
            }
            else
            {
                Console.WriteLine($"  {command.GetName()} - {command.GetHelpDescription()}");
            }
        }
        Environment.Exit(0);
    }

    private void ValidateCommandNames(ICommand[] commands)
    {
        if (commands.Length == 0)
        {
            throw new CommandException($"At least one command must be registered in a command group. Group [{GetName()}] has no commands.");
        }

        HashSet<string> names = [];
        foreach (ICommand command in commands)
        {
            string name = command.GetName().ToLowerInvariant();
            if (!names.Add(name))
            {
                throw new CommandException($"Two or more commands attempted to register with the same name within the command group: [{GetName()}]. Found at least two commands with the name: [{name}].");
            }
        }
    }
}

/// <summary>
/// A generic ICommandGroup initialized fom a specified name and an array of the commands to potentially be executed.
/// </summary>
public class GenericCommandGroup : BaseCommandGroup
{
    private readonly string name;
    private readonly string helpText;

    /// <summary>
    /// Initializes the GenericCommandGroup.
    /// </summary>
    /// <param name="commands">The array of commands to be grouped and accessed under this command.</param>
    /// <param name="name">An optional name to register this group command under. If no name is provided this will
    /// default to genericcommandgroup.</param>
    /// <param name="helpText">An option set of text to describe the functions contained within this group
    /// of commands.</param>
    public GenericCommandGroup(ImmutableArray<ICommand> commands, string? name = null, string? helpText = null)
    : base(commands.ToArray())
    {
        this.name = name ?? GetType().Name.ToLowerInvariant();
        this.helpText = helpText ?? string.Empty;
    }

    /// <summary>
    /// Returns the name provided during initialization.
    /// </summary>
    /// <returns>The name of the group command provided during initialization.</returns>
    public override string GetName() => name;

    /// <summary>
    /// Gets a line of text describing the command.
    /// </summary>
    /// <returns>A line of text describing the command.</returns>
    public override string GetHelpDescription() => helpText;
}