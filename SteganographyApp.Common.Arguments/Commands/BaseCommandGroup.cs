namespace SteganographyApp.Common.Arguments.Commands;

using System;
using System.Collections.Generic;
using System.Linq;

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
    /// <param name="args">The array of user provided command line arguments.</param>
    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            string expectedList = FormExpectedCommandNameList(subCommands);
            throw new CommandException($"No command found. Expected one of: [{expectedList}]");
        }

        ICommand nextCommand = GetNextCommand(args);

        string[] nextArgs = args.Skip(1).ToArray();

        nextCommand.Execute(nextArgs);
    }

    /// <summary>
    /// Gets the name of the command. Determines what entry the user must provide for this command group to be executed.
    /// </summary>
    /// <returns>The name of the command.</returns>
    public virtual string GetName() => GetType().Name.ToLowerInvariant();

    /// <summary>
    /// Returns a default empty string.
    /// </summary>
    /// <returns>An empty string.</returns>
    public virtual string GetHelpDescription()
        => string.Format("Run one of the following commands: {0}", FormExpectedCommandNameList(subCommands));

    private static string FormExpectedCommandNameList(ICommand[] subCommands)
        => string.Join(", ", subCommands.Select(GetCommandName));

    private static bool WasHelpRequested(string nextCommandName)
        => nextCommandName == "-h" || nextCommandName == "--help";

    private static string GetCommandName(ICommand command)
        => command.GetName().Trim().ToLowerInvariant();

    private ICommand GetNextCommand(string[] args)
    {
        string nextCommandName = args[0].ToLowerInvariant();
        if (WasHelpRequested(nextCommandName))
        {
            PrintHelpText();
        }

        ICommand? nextCommand = subCommands
            .Where(command => GetCommandName(command).Equals(nextCommandName))
            .FirstOrDefault();
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
                Console.WriteLine($"  {GetCommandName(command)}");
            }
            else
            {
                Console.WriteLine($"  {GetCommandName(command)} - {command.GetHelpDescription()}");
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
            string name = GetCommandName(command);
            if (!names.Add(name))
            {
                throw new CommandException($"All commands in a group must have a unique name. Group [{GetName()}] has multiple commands named: [{name}].");
            }
        }
    }
}