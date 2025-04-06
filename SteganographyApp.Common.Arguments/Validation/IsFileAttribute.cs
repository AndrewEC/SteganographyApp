namespace SteganographyApp.Common.Arguments.Validation;

using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// Validates that an underlying property or field is a string value that points to
/// an existing file on disk.
/// </summary>
/// <param name="name">An optional name for this validatin attribute instance. This can provide more
/// informatiom if an exception is thrown an potentially help narrow down the source of an error
/// if multiple InRangeAttributes are being used in a single program.</param>
public class IsFileAttribute(string? name = null) : ValidationAttribute(name, [typeof(string)])
{
    private static readonly CompositeFormat MessageFormat = CompositeFormat.Parse(
        "Path {0} does not point to a file.");

    /// <summary>
    /// Performs validation on the value of the attributed field or property to ensure the value
    /// points to a file on disk.
    /// <para>It can be assumed that the value is not null and is a string.</para>
    /// </summary>
    /// <param name="value">A string value representing a potential path to a file.</param>
    /// <exception cref="ValidationFailedException">Thrown if the input value does not point to a
    /// file on disk.</exception>
    protected override void DoValidate(object value)
    {
        if (!File.Exists((string)value))
        {
            throw new ValidationFailedException(string.Format(CultureInfo.InvariantCulture, MessageFormat, value));
        }
    }
}