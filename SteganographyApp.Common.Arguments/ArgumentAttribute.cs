namespace SteganographyApp.Common.Arguments;

using System;

/// <summary>
/// Denotes the field or property is an argument attribute and whose value can be parsed out from a user provided argument.
/// This will also provide some information regarding how to find the value to parse from the user provided arguments
/// and how to parse the value.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Initialize the argument attribute.
    /// </summary>
    /// <param name="name">The name of the argument.</param>
    /// <param name="shortName">The shortened name of the argument.</param>
    /// <param name="required">Whether the argument must be provided.</param>
    /// <param name="position">Makes the argument positional if a value above -1 is provided.</param>
    /// <param name="helpText">The text to display describing this argument when the user provides the -h or --help arguments.</param>
    /// <param name="parser">The name of a parser function to use to parse the value for this argument.</param>
    /// <param name="example">Provides an example of how to use the argument within the context of the current program.</param>
    public ArgumentAttribute(string name, string? shortName = null, bool required = false, int position = -1, string helpText = "", string? parser = null, string? example = null)
    {
        Name = name.Trim();
        ShortName = shortName?.Trim();
        Required = required;
        HelpText = helpText;
        Position = position;
        if (Position > -1)
        {
            Required = true;
        }
        Parser = parser;
        Example = example;
    }

    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the short name of the argument. This should typically be a shortened version of the arguments's Name.
    /// For example --request could be shortened to -r.
    /// </summary>
    public string? ShortName { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a value for the argument must be provided by the user. If an argument is
    /// positional then it is also required. This cannot be set to true when the attributed field or property type
    /// is a boolean.
    /// </summary>
    public bool Required { get; private set; }

    /// <summary>
    /// Gets the text to be displayed when the user provides the -h or --help argument.
    /// </summary>
    public string HelpText { get; private set; }

    /// <summary>
    /// Gets the position of the argument. When an argument has a position greater than -1 the name and short name values
    /// will be ignored.
    /// This property cannot have a positive value when the attributed field or property type is a boolean.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Gets the name of a parser function that should be used to parse the value for this field or property.
    /// The name of the parser must correspond to a static function within the same class as this field or property and
    /// have the signature matching: (object?, string) => object.
    /// </summary>
    public string? Parser { get; private set; }

    /// <summary>
    /// Gets a string outlining an example of how to apply this argument within the context of the current program.
    /// </summary>
    public string? Example { get; private set; }
}