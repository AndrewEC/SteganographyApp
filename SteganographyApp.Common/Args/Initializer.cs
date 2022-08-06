namespace SteganographyApp.Common.Arguments
{
    using System;
    
    internal static class Initializer
    {
        private const string ErrorTemplate = "Could not instantiate type [{0}]. Make sure type is a class and has a default constructor.";

        public static T Initialize<T>()
        where T : class
        {
            Type typeToInitialize = typeof(T);
            try
            {
                T? instance = Activator.CreateInstance(typeToInitialize) as T;
                return instance ?? throw new ParseException(FormErrorMessage(typeToInitialize.FullName));
            }
            catch (Exception e)
            {
                throw new ParseException(FormErrorMessage(typeToInitialize.FullName), e);
            }
        }

        private static string FormErrorMessage(string? typeName) => string.Format(ErrorTemplate, typeName ?? "Unknown Type");
    }
}