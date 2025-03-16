namespace SteganographyApp.Common.Injection;

using System;
using Microsoft.Extensions.DependencyInjection;
using SteganographyApp.Common.Data;
using SteganographyApp.Common.Injection.Proxies;
using SteganographyApp.Common.IO;
using SteganographyApp.Common.IO.Content;
using SteganographyApp.Common.Logging;

/// <summary>
/// A singleton class containing the <see cref="ServiceProvider"/>
/// by which all registered dependencies will be injected.
/// </summary>
public sealed class ServiceContainer : AbstractDisposable
{
    private static readonly ServiceContainer Instance = new();

    private readonly ServiceProvider serviceProvider;

    private ServiceContainer()
    {
        ServiceCollection services = new();

        services.AddSingleton<IBinaryUtil, BinaryUtil>();
        services.AddSingleton<ICompressionUtil, CompressionUtil>();
        services.AddSingleton<IDataEncoderUtil, DataEncoderUtil>();
        services.AddSingleton<IDummyUtil, DummyUtil>();
        services.AddSingleton<IEncoderProvider, EncoderProvider>();
        services.AddSingleton<IEncryptionUtil, EncryptionUtil>();
        services.AddSingleton<IKeyUtil, KeyUtil>();
        services.AddSingleton<IRandomizeUtil, RandomizeUtil>();

        services.AddSingleton<IFileIOProxy, FileIOProxy>();
        services.AddSingleton<IImageProxy, ImageProxy>();
        services.AddSingleton<IConsoleReader, ConsoleKeyReader>();
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();

        services.AddSingleton<ICalculator, Calculator>();

        services.AddSingleton<ILoggerFactory, LoggerFactory>();

        services.AddTransient<Func<IInputArguments, ContentReader>>((provider) =>
        {
            return (args) =>
            {
                IDataEncoderUtil dataEncoderUtil = provider.GetRequiredService<IDataEncoderUtil>();
                IFileIOProxy fileIOProxy = provider.GetRequiredService<IFileIOProxy>();
                return new ContentReader(args, dataEncoderUtil, fileIOProxy);
            };
        });

        services.AddTransient<Func<IInputArguments, ContentWriter>>((provider) =>
        {
            return (args) =>
            {
                IDataEncoderUtil dataEncoderUtil = provider.GetRequiredService<IDataEncoderUtil>();
                IFileIOProxy fileIOProxy = provider.GetRequiredService<IFileIOProxy>();
                return new ContentWriter(args, dataEncoderUtil, fileIOProxy);
            };
        });

        services.AddTransient<Func<IInputArguments, ImageStore>>((provider) =>
        {
            return (args) =>
            {
                IImageProxy imageProxy = provider.GetRequiredService<IImageProxy>();
                IEncoderProvider encoderProvider = provider.GetRequiredService<IEncoderProvider>();
                return new ImageStore(args, encoderProvider, imageProxy);
            };
        });

        services.AddTransient<Func<double, string, string, ProgressTracker>>((provider) =>
        {
            return (maxProgress, progressMessage, completeMessage) =>
            {
                IConsoleWriter writer = provider.GetRequiredService<IConsoleWriter>();
                return new ProgressTracker(maxProgress, progressMessage, completeMessage, writer);
            };
        });

        serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ContentWriter"/> with the
    /// required services injected.
    /// </summary>
    /// <param name="args">The dynamic arguments required by the content writer.</param>
    /// <returns>A new instance of the content writer.</returns>
    public static ContentWriter CreateContentWriter(IInputArguments args)
        => Instance.DoGetService<Func<IInputArguments, ContentWriter>>().Invoke(args);

    /// <summary>
    /// Creates a new instance of the <see cref="ContentReader"/> with the required
    /// services injected.
    /// </summary>
    /// <param name="args">The dynamic arguments required by the content reader.</param>
    /// <returns>A new instance of the content reader.</returns>
    public static ContentReader CreateContentReader(IInputArguments args)
        => Instance.DoGetService<Func<IInputArguments, ContentReader>>().Invoke(args);

    /// <summary>
    /// Creates a new instance of the <see cref="ImageStore"/> with the required
    /// dependencies injected.
    /// </summary>
    /// <param name="args">The dynamic arguments required by the image store.</param>
    /// <returns>A new instance of the ImageStore.</returns>
    public static ImageStore CreateImageStore(IInputArguments args)
        => Instance.DoGetService<Func<IInputArguments, ImageStore>>().Invoke(args);

    /// <summary>
    /// Creates a new <see cref="ProgressTracker"/> instance with the required
    /// service dependencies injected.
    /// </summary>
    /// <param name="maxProgress">The maximum progress.</param>
    /// <param name="progressMessage">The message to print while progress has not yet reached 100.</param>
    /// <param name="completeMessage">The message to print after the progress has reached 100.</param>
    /// <returns>A new progress tracker instance.</returns>
    public static ProgressTracker CreateProgressTracker(
        double maxProgress, string progressMessage, string completeMessage)
    {
        return Instance.DoGetService<Func<double, string, string, ProgressTracker>>()
            .Invoke(maxProgress, progressMessage, completeMessage);
    }

    /// <summary>
    /// Retrieves a service of type T from the underlying service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service being requested.</typeparam>
    /// <returns>A concrete implementation of the requested service type.</returns>
    public static T GetService<T>() => Instance.DoGetService<T>();

    /// <summary>
    /// Retrieves an ILogger instance for the requested type via the available
    /// service assoaciated with <see cref="ILoggerFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of the class that will be consuming the logger.</typeparam>
    /// <returns>An ILogger instance for the specified type.</returns>
    public static ILogger GetLogger<T>() => GetLogger(typeof(T));

    /// <summary>
    /// Retrieves an ILogger instance for the requested type via the available
    /// service assoaciated with <see cref="ILoggerFactory"/>.
    /// </summary>
    /// <param name="type">The type of the class that will be consuming the logger.</param>
    /// <returns>An ILogger instance for the specified type.</returns>
    public static ILogger GetLogger(Type type)
        => Instance.DoGetService<ILoggerFactory>().LoggerFor(type);

    /// <summary>
    /// Invokes the <see cref="ServiceProvider"/>'s dispose method.
    /// </summary>
    protected override void DoDispose() => RunIfNotDisposed(() => serviceProvider.Dispose());

    private T DoGetService<T>()
        => RunIfNotDisposedWithResult(() => serviceProvider.GetService<T>()!);
}