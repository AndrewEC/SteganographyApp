namespace SteganographyApp.Common.Arguments.Commands;

/// <summary>
/// A generic ICommandGroup initialized fom a specified name and an array of the commands to potentially be executed.
/// </summary>
/// <remarks>
/// Initializes the GenericCommandGroup.
/// </remarks>
/// <param name="commands">The array of commands to be grouped and accessed under this command.</param>
/// <param name="name">An optional name to register this group command under. If no name is provided this will
/// default to genericcommandgroup.</param>
/// <param name="helpText">An option set of text to describe the functions contained within this group
/// of commands.</param>
public class GenericCommandGroup(ICommand[] commands, string? name = null, string? helpText = null)
: BaseCommandGroup(commands)
{
    private readonly string? name = name;
    private readonly string? helpText = helpText;

    /// <summary>
    /// Returns the name provided during initialization.
    /// </summary>
    /// <returns>The name of the group command provided during initialization.</returns>
    public override string GetName() => name ?? base.GetName();

    /// <summary>
    /// Gets a line of text describing the command.
    /// </summary>
    /// <returns>A line of text describing the command.</returns>
    public override string GetHelpDescription() => helpText ?? base.GetHelpDescription();
}
