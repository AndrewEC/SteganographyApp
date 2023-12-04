namespace SteganographyApp.Common.Arguments;

using System;

/// <summary>
/// A class level metadata attribute used to help provide a description of the program being executed.
/// </summary>
/// <remarks>
/// Initializes the attribute.
/// </remarks>
/// <param name="helpText">The description to be used when displaying help text information to the user.</param>
/// <param name="example">Provides a complete example of how the program can be used.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ProgramDescriptorAttribute(string helpText, string? example = null) : Attribute
{
    /// <summary>
    /// Gets the description of the program.
    /// </summary>
    public string HelpText { get; private set; } = helpText;

    /// <summary>
    /// Gets a string outlining an example of how to use the current program.
    /// </summary>
    public string? Example { get; private set; } = example;
}