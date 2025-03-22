namespace SteganographyApp.Common.Arguments.Commands;

using System.Collections.Immutable;

/// <summary>
/// The command to be executed as part of the GenericCommand.
/// </summary>
/// <typeparam name="T">The class type the user's command line arguments will be parsed into.</typeparam>
/// <param name="args">The previously parsed arguments to execute.</param>
public delegate void CommandFunction<T>(T args);

/// <summary>
/// Provides some utility methods to allow you to more easily and concisely initialize a CliProgram.
/// </summary>
public static class Commands
{
    /// <summary>
    /// Creates a new LazyCommand instance that allows the underlying command to be
    /// lazily initialized when it is first accessed by the CliProgram.
    /// </summary>
    /// <typeparam name="T">The underlying ICommand type to reflectively instantiate. Requires the
    /// command type to provide a default constructor.</typeparam>
    /// <returns>A new LazyCommand instance.</returns>
    public static ICommand Lazy<T>()
    where T : ICommand => new LazyCommand<T>();

    /// <summary>
    /// Creates a generic GenericCommandGroup with a default name of genericcommandgroup. Useful if using a command
    /// group as the root command of a CliProgram.
    /// </summary>
    /// <param name="commands">The array of sub-commands to be selectively executed by the GenericCommandGroup command.</param>
    /// <returns>A new GenericCommandGroup instance with the default name.</returns>
    public static ICommand Group(params ICommand[] commands)
        => new GenericCommandGroup(commands);

    /// <summary>
    /// Creates a GenericCommandGroup with a specified name.
    /// </summary>
    /// <param name="name">The name of the group command.</param>
    /// <param name="commands">The array of sub-commands to selectively execute.</param>
    /// <returns>A new GenericCommandGroup instance with the specified name and sub-commands.</returns>
    public static ICommand Group(string name, params ICommand[] commands)
        => new GenericCommandGroup(commands, name);

    /// <summary>
    /// Creates a GenericCommandGroup with a specified name and help text.
    /// </summary>
    /// <param name="name">The name of the group command.</param>
    /// <param name="helpText">The help text description of the command.</param>
    /// <param name="commands">The array of sub-commands to selectively execute.</param>
    /// <returns>A new GenericCommandGroup instance with the specified name and sub-commands.</returns>
    public static ICommand Group(string name, string helpText, params ICommand[] commands)
        => new GenericCommandGroup(commands, name, helpText);
}
