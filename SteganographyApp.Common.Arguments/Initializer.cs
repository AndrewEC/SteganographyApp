namespace SteganographyApp.Common.Arguments;

using System;

/// <summary>
/// Utility to help initialize an instance of a class with a default constructor.
/// </summary>
public interface IInitializer
{
    /// <summary>
    /// Initializes an instance of type T. Requires the type to have a default constructor.
    /// </summary>
    /// <typeparam name="T">The type to reflectively instantiate. Type T must be a class.</typeparam>
    /// <returns>A new instance of the type T.</returns>
    public T Initialize<T>()
    where T : class;
}

/// <summary>
/// Utility to help initialize an instance of a class with a default constructor.
/// </summary>
public sealed class Initializer : IInitializer
{
    private const string ErrorTemplate = "Could not instantiate type [{0}]. Make sure type is a class and has a default constructor.";

    /// <inheritdoc/>
    public T Initialize<T>()
    where T : class
    {
        Type typeToInitialize = typeof(T);
        try
        {
            T? instance = Activator.CreateInstance(typeToInitialize) as T;
            return instance ?? throw new ParseException(FormErrorMessage(typeToInitialize.FullName));
        }
        catch (Exception e) when (e is not ParseException)
        {
            throw new ParseException(FormErrorMessage(typeToInitialize.FullName), e);
        }
    }

    private static string FormErrorMessage(string? typeName) => string.Format(ErrorTemplate, typeName ?? "Unknown Type");
}