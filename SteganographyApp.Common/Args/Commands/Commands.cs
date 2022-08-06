namespace SteganographyApp.Common.Arguments.Commands
{
    using System;

    public interface ICommand
    {
        public void Execute(CliProgram program, string[] args);
        public string GetName();
    }

    public abstract class BaseCommand<T> : ICommand where T : class
    {
        public void Execute(CliProgram program, string[] args)
        {
            Execute(CliParser.ParseArgs<T>(args, program.AdditionalParsers));
        }

        public abstract void Execute(T args);

        public virtual string GetName() => GetType().Name.ToLowerInvariant();
    }

    public delegate void CommandFunction<T>(T args);

    public class GenericCommand<T> : BaseCommand<T> where T : class
    {
        private readonly string name;
        private readonly CommandFunction<T> function;

        public GenericCommand(string name, CommandFunction<T> function)
        {
            this.name = name;
            this.function = function;
        }

        public override void Execute(T args)
        {
            function(args);
        }

        public override string GetName() => name;
    }

    public class AliasedCommand : ICommand
    {
        private readonly string name;
        private readonly ICommand actual;

        public AliasedCommand(string name, ICommand actual)
        {
            this.name = name;
            this.actual = actual;
        }

        public void Execute(CliProgram program, string[] args)
        {
            actual.Execute(program, args);
        }

        public string GetName() => name;
    }

    public class LaterCommand<T> : ICommand where T : ICommand
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

        public void Execute(CliProgram program, string[] args)
        {
            Actual.Execute(program, args);
        }

        public string GetName() => Actual.GetName();
    }

    public static partial class Command
    {
        public static ICommand From<T>(string name, CommandFunction<T> function) where T : class => new GenericCommand<T>(name.ToLowerInvariant(), function);
        public static ICommand Alias(string name, ICommand command) => new AliasedCommand(name, command);
        public static ICommand Later<T>() where T : ICommand => new LaterCommand<T>();
    }
}