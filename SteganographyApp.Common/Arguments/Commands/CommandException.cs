namespace SteganographyApp.Common.Arguments.Commands;

using System;

/// <summary>
/// A high level exception thrown if there was an error during the execution of a CliProgram.
/// </summary>
/// <remarks>
/// Initializes the exception instance.
/// </remarks>
/// <param name="message">The exception message.</param>
public sealed class CommandException(string message) : Exception(message) { }
