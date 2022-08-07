namespace SteganographyApp.Common.Arguments
{
    using System;

    /// <summary>
    /// A class level metadata attribute used to help provide a description of the program being executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProgramDescriptorAttribute : Attribute
    {
        /// <summary>
        /// Initializes the attribute.
        /// </summary>
        /// <param name="description">The description to be used when displaying help text information to the user.</param>
        public ProgramDescriptorAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets the description of the program.
        /// </summary>
        public string Description { get; private set; }
    }
}