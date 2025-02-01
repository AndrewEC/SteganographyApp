namespace SteganographyApp.Common.Arguments.Commands;

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
