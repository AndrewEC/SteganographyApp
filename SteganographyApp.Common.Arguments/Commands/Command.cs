namespace SteganographyApp.Common.Arguments.Commands;

/// <summary>
/// A high level abstract command that provides some reasonable default logic to execute a single command.
/// </summary>
/// <typeparam name="T">The type which the args from the root Execute command will be parsed to using the CliParser.</typeparam>
public abstract class Command<T> : ICommand
where T : class
{
    /// <summary>
    /// Parses the user provided arguments into an object of type T and passes it to the abstract
    /// Execute function.
    /// </summary>
    /// <param name="program">The CliProgram being executed.</param>
    /// <param name="args">The array of user provided command line arguments.</param>
    public void Execute(CliProgram program, string[] args)
        => Execute(CliParser.ParseArgs<T>(args));

    /// <summary>
    /// Execute the command providing some parsed argument object as input.
    /// </summary>
    /// <param name="args">An object containing the parsed user provided command line arguments.</param>
    public abstract void Execute(T args);

    /// <summary>
    /// A default implementation for the GetName function that will return the lower-case class name.
    /// </summary>
    /// <returns>The command name.</returns>
    public virtual string GetName() => GetType().Name.ToLowerInvariant();

    /// <summary>
    /// Returns the help text description pulled from the ProgramDescriptor attribute on the input
    /// CLI model specified in the generic type argument T.
    /// </summary>
    /// <returns>The help text pulled from the ProgramDescriptor.</returns>
    public virtual string GetHelpDescription() => Help.GetCommandDescription(typeof(T));
}
