namespace SteganographyApp.Common.Injection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using SteganographyApp.Common.Logging;

/// <summary>
/// An on request injection utility to help provide an implementation of a particular requested
/// interface. It also provides the ability to replace an existing implementation in the injector
/// with a mock of the interface.
/// <para>With this utility most of the static method declarations should be replaced by
/// calls to a method of a concrete instance that is injected via this utility.</para>
/// </summary>
public static class Injector
{
    private static readonly ImmutableDictionary<Type, object> DefaultInjectionValues;

    private static Dictionary<Type, object> injectionValues;

    private static bool onlyTestObjectsState = false;

    static Injector()
    {
        DefaultInjectionValues = InjectableLookup.LookupDefaultInjectableDictionary();
        injectionValues = new Dictionary<Type, object>();
        ResetInstances();
    }

    /// <summary>
    /// Proxy method that invokes the ILoggerFactory instance to return an ILogger instance
    /// for the specified type T.
    /// </summary>
    /// <typeparam name="T">The type of the class that will make use of the logger instance being provided.</typeparam>
    /// <returns>A new ILogger instance configured for the specified type T.</returns>
    public static ILogger LoggerFor<T>() => LoggerFor(typeof(T));

    /// <summary>
    /// Proxy method that invokes the ILoggerFactory instance to return an ILogger instance
    /// for the specified type T.
    /// </summary>
    /// <param name="type">The type of the class that will make use of the logger instance being provided.</param>
    /// <returns>A new ILogger instance configured for the specified type T.</returns>
    public static ILogger LoggerFor(Type type) => Provide<ILoggerFactory>().LoggerFor(type);

    /// <summary>
    /// Returns an instance from the dictionary of injectable values using the type of generic
    /// parameter T as the lookup key.
    /// </summary>
    /// <typeparam name="T">The type of the injectable instance to be returned.</typeparam>
    /// <returns>Returns an instance from the current collection of injectable instances that conforms to type T.</returns>
    public static T Provide<T>()
    {
        var type = typeof(T);
        if (IsProvidingRealObjectWhenInTestState(type))
        {
            string message = string.Format("Injector is in a test state but tried to provide a non-mocked object for type: [{0}]", type.Name);
            throw new InvalidOperationException(message);
        }
        return (T)injectionValues[type];
    }

    /// <summary>
    /// Replaces or adds a provider in the injection providers dictionary using the
    /// type of generic parameter T as the key and the provided instance as the value.
    /// </summary>
    /// <typeparam name="T">The key whose corresponding value in the injectables dictionary will be replaced
    /// with the <paramref name="instance"/> value.</typeparam>
    /// <param name="instance">An instance that conforms to T to be provided whenever Provide is called
    /// with type T as the type param.</param>
    public static void UseInstance<T>(T instance)
    where T : notnull
    {
        var type = typeof(T);
        if (!type.IsInterface)
        {
            throw new ArgumentException($"Correlated type must be an interface. The following type is not an interface: [{type.FullName}]");
        }
        if (!injectionValues.ContainsKey(type))
        {
            throw new ArgumentException($"Cannot UseInstance for type [{type.FullName}] since type is not available in default injection dictionary.");
        }
        injectionValues[type] = instance;
    }

    /// <summary>
    /// Resets all of the initial provider functions back to their original state.
    /// </summary>
    public static void ResetInstances()
    {
        injectionValues = new Dictionary<Type, object>();
        foreach (Type key in DefaultInjectionValues.Keys)
        {
            injectionValues[key] = DefaultInjectionValues[key];
        }
    }

    /// <summary>
    /// Sets the onlyTestObjectsState flag to true. While true this will cause the Provide method to throw
    /// an exception if the object about to be provided is the same as the default injectable value loaded during
    /// initialization. This is to help ensure that, when running unit tests, real implementations are not being
    /// accidently provided making it easier to track where objects need to be mocked.
    /// </summary>
    public static void AllowOnlyMockObjects()
    {
        onlyTestObjectsState = true;
    }

    /// <summary>
    /// Sets the onlyTestObjectsState flag to false.
    /// </summary>
    /// <seealso cref="AllowOnlyMockObjects()"/>
    public static void AllowAnyObjects()
    {
        onlyTestObjectsState = false;
    }

    private static bool IsProvidingRealObjectWhenInTestState(Type type) => onlyTestObjectsState
        && injectionValues[type] == DefaultInjectionValues[type];
}
