namespace SteganographyApp.Common.Logging
{
    using System;

    public interface ILogger
    {
        void Trace(string message, params object[] arguments);

        void Trace(string message, Func<object[]> provider);

        void Debug(string message, params object[] arguments);

        void Debug(string message, Func<object[]> provider);

        void Error(string message, params object[] arguments);

        void Error(string message, Func<object[]> provider);
    }

    internal sealed class Logger : ILogger
    {
        private readonly string typeName;

        public Logger(string typeName)
        {
            this.typeName = typeName;
        }

        public void Trace(string message, params object[] arguments) => Log(LogLevel.Trace, message, arguments);

        public void Trace(string message, Func<object[]> provider) => Log(LogLevel.Trace, message, provider);

        public void Debug(string message, params object[] arguments) => Log(LogLevel.Debug, message, arguments);

        public void Debug(string message, Func<object[]> provider) => Log(LogLevel.Debug, message, provider);

        public void Error(string message, params object[] arguments) => Log(LogLevel.Error, message, arguments);

        public void Error(string message, Func<object[]> provider) => Log(LogLevel.Error, message, provider);

        private void Log(LogLevel level, string message, params object[] arguments) => RootLogger.Instance.LogToFile(typeName, level, message, arguments);

        private void Log(LogLevel level, string message, Func<object[]> provider) => RootLogger.Instance.LogToFile(typeName, level, message, provider);
    }
}