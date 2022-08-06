namespace SteganographyApp.Common.Arguments
{
    using System;
    using System.Reflection;

    public sealed class TypeException : Exception
    {
        public TypeException(string message) : base(message) { }
    }

    public static class TypeHelper
    {
        private const string ErrorMessage = "Could not determine underlying type. MemberInfo must either be an instance of FieldInfo or PropertyInfo.";

        public static Type DeclaredType(MemberInfo memberInfo) => memberInfo switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => throw new TypeException(ErrorMessage)
        };

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

        public static object? GetValue(object instance, MemberInfo memberInfo) => memberInfo switch
        {
            FieldInfo field => field.GetValue(instance),
            PropertyInfo property => property.GetValue(instance),
            _ => throw new TypeException(ErrorMessage)
        };
    }
}