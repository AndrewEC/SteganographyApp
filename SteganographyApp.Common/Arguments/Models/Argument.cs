namespace SteganographyApp.Common.Arguments
{
    /// <summary>
    /// Encapsulates information about an argument that the user can specify when invoking the
    /// tool.
    /// </summary>
    public sealed class Argument
    {
        /// <summary>
        /// Initializes the argument with the required values.
        /// </summary>
        /// <param name="name">The full name of the argument.</param>
        /// <param name="shortName">The shorthand name of the argument.</param>
        /// <param name="parser">The delegate that will be invoked to parse the current argument from the associated user input.</param>
        /// <param name="flag">Indicates if this argument is to be a switch that can only have a true or false value.</param>
        /// <param name="sensitive">Indicates that this argument is sensitive and the user should potentially be prompted to
        /// enter the value in an interactive mode.</param>
        public Argument(string name, string shortName, ValueParser parser, bool flag = false, bool sensitive = false)
        {
            Name = name;
            ShortName = shortName;
            Parser = parser;
            IsFlag = flag;
            IsSensitive = sensitive;
        }

        /// <summary>
        /// Gets the full name of the argument. Used when matching arguments against the user's
        /// input to determine which argument to parse.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the short name of the argument. Used when matching arguments against the user's
        /// input to determine which argument to parse.
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Gets the delegate that will be invoked to parse and set the value of the final argument
        /// using the users input.
        /// </summary>
        public ValueParser Parser { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this argument should have a simple true/false value or if it requires
        /// a user defined value.
        /// </summary>
        public bool IsFlag { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this argument should be treated as sensitive and provide the user the option
        /// to provide a value through an interactive mode.
        /// </summary>
        public bool IsSensitive { get; private set; }
    }
}