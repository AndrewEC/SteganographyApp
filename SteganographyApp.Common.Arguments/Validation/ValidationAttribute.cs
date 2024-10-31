namespace SteganographyApp.Common.Arguments.Validation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// The base <see cref="ValidationAttribute"/> that all other validation attributes
/// will inherit from. This contains some basic preflight checks to ensure values being
/// passed into the implementing <see cref="DoValidate(object)"/> method are neither
/// null and guaranteed to be one of the types the implementing instance supports.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public abstract class ValidationAttribute : Attribute
{
    private readonly IEnumerable<Type> validTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationAttribute"/> class.
    /// </summary>
    /// <param name="name">The preferred name of the attribute. This name will be surfaced and some
    /// error scenarios and can be useful to identify which specific validation attribute is having
    /// an issue. If no value is provided the name of the type will be used instead.</param>
    /// <param name="validTypes">The collection of Types that this attribute can process.</param>
    public ValidationAttribute(string? name, IEnumerable<Type> validTypes)
    {
        Name = name ?? GetType().Name;
        this.validTypes = validTypes;
    }

    /// <summary>
    /// Gets the name of the validation attribute being applied.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Takes in a value to validate, checks if the value is null, checks if the
    /// type of the value is one of the types accepted by the implementing validation
    /// attribute, then invokes the <see cref="DoValidate(object)"/> method when
    /// all checks are successful.
    /// </summary>
    /// <param name="member">Info on the underlying field or property being attribute.
    /// This is used to determine the declared type of the value being validated.</param>
    /// <param name="value">The value to be validated.</param>
    /// <exception cref="IncompatibleTypeException">Thrown if the type of the value
    /// is not one of the types supported by the underlying validation attribute.</exception>
    public void Validate(MemberInfo member, object? value)
    {
        Type valueType = TypeHelper.GetDeclaredType(member);
        if (!IsValidType(valueType))
        {
            throw new IncompatibleTypeException(this, validTypes, valueType);
        }
        if (value == null)
        {
            return;
        }
        DoValidate(value);
    }

    /// <summary>
    /// Delegate method to validate the value.
    /// </summary>
    /// <param name="value">The value to be validated.</param>
    protected abstract void DoValidate(object value);

    private bool IsValidType(Type valueType) => validTypes
        .Where(validType => valueType.IsInstanceOfType(validType) || valueType == validType)
        .FirstOrDefault() != null;
}