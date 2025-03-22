namespace SteganographyApp.Common.Arguments.Commands;

/// <summary>
/// The top level command interface.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Execute the current command.
    /// </summary>
    /// <param name="args">The list of user provided command line arguments to execute.</param>
    public void Execute(string[] args);

    /// <summary>
    /// The name of the command. The name specified the input the user must provide to trigger the command.
    /// </summary>
    /// <returns>The command name.</returns>
    public string GetName();

    /// <summary>
    /// Gets a line of text describing the command.
    /// </summary>
    /// <returns>A line of text describing the command.</returns>
    public string GetHelpDescription();
}