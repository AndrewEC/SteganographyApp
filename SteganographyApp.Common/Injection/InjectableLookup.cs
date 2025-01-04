namespace SteganographyApp.Common.Injection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

/// <summary>
/// For looking up and initializing the things within the current assembly that can be injected.
/// </summary>
internal static class InjectableLookup
{
    /// <summary>
    /// This method looks up all the types in the same assembly as this Injector class in which the
    /// type is both a class and has the InjectableAttribute present.
    /// <para>It then reflectively instantiates the class and stores it in the
    /// dictionary of DefaultInjectionProviders using the class type as the key and the instance
    /// as the value.</para>
    /// </summary>
    /// <returns>Returns an immutable dictionary with all the type and an instance
    /// of each type within this assembly that has the injectable attribute.</returns>
    internal static ImmutableDictionary<Type, object> LookupDefaultInjectableDictionary()
    {
        var injectables = new Dictionary<Type, object>();

        foreach (var injectableType in FindInjectableTypesInAssembly())
        {
            var correlatedType = GetInjectableAttribute(injectableType)!.CorrelatesWith;

            if (!correlatedType.IsInterface)
            {
                throw new ArgumentException($"Cannot use type [{injectableType.FullName}] for injection because the "
                    + $"attributed type [{correlatedType.FullName}] is not an interface.");
            }

            if (injectables.TryGetValue(correlatedType, out object? value))
            {
                string? existingTypeName = value.GetType().FullName;
                throw new ArgumentException($"Attempt to register two injectables with the same correlated type. "
                    + $"[{existingTypeName}] and [{correlatedType.FullName}]");
            }

            var ctor = injectableType.GetConstructor(Type.EmptyTypes);
            injectables[correlatedType] = ctor?.Invoke([])
                ?? throw new ArgumentException("Injectable types must have a default empty construct. No matching constructor found for type: " + injectableType.Name);
        }

        return injectables.ToImmutableDictionary();
    }

    private static IEnumerable<Type> FindInjectableTypesInAssembly() => typeof(Injector).Assembly.GetTypes().Where(IsInjectableType);

    private static bool IsInjectableType(Type type) => type.IsClass && GetInjectableAttribute(type) != null;

    private static InjectableAttribute? GetInjectableAttribute(Type type) => type.GetCustomAttribute(typeof(InjectableAttribute), false) as InjectableAttribute;
}