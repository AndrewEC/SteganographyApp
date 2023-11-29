namespace SteganographyApp.Common.Arguments;

using System;
using System.Reflection;

/// <summary>
/// Assists in retrieving or applying information in a reflective manner to a MemberInfo instance.
/// </summary>
public static class TypeHelper
{
    private const string ErrorMessage = "Could not determine underlying type. MemberInfo must either be an instance of FieldInfo or PropertyInfo.";

    /// <summary>
    /// Gets the underlying type of the member. This only works on fields and properties. Attempting to use this with
    /// any other type will result in a TypeException.
    /// </summary>
    /// <param name="memberInfo">The member info from which we want to extract the underlying field type or property type.</param>
    /// <returns>The underlying type of the member.</returns>
    public static Type DeclaredType(MemberInfo memberInfo) => memberInfo switch
    {
        FieldInfo field => field.FieldType,
        PropertyInfo property => property.PropertyType,
        _ => throw new TypeException(ErrorMessage),
    };

    /// <summary>
    /// Attempts to reflectively set the value of a field or property. This only works on fields and properties. Attempting to use this with
    /// any other type will result in a TypeException.
    /// </summary>
    /// <param name="instance">The instance containing the member we are trying to set.</param>
    /// <param name="memberInfo">The field or property declared by the instance to set.</param>
    /// <param name="value">The value the member will be set to.</param>
    public static void SetValue(object instance, MemberInfo memberInfo, object value)
    {
        if (memberInfo is FieldInfo field)
        {
            field.SetValue(instance, value);
            return;
        }
        else if (memberInfo is PropertyInfo property)
        {
            property.SetValue(instance, value);
            return;
        }
        throw new TypeException(ErrorMessage);
    }

    /// <summary>
    /// Attempts to reflectively get the value of a field or property. This only works on fields and properties. Attempting to use this with
    /// any other type will result in a TypeException.
    /// </summary>
    /// <param name="instance">The instance declaring the member whose value we want to retrieve.</param>
    /// <param name="memberInfo">The field or property whose value we want to retrieve.</param>
    /// <returns>The value retrieved from the member.</returns>
    public static object? GetValue(object instance, MemberInfo memberInfo) => memberInfo switch
    {
        FieldInfo field => field.GetValue(instance),
        PropertyInfo property => property.GetValue(instance),
        _ => throw new TypeException(ErrorMessage),
    };
}

/// <summary>
/// An exception indicating the underlying type of the MemberInfo is not one of either FieldInfo or PropertyInfo.
/// </summary>
/// <remarks>
/// Initializes the exception.
/// </remarks>
/// <param name="message">The exception message.</param>
public sealed class TypeException(string message) : Exception(message) { }