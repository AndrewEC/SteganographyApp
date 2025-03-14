using System;

/// <summary>
/// Used to indicate that a field is required.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RequiredAttribute : Attribute
{
}
