namespace SteganographyApp.Common.Arguments;

using SteganographyApp.Common.Injection;

/// <summary>
/// A static class containing a series of parsers for parsing out the user's provided arguments.
/// </summary>
public static class ParserFunctions
{
    /// <summary>
    /// Verifies that the value parameter points to an existing file.
    /// </summary>
    /// <param name="value">A string pointing to the location of a user specified file.</param>
    /// <returns>The original input value parmeter.</returns>
    public static string ParseFilePath(string value)
    {
        if (!Injector.Provide<IFileIOProxy>().IsExistingFile(value))
        {
            throw new ArgumentValueException($"Input file could not be found or is not a file: {value}");
        }
        return value;
    }
}