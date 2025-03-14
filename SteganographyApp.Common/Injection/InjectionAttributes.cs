namespace SteganographyApp.Common.Injection;

using System;

/// <summary>
/// A class level attribute that indicates the attributed class should be
/// instantiated and made available globally for injection.
/// It's expected that the constructor of the class being attributed is parameterless.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class InjectableAttribute : Attribute
{
    /// <summary>
    /// Initialize an injectable attribute with the correlated type.
    /// </summary>
    /// <param name="correlatesWith">The interface type that will act as the key that will be used
    /// to lookup an instance of this attributed class when injecting a value.</param>
    public InjectableAttribute(Type correlatesWith)
    {
        if (!correlatesWith.IsInterface)
        {
            throw new ArgumentException("The correlatesWith Type argument of the InjectableAttribute constructor must be an interface. The provided type is not an interface: " + correlatesWith.FullName);
        }

        CorrelatesWith = correlatesWith;
    }

    /// <summary>
    /// Gets the interface type that will act as the lookup key when trying to lookup and inject
    /// the instance of the class with this attribute.
    /// </summary>
    public Type CorrelatesWith { get; private set; }
}
