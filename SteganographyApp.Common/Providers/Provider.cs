using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using SteganographyApp.Common.Data;

namespace SteganographyApp.Common.Providers
{

    public delegate object ProviderFunction(params object[] arguments);

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

        public static T Provide<T>(params object[] arguments)
        {
            return (T) InjectionProviders[typeof(T)](arguments);
        }

        public static void UseProvider<T>(T instance)
        {
            InjectionProviders[typeof(T)] = CreateProviderFor(instance);
        }

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