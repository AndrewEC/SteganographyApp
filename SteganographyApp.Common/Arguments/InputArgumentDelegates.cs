namespace SteganographyApp.Common.Arguments
{
    /// <summary>
    /// Takes in a value retrieved from an associated key, parses it, and sets the
    /// relevant property value in the InputArguments instance
    /// </summary>
    /// <param name="args">The InputArguments param to modify.</param>
    /// <param name="value">The value of the key/value pair from the array of arguments.</param>
    public delegate void ValueParser(InputArguments args, string value);

    /// <summary>
    /// Takes in the collected set of argument/value pairs and performs a final validation
    /// on them.
    /// <para>If the string returned is neither null or empty than then the validation is treated
    /// as a failure.</para>
    /// </summary>
    /// <param name="args">The InputArguments and all their associated values.</param>
    /// <returns>An empty or null string if validation was successful, otherwise should return a message
    /// containing information about the validation failure.</returns>
    public delegate string PostValidation(IInputArguments args);
}