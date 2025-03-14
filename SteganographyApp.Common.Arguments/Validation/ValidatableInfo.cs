namespace SteganographyApp.Common.Arguments.Validation;

using System.Collections.Immutable;
using System.Reflection;

/// <summary>
/// Contains member and attribute info to dictate how a field can be validated.
/// </summary>
/// <param name="info">Reflective info on the field being validated.</param>
/// <param name="argument">Attribute info used to help log validation errors.</param>
/// <param name="validations">Validation attributes indicating what validations
/// need to be run on the field.</param>
internal readonly struct ValidatableInfo(MemberInfo info,
    ArgumentAttribute argument,
    ImmutableArray<ValidationAttribute> validations)
{
    /// <summary>
    /// Gets the reflective info on the field being validated.
    /// </summary>
    public MemberInfo Info { get; } = info;

    /// <summary>
    /// Gets the attribute info for the field being validated.
    /// </summary>
    public ArgumentAttribute Argument { get; } = argument;

    /// <summary>
    /// Gets the list of validations being applied to the field.
    /// </summary>
    public ImmutableArray<ValidationAttribute> Validations { get; } = validations;
}
