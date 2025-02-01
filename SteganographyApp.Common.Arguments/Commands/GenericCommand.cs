namespace SteganographyApp.Common.Arguments.Commands;

/// <summary>
/// A high level generic command instantiated using a name and a function.
/// </summary>
/// <typeparam name="T">Specifies the type of the arguments to be passed into the Execute method.</typeparam>
/// <remarks>
/// Initializes the generic command.
/// </remarks>
/// <param name="name">The name of the command.</param>
/// <param name="function">The function, associated with the command, to execute.</param>
/// <param name="helpText">An optional parameter specifying a custom set of helpt text
/// to describe the behaviour of this command.</param>
public class GenericCommand<T>(string name, CommandFunction<T> function, string? helpText = null) : Command<T>
where T : class
{
    private readonly string name = name;
    private readonly string? helpText = helpText;
    private readonly CommandFunction<T> function = function;

    /// <summary>
    /// Invokes the command function provided during initialization.
    /// </summary>
    /// <param name="args">The arguments being executed.</param>
    public override void Execute(T args) => function(args);

    /// <summary>
    /// Returns the name of the command provided during initialization.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetName() => name;

    /// <summary>
    /// Returns either the custom help text specified during the creation of this command
    /// or the default help text.
    /// </summary>
    /// <returns>The help description text.</returns>
    public override string GetHelpDescription() => helpText ?? base.GetHelpDescription();
}
