namespace SteganographyApp.Common.Arguments;

using Microsoft.Extensions.DependencyInjection;
using SteganographyApp.Common.Arguments.Matching;
using SteganographyApp.Common.Arguments.Validation;

public sealed class ArgumentsServiceContainer
{
    private static readonly ArgumentsServiceContainer Instance = new();

    private readonly ServiceProvider serviceProvider;

    private ArgumentsServiceContainer()
    {
        ServiceCollection services = new();

        services.AddTransient<ICliParser, CliParser>();

        services.AddSingleton<IArgumentRegistration, ArgumentRegistration>();
        services.AddSingleton<IHelp, Help>();
        services.AddSingleton<IArgumentValueMatcher, ArgumentValueMatcher>();
        services.AddSingleton<ICliValidator, CliValidator>();
        services.AddSingleton<IInitializer, Initializer>();
        services.AddSingleton<IParserFunctionLookup, ParserFunctionLookup>();

        serviceProvider = services.BuildServiceProvider();
    }

    public static T GetService<T>()
    where T : class => Instance.serviceProvider.GetService<T>()!;
}