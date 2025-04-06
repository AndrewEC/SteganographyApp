namespace SteganographyApp.Common.Arguments.Validation;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// An exception thrown to identify that the underlying type being attributed
/// by a validation attribute is not supported by said validation attribute.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IncompatibleTypeException"/> class.
/// </remarks>
/// <param name="attribute">The validation attribute being executed.</param>
/// <param name="validTypes">The collection of types the validation attribute supports.</param>
/// <param name="actualType">The type of the value being passed to the validation attribute.</param>
public class IncompatibleTypeException(ValidationAttribute attribute, IEnumerable<Type> validTypes, Type actualType)
: Exception(string.Format(CultureInfo.InvariantCulture, MessageFormat, attribute.Name, string.Join(", ", validTypes), actualType))
{
    private static readonly CompositeFormat MessageFormat = CompositeFormat.Parse(
        "Incompatible type provided to validator [{0}]. Valid types are [{1}] but instead received [{2}].");
}