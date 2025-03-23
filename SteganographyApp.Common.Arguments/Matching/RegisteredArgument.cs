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
/// <param name="Attribute">The attribute.</param>
/// <param name="Member">The field or property of the argument class that has been attributed.</param>
/// <param name="Parser">The parser function that will parse a value from the user's input and set the member value.</param>
public record class RegisteredArgument(
    ArgumentAttribute Attribute,
    MemberInfo Member,
    Func<object, string, object> Parser);