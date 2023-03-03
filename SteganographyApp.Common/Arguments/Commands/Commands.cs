namespace SteganographyApp.Common.Arguments.Commands
{
    using System;

    /// <summary>
    /// The command to be executed as part of the GenericCommand.
    /// </summary>
    /// <typeparam name="T">The class type the user's command line arguments will be parsed into.</typeparam>
    /// <param name="args">The previously parsed arguments to execute.</param>
    public delegate void CommandFunction<T>(T args);

    /// <summary>
    /// The top level command interface.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Execute the current command.
        /// </summary>
        /// <param name="program">The CliProgram instance being executed. Using this allows a command to access
        /// additional parsers if they have been made available.</param>
        /// <param name="args">The list of user provided command line arguments to execute.</param>
        public void Execute(CliProgram program, string[] args);

        /// <summary>
        /// The name of the command. The name specified the input the user must provide to trigger the command.
        /// </summary>
        /// <returns>The command name.</returns>
        public string GetName();
    }

    /// <summary>
    /// Provides some utility methods to allow you to more easily and concisely initialize a CliProgram.
    /// </summary>
    public static partial class Commands
    {
        /// <summary>
        /// Initializes a GenericCommand instance from the specified name and CommandFunction.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="function">The function that will be executed when the command is executed.</param>
        /// <typeparam name="T">The type of the argument class.</typeparam>
        /// <returns>A new GenericCommand instance initialized with the specified name and function.</returns>
        public static ICommand From<T>(string name, CommandFunction<T> function) where T : class => new GenericCommand<T>(name.ToLowerInvariant(), function);

        /// <summary>
        /// Creates a new AliasedCommand instance allowing the existing command to be executed using a different name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="command">The underlying command to execute.</param>
        /// <returns>A new AliasedCommand instance.</returns>
        public static ICommand Alias(string name, ICommand command) => new AliasedCommand(name, command);

        /// <summary>
        /// Creates a new LazyCommand instance that allows the underlying command to be lazily initialized when it is first accessed by the CliProgram.
        /// </summary>
        /// <typeparam name="T">The underlying ICommand type to reflectively instantiate. Requires the command type to provide
        /// a default constructor.</typeparam>
        /// <returns>A new LazyCommand instance.</returns>
        public static ICommand Lazy<T>() where T : ICommand => new LazyCommand<T>();
    }

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
        {
            Execute(CliParser.ParseArgs<T>(args, program.AdditionalParsers));
        }

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
    }

    /// <summary>
    /// A high level generic command instantiated using a name and a function.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the arguments to be passed into the Execute method.</typeparam>
    public class GenericCommand<T> : Command<T>
    where T : class
    {
        private readonly string name;
        private readonly CommandFunction<T> function;

        /// <summary>
        /// Initializes the generic command.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="function">The function, associated with the command, to execute.</param>
        public GenericCommand(string name, CommandFunction<T> function)
        {
            this.name = name;
            this.function = function;
        }

        /// <summary>
        /// Invokes the command function provided during initialization.
        /// </summary>
        /// <param name="args">The arguments being executed.</param>
        public override void Execute(T args)
        {
            function(args);
        }

        /// <summary>
        /// Returns the name of the command provided during initialization.
        /// </summary>
        /// <returns>The command name.</returns>
        public override string GetName() => name;
    }

    /// <summary>
    /// Allows an underlying command to be aliased allowing said command to be executed or referenced
    /// using a different name other than the one it was originally initialized with.
    /// </summary>
    public class AliasedCommand : ICommand
    {
        private readonly string name;
        private readonly ICommand actual;

        /// <summary>
        /// Initializes the aliased command.
        /// </summary>
        /// <param name="name">The new name of the command that will override the name of the original command.</param>
        /// <param name="actual">The actual command that is being aliased.</param>
        public AliasedCommand(string name, ICommand actual)
        {
            this.name = name;
            this.actual = actual;
        }

        /// <summary>
        /// Acts as a proxy to the underlying commands Execute function.
        /// </summary>
        /// <param name="program">The CliProgram instance being executed. Using this allows a command to access
        /// additional parsers if they have been made available.</param>
        /// <param name="args">The list of user provided command line arguments to execute.</param>
        public void Execute(CliProgram program, string[] args)
        {
            actual.Execute(program, args);
        }

        /// <summary>
        /// Gets the alias provided during initialization.
        /// </summary>
        /// <returns>The command name.</returns>
        public string GetName() => name;
    }

    /// <summary>
    /// A command that will lazily instantiate and proxy all method calls to an underlying command type.
    /// This allows one to provide a command to a CliProgram for execution without needing to initialize the
    /// command up-front.
    /// </summary>
    /// <typeparam name="T">The type of the underlying command being lazily proxies.</typeparam>
    public class LazyCommand<T> : ICommand
    where T : ICommand
    {
        private ICommand? actual;

        private ICommand Actual
        {
            get
            {
                if (actual == null)
                {
                    actual = Activator.CreateInstance(typeof(T)) as ICommand;
                }
                return actual!;
            }
        }

        /// <summary>
        /// Proxies the call to the underlying command instance.
        /// </summary>
        /// <param name="program">The CliProgram instance being executed.</param>
        /// <param name="args">The array of user provided command line arguments.</param>
        public void Execute(CliProgram program, string[] args)
        {
            Actual.Execute(program, args);
        }

        /// <summary>
        /// Returns the name from the underlying command being proxies.
        /// </summary>
        /// <returns>The command name.</returns>
        public string GetName() => Actual.GetName();
    }
}