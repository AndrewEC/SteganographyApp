namespace SteganographyApp.Common.Arguments.Validation;

using System.Collections.Immutable;
using System.Reflection;

/// <summary>
/// Contains member and attribute info to dictate how a field can be validated.
/// </summary>
/// <param name="Info">Reflective info on the field being validated.</param>
/// <param name="Argument">Attribute info used to help log validation errors.</param>
/// <param name="Validations">Validation attributes indicating what validations
/// need to be run on the field.</param>
internal sealed record class ValidatableInfo(MemberInfo Info,
    ArgumentAttribute Argument,
    ImmutableArray<ValidationAttribute> Validations) { }
