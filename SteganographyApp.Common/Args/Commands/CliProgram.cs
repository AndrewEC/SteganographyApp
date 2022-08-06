namespace SteganographyApp.Common.Arguments.Commands
{
    using System;

    public sealed class CliProgram
    {
        private readonly ICommand root;
        private IParserProvider? additionalParsers;
        public IParserProvider? AdditionalParsers
        {
            get => additionalParsers;
        }

        private CliProgram(ICommand root)
        {
            this.root = root;
        }

        public static CliProgram Create(ICommand root) => new CliProgram(root);

        public CliProgram WithParsers(IParserProvider additionalParsers)
        {
            this.additionalParsers = additionalParsers;
            return this;
        }

        public void Execute(string[] args)
        {
            this.root.Execute(this, args);
        }

        public Exception? TryExecute(string[] args)
        {
            try
            {
                Execute(args);
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
    }
}