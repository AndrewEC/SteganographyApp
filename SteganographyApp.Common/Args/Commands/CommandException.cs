namespace SteganographyApp.Common.Arguments.Commands
{
    using System;

    /// <summary>
    /// A high level exception thrown if there was an error during the execution of a CliProgram.
    /// </summary>
    public sealed class CommandException : Exception
    {
        /// <summary>
        /// Initializes the exception instance.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public CommandException(string message) : base(message) { }
    }
}