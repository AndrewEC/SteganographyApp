namespace SteganographyApp.Common
{
    using System;

    using SteganographyApp.Common.Injection;

    public interface ILoggerFactory
    {
        ILogger LoggerFor(Type type);
    }

    [Injectable(typeof(ILoggerFactory))]
    public class LoggerFactory : ILoggerFactory
    {
        private static readonly string TypeNameTemplate = "{0}.{1}";

        public ILogger LoggerFor(Type type)
        {
            string name = string.Format(TypeNameTemplate, type.Namespace, type.Name);
            return new Logger(name);
        }
    }
}