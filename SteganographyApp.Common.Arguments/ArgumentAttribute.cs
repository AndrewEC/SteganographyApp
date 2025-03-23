namespace SteganographyApp.Common.Arguments;

using System;

/// <summary>
/// Denotes the field or property is an argument whose value is intended
/// to be parsed out from a user provided argument.
/// </summary>
/// <param name="name">The name of the argument. This is what the user
/// will need to enter to specify the value of the argument value.
/// E.g. --name "Jane Doe". Has no affect if the argument is positional.</param>
/// <param name="shortName">The shortened name of the argument. Has no affect
/// if the argument is positional.</param>
/// <param name="position">Makes the argument positional if a value
/// above -1 is provided. Positional parameters are required. A boolean field
/// cannot be made positional.</param>
/// <param name="helpText">The text to display describing this
/// argument when the user provides the -h or --help arguments.</param>
/// <param name="parser">The name of a parser function to use
/// to parse the value for this argument. The parser function must be public static
/// and be defined in the same type, or inherited type, as the field being attributed
/// and match the signature: (object?, string) => object.</param>
/// <param name="example">Provides an example of how to use the
/// argument within the context of the current program.</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ArgumentAttribute(string name, string? shortName = null, int position = -1, string helpText = "", string? parser = null, string? example = null) : Attribute
{
    /// <summary>
    /// Gets the name of the argument. This is what the user
    /// will need to enter to specify the value of the argument value.
    /// E.g. --name "Jane Doe". Has no affect if the argument is positional.
    /// </summary>
    public string Name { get; } = name.Trim();

    /// <summary>
    /// Gets shortened name of the argument. Has no affect
    /// if the argument is positional.
    /// </summary>
    public string? ShortName { get; } = shortName?.Trim();

    /// <summary>
    /// Gets the text to display describing this argument when the user
    /// provides the -h or --help arguments.
    /// </summary>
    public string HelpText => helpText;

    /// <summary>
    /// Gets the position of the argument. An argument is considered positional
    /// if this value is greater than -1.
    /// </summary>
    public int Position => position;

    /// <summary>
    /// Gets the name of a parser function to use
    /// to parse the value for this argument. The parser function must be public
    /// static and be defined in the same type, or inherited type, as the field
    /// being attributed and match the signature: (object?, string) => object.
    /// </summary>
    public string? Parser => parser;

    /// <summary>
    /// Gets an example of how to use the argument within the context of the
    /// current program.
    /// </summary>
    public string? Example => example;
}