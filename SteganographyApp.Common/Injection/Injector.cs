using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SteganographyApp.Common.Injection
{

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InjectableAttribute : Attribute
    {

        public Type CorrelatesWith { private set; get; }

        public InjectableAttribute(Type correlatesWith)
        {
            CorrelatesWith = correlatesWith;
        }

    }


    /// <summary>
    /// An on request injection utility to help provide an implementation of a particular requested
    /// interface. It also provides the ability to replace an existing implementation in the injector
    /// with a mock of the interface.
    /// <para>With this utility most of the static method declarations should be replaced by
    /// calls to a method of a concrete instance that is injected via this utility.</para>
    /// </summary>
    public static class Injector
    {

        private static ImmutableDictionary<Type, object> DefaultInjectionValues;

        private static readonly Dictionary<Type, object> InjectionValues = new Dictionary<Type, object>();

        private static bool onlyTestObjectsState = false;

        static Injector()
        {
            DefaultInjectionValues = CreateDefaultInjectableDictionary();
            ResetInstances();
        }

        /// <summary>
        /// This method looks up all the types in the same assembly as this Injector class in which the
        /// type is both a class and has the InjectableAttribute present.
        /// <para>It then reflectively invokes the constructor on the class and stores it in the
        /// dictionary of DefaultInjectionProviders.</para>
        /// </summary>
        private static ImmutableDictionary<Type, object> CreateDefaultInjectableDictionary()
        {

            var injectables = new Dictionary<Type, object>();

            foreach (var injectableType in FindInjectableTypesInAssembly())
            {
                var ctor = injectableType.GetConstructor(Type.EmptyTypes);
                var correlatedType = GetInjectableAttribute(injectableType).CorrelatesWith;
                injectables[correlatedType] = ctor.Invoke(new object[]{});
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
            return type.GetCustomAttributes(typeof(InjectableAttribute), false).FirstOrDefault() as InjectableAttribute;
        }

        /// <summary>
        /// Returns an instance from the dictionary of injection provides using the generic
        /// parameter T as the lookup key.
        /// </summary>
        public static T Provide<T>()
        {
            var type = typeof(T);
            if (IsProvidingNonMockObjectWhenInTestState(type))
            {
                string message = string.Format("Injector is in a test state but tried to provide a non-mocked object for type: {0}", type.Name);
                throw new InvalidOperationException(message);
            }
            return (T) InjectionValues[typeof(T)];
        }

        private static bool IsProvidingNonMockObjectWhenInTestState(Type type)
        {
            return onlyTestObjectsState && InjectionValues[type] == DefaultInjectionValues[type];
        }

        /// <summary>
        /// Replaces or adds a provider in the injection providers dictionary using the
        /// type of generic parameter T as the key and the provided instance as the value
        /// return from the provider function.
        /// </summary>
        public static void UseInstance<T>(T instance)
        {
            InjectionValues[typeof(T)] = instance;
        }

        /// <summary>
        /// Resets all of the initial provider functions back to their original state.
        /// </summary>
        public static void ResetInstances()
        {
            foreach (Type key in DefaultInjectionValues.Keys)
            {
                InjectionValues[key] = DefaultInjectionValues[key];
            }
        }

        public static void AllowOnlyTestObjects()
        {
            onlyTestObjectsState = true;
        }

        public static void AllowOnlyRealObjects()
        {
            onlyTestObjectsState = false;
        }

    }

}