namespace SteganographyApp.Common.Arguments;

using System;
using System.Reflection;

/// <summary>
/// A top level interface allowing one to augment the cli parser to use custom parsers for custom field types.
/// </summary>
public interface IParserFunctionProvider
{
    /// <summary>
    /// Lookup and return function for parsing a field of the type specified by the member parameter.
    /// </summary>
    /// <param name="attribute">The meta attribute about the argument to parse.</param>
    /// <param name="member">The field or property being parsed.</param>
    /// <returns>The matched parser function or null if not matching function could be found.</returns>
    public Func<object, string, object>? Find(ArgumentAttribute attribute, MemberInfo member);
}
