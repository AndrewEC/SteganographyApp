using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using SteganographyApp.Common.Data;
using SteganographyApp.Common.IO;

namespace SteganographyApp.Common.Providers
{


    /// <summary>
    /// An on request injection utility to help provide an implementation of a particular requested
    /// interface. It also provides the ability to replace an existing implementation in the injector
    /// with a mock of the interface.
    /// <para>With this utility most of the static method declarations should be replaced by
    /// calls to a method of a concrete instance that is injected via this utility.</para>
    /// </summary>
    public static class Injector
    {

        private static readonly ImmutableDictionary<Type, object> DefaultInjectionProviders = new Dictionary<Type, object>
        {
            { typeof(IEncryptionProvider), new EncryptionProvider() },
            { typeof(IFileProvider), new FileProvider() },
            { typeof(IBinaryUtil), new BinaryUtil() },
            { typeof(IDummyUtil), new DummyUtil() },
            { typeof(IRandomizeUtil), new RandomizeUtil() },
            { typeof(IDataEncoderUtil), new DataEncoderUtil() },
            { typeof(IImageProvider), new ImageProvider() },
            { typeof(ICompressionUtil), new CompressionUtil() },
            { typeof(IConsoleWriter), new ConsoleWriter() },
            { typeof(IConsoleReader), new ConsoleKeyReader() },
            { typeof(IChunkTableHelper), new ChunkTableHelper() }
        }
        .ToImmutableDictionary();

        private static readonly Dictionary<Type, object> InjectionProviders = new Dictionary<Type, object>();

        static Injector()
        {
            ResetProviders();
        }

        /// <summary>
        /// Returns an instance from the dictionary of injection provides using the generic
        /// parameter T as the lookup key.
        /// </summary>
        public static T Provide<T>()
        {
            return (T) InjectionProviders[typeof(T)];
        }

        /// <summary>
        /// Replaces or adds a provider in the injection providers dictionary using the
        /// type of generic parameter T as the key and the provided instance as the value
        /// return from the provider function.
        /// </summary>
        public static void UseProvider<T>(T instance)
        {
            InjectionProviders[typeof(T)] = instance;
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

    }

}