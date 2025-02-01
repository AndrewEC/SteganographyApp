namespace SteganographyApp.Common.Arguments.Commands;

using System;

using SteganographyApp.Common.Arguments.Parsers;

/// <summary>
/// Create a cli program to execute a specific command or sub-program based on the user's input.
/// </summary>
public sealed class CliProgram
{
    private readonly ICommand root;
    private IParserFunctionProvider? additionalParsers;

    private CliProgram(ICommand root)
    {
        this.root = root;
    }

    /// <summary>
    /// Gets the optionally available list of additional parsers that can be configured to parse specific types.
    /// </summary>
    public IParserFunctionProvider? AdditionalParsers
    {
        get => additionalParsers;
    }

    /// <summary>
    /// Initialize a new CliProgram instance with a root ICommand instance.
    /// </summary>
    /// <param name="root">The root command, or the first command, that will be executed by the CliProgram flow.</param>
    /// <returns>A new instance of the CliProgram with the first command to be executed being the root command parameter.</returns>
    public static CliProgram Create(ICommand root) => new(root);

    /// <summary>
    /// Allow the CliProgram to execute with an optional set of argument parsers.
    /// </summary>
    /// <param name="additionalParsers">The object that can provide an additional set of parsers for custom types beyond
    /// the default set of implicitly provided parsers.</param>
    /// <returns>The current CliProgram instance.</returns>
    public CliProgram WithAdditionalParsers(IParserFunctionProvider additionalParsers)
    {
        this.additionalParsers = additionalParsers;
        return this;
    }

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