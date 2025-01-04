namespace SteganographyApp.Common.Arguments.Matching;

using System;
using System.Reflection;

/// <summary>
/// The result of matching a registered argument to the user's input.
/// </summary>
/// <remarks>
/// Initializes the match result.
/// </remarks>
/// <param name="registeredArgument">The registered argument containing the attribute, member, and parser information.</param>
/// <param name="input">The user provided input to be parsed and set based on the info from the registered argumetn.</param>
internal readonly struct MatchResult(RegisteredArgument registeredArgument, string input)
{
    /// <summary>
    /// Gets the argument attribute.
    /// </summary>
    public readonly ArgumentAttribute Attribute { get; } = registeredArgument.Attribute;

    /// <summary>
    /// Gets the reflective info about the field or property being set.
    /// </summary>
    public readonly MemberInfo Member { get; } = registeredArgument.Member;

    /// <summary>
    /// Gets the user provided input value to be parsed.
    /// </summary>
    public readonly string Input { get; } = input;

    /// <summary>
    /// Gets the parser function used to parse the Input value and set the value of the Member.
    /// </summary>
    public readonly Func<object, string, object> Parser { get; } = registeredArgument.Parser;
}
