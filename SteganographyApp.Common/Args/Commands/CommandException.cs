namespace SteganographyApp.Common.Arguments.Commands
{
    using System;

    public sealed class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}