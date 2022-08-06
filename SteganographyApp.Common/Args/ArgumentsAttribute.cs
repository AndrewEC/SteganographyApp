namespace SteganographyApp.Common.Arguments
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProgramDescriptorAttribute : Attribute
    {
        public ProgramDescriptorAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}