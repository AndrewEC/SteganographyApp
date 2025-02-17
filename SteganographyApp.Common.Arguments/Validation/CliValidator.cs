namespace SteganographyApp.Common.Arguments.Validation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

/// <summary>
/// Static utility class to assist in validating a given <see cref="CliParser"/>
/// initialized model.
/// </summary>
public static class CliValidator
{
    private static readonly string ArgumentIdentifierTemplate = "{0}|{1}";
    private static readonly string ValidationFailedTemlate = "Validation failed on field: {0}. {1}";

    /// <summary>
    /// Validates the <see cref="CliParser"/> initialized model. This will identify all fields and properties
    /// on the model that have been annotated with both <see cref="ArgumentAttribute"/> and
    /// at least one <see cref="ValidationAttribute"/>.
    /// <para>This will iterate through and execute each <see cref="ValidationAttribute"/> and raise a
    /// <see cref="ValidationFailedException"/> if any validation attribute throws either a <see cref="ValidationFailedException"/>
    /// or <see cref="IncompatibleTypeException"/>.</para>
    /// </summary>
    /// <param name="instance">The instance of the model to validate.</param>
    /// <exception cref="ValidationFailedException">Thrown if any of the validators applied to any field or property
    /// on the model failed validation.</exception>
    public static void Validate(object instance)
    {
        ImmutableArray<ValidatableInfo> validatableMembers = FindValidatableMembers(instance);
        if (validatableMembers.Length == 0)
        {
            return;
        }

        foreach (ValidatableInfo validatable in validatableMembers)
        {
            object? value = TypeHelper.GetValue(instance, validatable.Info);
            try
            {
                foreach (ValidationAttribute validationAttribute in validatable.Validations)
                {
                    validationAttribute.Validate(validatable.Info, value);
                }
            }
            catch (Exception e)
            {
                string argumentName = FormName(validatable.Argument);
                throw new ValidationFailedException(string.Format(ValidationFailedTemlate, argumentName, e.Message), e);
            }
        }
    }

    private static string FormName(ArgumentAttribute argument)
    {
        if (argument.ShortName == null)
        {
            return argument.Name;
        }
        return string.Format(ArgumentIdentifierTemplate, argument.Name, argument.ShortName);
    }

    private static ImmutableArray<ValidatableInfo> FindValidatableMembers(object instance)
    {
        ImmutableArray<MemberInfo> instanceMembers = TypeHelper.GetAllFieldsAndProperties(instance.GetType());
        if (instanceMembers.Length == 0)
        {
            return [];
        }

        List<ValidatableInfo> validatable = [];
        foreach (MemberInfo member in instanceMembers)
        {
            if (member.GetCustomAttribute(typeof(ArgumentAttribute)) is not ArgumentAttribute argumentAttribute)
            {
                continue;
            }

            ImmutableArray<ValidationAttribute> validationAttributes = member.GetCustomAttributes(typeof(ValidationAttribute))
                .Select(attribute => (ValidationAttribute)attribute)
                .ToImmutableArray();
            if (validationAttributes.Length == 0)
            {
                continue;
            }

            validatable.Add(new(member, argumentAttribute, validationAttributes));
        }
        return validatable.ToImmutableArray();
    }
}