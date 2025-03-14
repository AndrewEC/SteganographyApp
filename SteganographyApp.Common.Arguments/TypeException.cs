namespace SteganographyApp.Common.Arguments;

using System;

/// <summary>
/// An exception indicating the underlying type of the MemberInfo is not one of either FieldInfo or PropertyInfo.
/// </summary>
/// <remarks>
/// Initializes the exception.
/// </remarks>
/// <param name="message">The exception message.</param>
public sealed class TypeException(string message) : Exception(message) { }
