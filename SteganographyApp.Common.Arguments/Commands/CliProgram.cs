namespace SteganographyApp.Common.Arguments.Commands;

using System;

/// <summary>
/// Create a cli program to execute a specific command or sub-program based on the user's input.
/// </summary>
public sealed class CliProgram
{
    private readonly ICommand root;

    private CliProgram(ICommand root)
    {
        this.root = root;
    }

    /// <summary>
    /// Initialize a new CliProgram instance with a root ICommand instance.
    /// </summary>
    /// <param name="root">The root command, or the first command, that will be executed by the CliProgram flow.</param>
    /// <returns>A new instance of the CliProgram with the first command to be executed being the root command parameter.</returns>
    public static CliProgram Create(ICommand root) => new(root);

    /// <summary>
    /// Executes the CliProgram.
    /// </summary>
    /// <param name="args">The array of user provided command line arguments.</param>
    public void Execute(string[] args) => root.Execute(this, args);

    /// <summary>
    /// Executes the CliProgram. This will catch and return any exception thrown during the execution of the program.
    /// </summary>
    /// <param name="args">The array of user provided command line arguments.</param>
    /// <returns>The exception thrown while executing the program. Will return null if execution was successful.</returns>
    public Exception? TryExecute(string[] args)
    {
        try
        {
            Execute(args);
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }
}