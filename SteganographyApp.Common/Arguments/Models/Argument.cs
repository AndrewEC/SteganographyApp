namespace SteganographyApp.Common.Arguments
{
    /// <summary>
    /// Encapsulates information about an argument that the user can specify when invoking the
    /// tool.
    /// </summary>
    public sealed class Argument
    {
        public Argument(string name, string shortName, ValueParser parser, bool flag = false)
        {
            Name = name;
            ShortName = shortName;
            Parser = parser;
            IsFlag = flag;
        }

        public string Name { get; private set; }

        public string ShortName { get; private set; }

        public ValueParser Parser { get; private set; }

        public bool IsFlag { get; private set; }
    }
}