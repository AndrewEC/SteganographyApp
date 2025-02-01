namespace SteganographyApp.Common.Arguments.Commands;

/// <summary>
/// Allows an underlying command to be aliased allowing said command to be executed or referenced
/// using a different name other than the one it was originally initialized with.
/// </summary>
/// <remarks>
/// Initializes the aliased command.
/// </remarks>
/// <param name="name">The new name of the command that will override the name of the original command.</param>
/// <param name="actual">The actual command that is being aliased.</param>
public class AliasedCommand(string name, ICommand actual) : ICommand
{
    private readonly string name = name;
    private readonly ICommand actual = actual;

    /// <summary>
    /// Acts as a proxy to the underlying commands Execute function.
    /// </summary>
    /// <param name="program">The CliProgram instance being executed. Using this allows a command to access
    /// additional parsers if they have been made available.</param>
    /// <param name="args">The list of user provided command line arguments to execute.</param>
    public void Execute(CliProgram program, string[] args) => actual.Execute(program, args);

    /// <summary>
    /// Gets the alias provided during initialization.
    /// </summary>
    /// <returns>The command name.</returns>
    public string GetName() => name;

    /// <summary>
    /// Gets a line of text describing the command.
    /// </summary>
    /// <returns>A line of text describing the command.</returns>
    public string GetHelpDescription() => actual.GetHelpDescription();
}
