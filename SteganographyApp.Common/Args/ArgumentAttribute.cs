namespace SteganographyApp.Common.Arguments
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute(string name, string? shortName = null, bool required = false, int position = -1, string helpText = "", string? parser = null)
        {
            Name = name;
            ShortName = shortName;
            Required = required;
            HelpText = helpText;
            Position = position;
            if (Position > -1)
            {
                Required = true;
            }
            Parser = parser;
        }

        public string Name { get; private set; }
        public string? ShortName { get; private set; }
        public bool Required { get; private set; }
        public string HelpText { get; private set; }
        public int Position { get; private set; }
        public string? Parser { get; private set; }
    }
}