using System;
using System.Text;

using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common
{

    public enum LogLevel
    {
        Trace,
        Debug,
        Error
    }

    public interface ILogger
    {
        void Trace(string message, params object[] arguments);
        void Debug(string message, params object[] arguments);
        void Error(string message, params object[] arguments);
        void Log(LogLevel level, string message, params object[] arguments);
    }

    sealed class Logger : ILogger
    {

        private readonly string typeName;

        public Logger(string typeName)
        {
            this.typeName = typeName;
        }

        public void Trace(string message, params object[] arguments) => Log(LogLevel.Trace, message, arguments);
        public void Debug(string message, params object[] arguments) => Log(LogLevel.Debug, message, arguments);
        public void Error(string message, params object[] arguments) => Log(LogLevel.Error, message, arguments);

        public void Log(LogLevel level, string message, params object[] arguments)
        {
            RootLogger.Instance.LogToFile(typeName, level, message, arguments);
        }
    }

    public sealed class RootLogger
    {

        public static readonly RootLogger Instance = new RootLogger();

        private static readonly string LogFileName = "steganography.logs.txt";
        private static readonly string LogMessageTemplate = "[{0}] [{1}] => {2}\n";
        private static readonly object _lock = new object();

        private IReadWriteStream writeLogStream;
        private bool canLog = false;

        private RootLogger() { }

        ~RootLogger()
        {
            if (writeLogStream != null)
            {
                writeLogStream.Dispose();
                writeLogStream = null;
            }
        }

        public void Enable()
        {
            canLog = true;
        }

        public void LogToFile(string typeName, LogLevel level, string message, params object[] arguments)
        {
            if (!canLog)
            {
                return;
            }
            LogToFile(FormLogMessage(typeName, level, message, arguments));
        }

        private void LogToFile(string message)
        {
            lock (_lock)
            {
                if (writeLogStream == null)
                {
                    canLog = TryOpenLogFileForWrite();
                    if (!canLog)
                    {
                        return;
                    }
                }
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                writeLogStream.Write(messageBytes, 0, messageBytes.Length);
                writeLogStream.Flush();
            }
        }

        private string FormLogMessage(string typeName, LogLevel level, string message, params object[] arguments)
        {
            string subMessage = string.Format(message, arguments);
            return string.Format(LogMessageTemplate, level.ToString(), typeName, subMessage);
        }

        private bool TryOpenLogFileForWrite()
        {
            try
            {
                var provider = Injector.Provide<IFileProvider>();
                if (provider.IsExistingFile(LogFileName))
                {
                    provider.Delete(LogFileName);
                }
                writeLogStream = provider.OpenFileForWrite(LogFileName);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning: Could not open log file for write. Logging will be disabled. Caused by: {0}", e.Message);
                return false;
            }
        }

    }

    public interface ILoggerFactory
    {
        ILogger LoggerFor(Type type);
    }

    [Injectable(typeof(ILoggerFactory))]
    public class LoggerFactory: ILoggerFactory
    {

        private static readonly string TypeNameTemplate = "{0}.{1}";

        public ILogger LoggerFor(Type type)
        {
            string name = string.Format(TypeNameTemplate, type.Namespace, type.Name);
            return new Logger(name);
        }
    }

}