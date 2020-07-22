using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Providers
{

    public delegate object ProviderFunction(params object[] arguments);

    /// <summary>
    /// An on request injection utility to help provide an implementation of a particular requested
    /// interface. It also provides the ability to replace an existing implementation in the injector
    /// with a mock of the interface.
    /// <para>With this utility most of the static method declarations should be replaced by
    /// calls to a method of a concrete instance that is injected via this utility.</para>
    /// </summary>
    public static class Injector
    {

        private static readonly ImmutableDictionary<Type, ProviderFunction> DefaultInjectionProviders = new Dictionary<Type, ProviderFunction>
        {
            { typeof(IEncryptionProvider), CreateProviderFor(new EncryptionProvider()) },
            { typeof(IFileProvider), CreateProviderFor(new FileProvider()) },
            { typeof(IBinaryUtil), CreateProviderFor(new BinaryUtil()) },
            { typeof(IDummyUtil), CreateProviderFor(new DummyUtil()) },
            { typeof(IRandomizeUtil), CreateProviderFor(new RandomizeUtil()) },
            { typeof(IDataEncoderUtil), CreateProviderFor(new DataEncoderUtil()) },
            { typeof(IImageProvider), CreateProviderFor(new ImageProvider()) },
            { typeof(ICompressionUtil), CreateProviderFor(new CompressionUtil()) }
        }
        .ToImmutableDictionary();

        private static readonly Dictionary<Type, ProviderFunction> InjectionProviders = new Dictionary<Type, ProviderFunction>();

        static Injector()
        {
            ResetProviders();
        }

        /// <summary>
        /// Returns an instance from the dictionary of injection provides using the generic
        /// parameter T as the lookup key.
        /// </summary>
        public static T Provide<T>(params object[] arguments)
        {
            return (T) InjectionProviders[typeof(T)](arguments);
        }

        /// <summary>
        /// Replaces or adds a provider in the injection providers dictionary using the
        /// type of generic parameter T as the key and the provided instance as the value
        /// return from the provider function.
        /// </summary>
        public static void UseProvider<T>(T instance)
        {
            InjectionProviders[typeof(T)] = CreateProviderFor(instance);
        }

        /// <summary>
        /// Resets all of the initial provider functions back to their original state.
        /// </summary>
        public static void ResetProviders()
        {
            foreach (Type key in DefaultInjectionProviders.Keys)
            {
                InjectionProviders[key] = DefaultInjectionProviders[key];
            }
        }

        public static ProviderFunction CreateProviderFor(object toReturn)
        {
            return (object[] arguments) => toReturn;
        }

    }

}