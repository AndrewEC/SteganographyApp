namespace SteganographyApp.Common.Arguments.Commands;

using System;

/// <summary>
/// A command that will lazily instantiate and proxy all method calls to an underlying command type.
/// This allows one to provide a command to a CliProgram for execution without needing to initialize the
/// command up-front.
/// </summary>
/// <typeparam name="T">The type of the underlying command being lazily proxies.</typeparam>
public class LazyCommand<T> : ICommand
where T : ICommand
{
    private ICommand? actual;

    // Stryker disable all
    private ICommand Actual
    {
        get => (actual ??= Activator.CreateInstance(typeof(T)) as ICommand)!;
    }

    // Stryker restore all

    /// <summary>
    /// Proxies the call to the underlying command instance.
    /// </summary>
    /// <param name="args">The array of user provided command line arguments.</param>
    public void Execute(string[] args) => Actual.Execute(args);

    /// <summary>
    /// Returns the name from the underlying command being proxies.
    /// </summary>
    /// <returns>The command name.</returns>
    public string GetName() => Actual.GetName();

    /// <summary>
    /// Gets a line of text describing the command.
    /// </summary>
    /// <returns>A line of text describing the command.</returns>
    public string GetHelpDescription() => Actual.GetHelpDescription();
}
