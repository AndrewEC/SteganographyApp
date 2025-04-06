namespace SteganographyApp.Common.Arguments;

using System;
using System.Collections.Immutable;
using System.Reflection;
using SteganographyApp.Common.Arguments.Validation;

/// <summary>
/// Assists in retrieving or applying information in a reflective manner
/// to a <see cref="MemberInfo"/> instance.
/// </summary>
public static class TypeHelper
{
    private const string ErrorMessage = "Could not determine underlying type. MemberInfo must either be an instance of FieldInfo or PropertyInfo.";

    /// <summary>
    /// Gets the underlying type of the member. This only works on fields and properties. Attempting to use this with
    /// any other type will result in a <see cref="TypeException"/>.
    /// </summary>
    /// <param name="memberInfo">The member info from which we want to extract the underlying field type or property type.</param>
    /// <returns>The underlying type of the member.</returns>
    public static Type GetDeclaredType(MemberInfo memberInfo) => memberInfo switch
    {
        FieldInfo field => field.FieldType,
        PropertyInfo property => property.PropertyType,
        _ => throw new TypeException(ErrorMessage),
    };

    /// <summary>
    /// Attempts to reflectively set the value of a field or property. This only works on fields and properties. Attempting to use this with
    /// any other type will result in a <see cref="TypeException"/>.
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
    /// any other type will result in a <see cref="TypeException"/>.
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

    /// <summary>
    /// Gets an array representing all field and property members of the specified type.
    /// </summary>
    /// <param name="modelType">The to get all the fields and properties of.</param>
    /// <returns>An immutable array of all field and property members.</returns>
    public static ImmutableArray<MemberInfo> GetAllFieldsAndProperties(Type modelType)
        => [.. modelType.GetFields(), .. modelType.GetProperties()];

    /// <summary>
    /// Gets a bool indicating if the argument is required. I.e. the user must provide a cli
    /// value for the argument. Arguments that are positional are automatically considered
    /// required.
    /// </summary>
    /// <param name="attribute">The argument attribute. Required to get the position of the
    /// argument.</param>
    /// <param name="member">The underlying member being attributed.</param>
    /// <returns>True if the argument is positional or if the member has the Required
    /// attribute. Otherwise, false.</returns>
    public static bool IsArgumentRequired(ArgumentAttribute attribute, MemberInfo member)
        => attribute.Position > -1 || member.GetCustomAttribute(typeof(RequiredAttribute)) != null;
}
