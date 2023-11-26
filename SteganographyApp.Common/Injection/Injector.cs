namespace SteganographyApp.Common.Injection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    using SteganographyApp.Common.Logging;

    /// <summary>
    /// For looking up and initializing the things within the current assembly that can be injected.
    /// </summary>
    public static class InjectableLookup
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
                var ctor = injectableType.GetConstructor(Type.EmptyTypes);
                var correlatedType = GetInjectableAttribute(injectableType)!.CorrelatesWith;
                injectables[correlatedType] = ctor?.Invoke(Array.Empty<object>())
                    ?? throw new ArgumentException("Injectable types must have a default empty construct. No matching constructor found for type: " + injectableType.Name);
            }

            return injectables.ToImmutableDictionary();
        }

        private static IEnumerable<Type> FindInjectableTypesInAssembly() => typeof(Injector).Assembly.GetTypes().Where(IsInjectableType);

        private static bool IsInjectableType(Type type) => type.IsClass && GetInjectableAttribute(type) != null;

        private static InjectableAttribute? GetInjectableAttribute(Type type) => type.GetCustomAttribute(typeof(InjectableAttribute), false) as InjectableAttribute;
    }

    /// <summary>
    /// For invoking any post construct methods registered on the things within the assembly that are injectable
    /// and have already been instantiated.
    /// </summary>
    public static partial class PostConstruct
    {
        /// <summary>
        /// Attempts to lookup a publicly accessible method from each of the input instances that also
        /// has the PostConstruct attribute and invoke it. This expects the post construct method to be
        /// parameterless.
        /// </summary>
        /// <param name="instances">The collection of instanced injectable objects to invoke the post
        /// constructors of if they exist.</param>
        internal static void InvokePostConstructMethods(IEnumerable<object> instances)
        {
            foreach (object instance in instances)
            {
                GetPostConstructMethod(instance)?.Invoke(instance, Array.Empty<object>());
            }
        }

        private static MethodInfo? GetPostConstructMethod(object instance) => instance.GetType().GetMethods().Where(HasPostConstructAttribute).FirstOrDefault();

        private static bool HasPostConstructAttribute(MethodInfo info) => info.GetCustomAttribute(typeof(PostConstructAttribute), false) != null;
    }

    /// <summary>
    /// An on request injection utility to help provide an implementation of a particular requested
    /// interface. It also provides the ability to replace an existing implementation in the injector
    /// with a mock of the interface.
    /// <para>With this utility most of the static method declarations should be replaced by
    /// calls to a method of a concrete instance that is injected via this utility.</para>
    /// </summary>
    public static partial class Injector
    {
        private static readonly ImmutableDictionary<Type, object> DefaultInjectionValues;

        private static Dictionary<Type, object> injectionValues;

        private static bool onlyTestObjectsState = false;

        static Injector()
        {
            DefaultInjectionValues = InjectableLookup.LookupDefaultInjectableDictionary();
            injectionValues = new Dictionary<Type, object>();
            ResetInstances();
            PostConstruct.InvokePostConstructMethods(DefaultInjectionValues.Values);
        }

        /// <summary>
        /// Proxy method that invokes the ILoggerFactory instance to return an ILogger instance
        /// for the specified type T.
        /// </summary>
        /// <typeparam name="T">The type of the class that will make use of the logger instance being provided.</typeparam>
        /// <returns>A new ILogger instance configured for the specified type T.</returns>
        public static ILogger LoggerFor<T>() => Provide<ILoggerFactory>().LoggerFor(typeof(T));

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
                throw new ArgumentException("Cannot UseInstance when correlated type is not an interface. The provided type is not an interface: " + type.FullName);
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
}