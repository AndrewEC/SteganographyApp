namespace SteganographyApp.Common.Arguments.Commands;

/// <summary>
/// Provides some utility methods to allow you to more easily and concisely initialize a CliProgram.
/// </summary>
public static class Commands
{
    /// <summary>
    /// Creates a new <see cref="LazyCommand{T}"/> instance that allows the underlying command to be
    /// lazily initialized when it is first accessed by the CliProgram.
    /// </summary>
    /// <typeparam name="T">The underlying <see cref="ICommand"/> type to reflectively instantiate. Requires the
    /// command type to provide a default constructor.</typeparam>
    /// <returns>A new <see cref="LazyCommand{T}"/> instance.</returns>
    public static ICommand Lazy<T>()
    where T : ICommand => new LazyCommand<T>();

    /// <summary>
    /// Creates a generic command a default name of genericcommandgroup. Useful if using a command
    /// group as the root command of a <see cref="CliProgram"/> where the command name does not matter.
    /// </summary>
    /// <param name="commands">The array of sub-commands to be selectively executed by command group.</param>
    /// <returns>A new command group with the default name.</returns>
    public static ICommand Group(params ICommand[] commands)
        => new GenericCommandGroup(commands);

    /// <summary>
    /// Creates a generic command with a specified name.
    /// </summary>
    /// <param name="name">The name of the group command.</param>
    /// <param name="commands">The array of sub-commands to selectively execute.</param>
    /// <returns>A new command group with the specified name.</returns>
    public static ICommand Group(string name, params ICommand[] commands)
        => new GenericCommandGroup(commands, name);

    /// <summary>
    /// Creates a generic command with a specified name and help text.
    /// </summary>
    /// <param name="name">The name of the group command.</param>
    /// <param name="helpText">The help text description of the command.</param>
    /// <param name="commands">The array of sub-commands to selectively execute.</param>
    /// <returns>A new generic command with the specified name, help text, and sub-commands.</returns>
    public static ICommand Group(string name, string helpText, params ICommand[] commands)
        => new GenericCommandGroup(commands, name, helpText);
}
