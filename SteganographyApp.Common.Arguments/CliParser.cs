namespace SteganographyApp.Common.Arguments;

using System;
using System.Collections.Immutable;
using System.Linq;

using SteganographyApp.Common.Arguments.Matching;
using SteganographyApp.Common.Arguments.Validation;

/// <summary>
/// Provides the functionality to parse the user provided arguments into a concrete class instance.
/// </summary>
public interface ICliParser
{
    /// <summary>
    /// Gets the last error that occurred while trying to parse the user's
    /// command line arguments. This will be null if no error ocurred or if
    /// <see cref="TryParseArgs{T}(out T, string[])"/> has not been invoked.
    /// </summary>
    public Exception? LastError { get; }

    /// <summary>
    /// Attempts to parse the user provided arguments into the specified
    /// class model. If an exception is thrown during the parsing process this will
    /// allow the exception to propogate up.
    /// </summary>
    /// <param name="arguments">The list of user provided arguments to be parsed.</param>
    /// <typeparam name="T">The class containing the argument attributes from
    /// which the arguments to parsed will be derived from.</typeparam>
    /// <returns>An instance of T.</returns>
    public T ParseArgs<T>(string[] arguments)
    where T : class;

    /// <summary>
    /// Attempts to parse the user provided arguments into the specified class
    /// model. If an exception occurs during the parsing process this will
    /// return false but will not directly re-throw the original exception.
    /// The actual exception causing the failure can be accessed through the LastError property.
    /// </summary>
    /// <param name="model">The model, whose type matches the type of type
    /// parameter T, to be initialized.</param>
    /// <param name="arguments">The list of user provided arguments to be
    /// parsed.</param>
    /// <typeparam name="T">The class containing the argument attributes from
    /// which the arguments to parsed will be derived from.</typeparam>
    /// <returns>True if this was successful in parsing out the user provided
    /// arguments, otherwise returns false.</returns>
    public bool TryParseArgs<T>(out T model, string[] arguments)
    where T : class;
}

/// <summary>
/// Provides the functionality to parse the user provided arguments into a concrete class instance.
/// </summary>
public sealed class CliParser(
    IArgumentRegistration registration,
    IHelp help,
    IArgumentValueMatcher matcher,
    ICliValidator validator,
    IInitializer initializer) : ICliParser
{
    private Exception? lastError;

    /// <inheritdoc/>
    public Exception? LastError { get => lastError; }

    /// <inheritdoc/>
    public T ParseArgs<T>(string[] arguments)
    where T : class => ParseArgs(arguments, initializer.Initialize<T>());

    /// <inheritdoc/>
    public bool TryParseArgs<T>(out T model, string[] arguments)
    where T : class
    {
        T instance = initializer.Initialize<T>();
        try
        {
            model = ParseArgs(arguments, instance);
            return true;
        }
        catch (Exception e)
        {
            model = instance;
            lastError = e;
            return false;
        }
    }

    private static bool WasHelpRequested(string[] arguments)
        => arguments.Contains("--help") || arguments.Contains("-h");

    private T ParseArgs<T>(string[] arguments, T instance)
    where T : class
    {
        ImmutableArray<RegisteredArgument> registeredArguments = registration
            .FindAttributedArguments(typeof(T));

        if (WasHelpRequested(arguments))
        {
            help.PrintHelp(typeof(T), instance, registeredArguments);
            Environment.Exit(0);
        }

        foreach (MatchResult match in matcher.Match(arguments, registeredArguments))
        {
            try
            {
                var result = match.Parser.Invoke(instance, match.Input);
                TypeHelper.SetValue(instance, match.Member, result);
            }
            catch (Exception e)
            {
                throw new ParseException($"Could not read in argument [{match.Attribute.Name}] from value [{match.Input}] because: [{e.Message}].", e);
            }
        }

        validator.Validate(instance);

        return instance;
    }
}