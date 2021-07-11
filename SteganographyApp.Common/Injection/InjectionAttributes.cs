namespace SteganographyApp.Common.Injection
{
    using System;

    /// <summary>
    /// A class level attribute that indicates the attributed class should be
    /// instantiated and made available globally for injection.
    /// It's expected that the constructor of the class being attributed is parameterless.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectableAttribute : Attribute
    {
        public InjectableAttribute(Type correlatesWith)
        {
            CorrelatesWith = correlatesWith;
        }

        public Type CorrelatesWith { get; private set; }
    }

    /// <summary>
    /// A method level attribute that indicates the attributed method should be invoked
    /// by the static Injector class after all injectable classes have been instantiated.
    /// The method being attributed must be parameterless.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PostConstructAttribute : Attribute { }
}