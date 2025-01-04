namespace SteganographyApp.Common.Arguments.Matching;

using System;
using System.Reflection;

/// <summary>
/// Contains basic information about the argument found from the argument class including the attribute info,
/// member info, and the parser to use to parse a value for the Member.
/// </summary>
/// <remarks>
/// Initializes the registered argument.
/// </remarks>
/// <param name="attribute">The attribute.</param>
/// <param name="member">The field or property of the argument class that has been attributed.</param>
/// <param name="parser">The parser function that will parse a value from the user's input and set the member value.</param>
internal class RegisteredArgument(ArgumentAttribute attribute, MemberInfo member, Func<object, string, object> parser)
{
    /// <summary>
    /// Gets a reference to the attribute info.
    /// </summary>
    public ArgumentAttribute Attribute { get; } = attribute;

    /// <summary>
    /// Gets a referce to the member of the argument class that has been attributed with the argument attribute.
    /// </summary>
    public MemberInfo Member { get; } = member;

    /// <summary>
    /// Gets the parser function that will be used to parse out a value from the user's input and set the Member value.
    /// </summary>
    public Func<object, string, object> Parser { get; } = parser;
}