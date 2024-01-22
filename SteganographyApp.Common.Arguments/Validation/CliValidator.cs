namespace SteganographyApp.Common.Arguments.Validation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

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
        Dictionary<MemberInfo, (ArgumentAttribute Argument, ImmutableArray<ValidationAttribute> Validations)> verifiable = FindValidatableMembers(instance);
        if (verifiable.Count == 0)
        {
            return;
        }
        foreach (MemberInfo member in verifiable.Keys)
        {
            object? value = TypeHelper.GetValue(instance, member);
            try
            {
                foreach (ValidationAttribute validationAttribute in verifiable[member].Validations)
                {
                    validationAttribute.Validate(member, value);
                }
            }
            catch (Exception e) when (e is ValidationFailedException || e is IncompatibleTypeException)
            {
                string argumentName = FormName(verifiable[member].Argument);
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

    private static Dictionary<MemberInfo, (ArgumentAttribute Argument, ImmutableArray<ValidationAttribute> Validations)> FindValidatableMembers(object instance)
    {
        ImmutableArray<MemberInfo> verifiableMembers = TypeHelper.GetAllFieldsAndProperties(instance.GetType());
        Dictionary<MemberInfo, (ArgumentAttribute, ImmutableArray<ValidationAttribute>)> verifiable = [];
        if (verifiableMembers.Length == 0)
        {
            return verifiable;
        }

        foreach (MemberInfo member in verifiableMembers)
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

            verifiable.Add(member, (argumentAttribute, validationAttributes));
        }
        return verifiable;
    }
}