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
    public static partial class Injector
    {
        /// <summary>
        /// This method looks up all the types in the same assembly as this Injector class in which the
        /// type is both a class and has the InjectableAttribute present.
        /// <para>It then reflectively instantiates the class and stores it in the
        /// dictionary of DefaultInjectionProviders using the class type as the key and the instance
        /// as the value.</para>
        /// </summary>
        public static ImmutableDictionary<Type, object> LookupDefaultInjectableDictionary()
        {
            var injectables = new Dictionary<Type, object>();

            foreach (var injectableType in FindInjectableTypesInAssembly())
            {
                var ctor = injectableType.GetConstructor(Type.EmptyTypes);
                var correlatedType = GetInjectableAttribute(injectableType).CorrelatesWith;
                injectables[correlatedType] = ctor.Invoke(new object[] { });
            }

            return injectables.ToImmutableDictionary();
        }

        private static IEnumerable<Type> FindInjectableTypesInAssembly()
        {
            return typeof(Injector).Assembly.GetTypes().Where(IsInjectableType);
        }

        private static bool IsInjectableType(Type type)
        {
            return type.IsClass && GetInjectableAttribute(type) != null;
        }

        private static InjectableAttribute GetInjectableAttribute(Type type)
        {
            return type.GetCustomAttribute(typeof(InjectableAttribute), false) as InjectableAttribute;
        }
    }

    /// <summary>
    /// For invoking any post construct methods registered on the things within the assembly that are injectable
    /// and have already been instantiated.
    /// </summary>
    public static partial class Injector
    {
        /// <summary>
        /// Attempts to lookup a publicly accessible method from each of the input instances that also
        /// has the PostConstruct attribute and invoke it. This expects the post construct method to be
        /// parameterless.
        /// </summary>
        public static void InvokePostConstructMethods(IEnumerable<object> instances)
        {
            foreach (object instance in instances)
            {
                GetPostConstructMethod(instance)?.Invoke(instance, new object[] { });
            }
        }

        private static MethodInfo GetPostConstructMethod(object instance)
        {
            return instance.GetType().GetMethods()
                .Where(HasPostConstructAttribute)
                .FirstOrDefault();
        }

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
        private static Dictionary<Type, object> injectionValues;

        private static ImmutableDictionary<Type, object> defaultInjectionValues;

        private static bool onlyTestObjectsState = false;

        static Injector()
        {
            defaultInjectionValues = LookupDefaultInjectableDictionary();
            ResetInstances();
            InvokePostConstructMethods(defaultInjectionValues.Values);
        }

        /// <summary>
        /// Proxy method that invokes the ILoggerFactory instance to return an ILogger instance
        /// for the specified type T.
        /// </summary>
        /// <typeparam name="T">The type of the class that will make use of the logger instance being provided.</typeparam>
        public static ILogger LoggerFor<T>() => Provide<ILoggerFactory>().LoggerFor(typeof(T));

        /// <summary>
        /// Returns an instance from the dictionary of injectable values using the type of generic
        /// parameter T as the lookup key.
        /// </summary>
        /// <typeparam name="T">The type of the injectable instance to be returned.</typeparam>
        public static T Provide<T>()
        {
            var type = typeof(T);
            if (IsProvidingNonMockObjectWhenInTestState(type))
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
        /// <param name="instance">An instance that conforms to T to be provided whenever Provide is called
        /// with type T as the type param.</param>
        public static void UseInstance<T>(T instance) => injectionValues[typeof(T)] = instance;

        /// <summary>
        /// Resets all of the initial provider functions back to their original state.
        /// </summary>
        public static void ResetInstances()
        {
            injectionValues = new Dictionary<Type, object>();
            foreach (Type key in defaultInjectionValues.Keys)
            {
                injectionValues[key] = defaultInjectionValues[key];
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
        /// <seealso cref="Injector.AllowOnlyMockObjects()"/>
        public static void AllowAnyObjects()
        {
            onlyTestObjectsState = false;
        }

        private static bool IsProvidingNonMockObjectWhenInTestState(Type type) => onlyTestObjectsState
            && injectionValues[type] == defaultInjectionValues[type];
    }
}