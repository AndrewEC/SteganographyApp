namespace SteganographyApp.Common
{
    using System;
    using System.Text;

    using SteganographyApp.Common.Injection;

    public sealed class RootLogger
    {
        public static readonly RootLogger Instance = new RootLogger();

        private static readonly string LogFileName = "steganography.logs.txt";
        private static readonly string LogMessageTemplate = "[{0}] [{1}] => {2}\n";
        private static readonly object SyncLock = new object();

        private static LogLevel logLevel = LogLevel.None;
        private IReadWriteStream writeLogStream;

        private RootLogger() { }

        ~RootLogger()
        {
            if (writeLogStream != null)
            {
                try
                {
                    writeLogStream.Dispose();
                    writeLogStream = null;
                }
                catch
                {
                }
            }
        }

        public void Enable(LogLevel level)
        {
            logLevel = TryOpenLogFileForWrite() ? level : LogLevel.None;
        }

        public void LogToFile(string typeName, LogLevel level, string message, params object[] arguments)
        {
            if (!CanLog(level))
            {
                return;
            }
            LogToFile(FormLogMessage(typeName, level, message, arguments));
        }

        public void LogToFile(string typeName, LogLevel level, string message, ArgumentProvider provider)
        {
            if (!CanLog(level))
            {
                return;
            }
            try
            {
                object[] arguments = provider();
                LogToFile(FormLogMessage(typeName, level, message, arguments));
            }
            catch (Exception e)
            {
                LogToFile(FormLogMessage(typeName, LogLevel.Error, "Error forming log messsage: [{0}]", e.Message));
                LogToFile(FormLogMessage(typeName, LogLevel.Error, "Original message: [{0}]", message));
            }
        }

        private bool CanLog(LogLevel requestedLevel) => (int)requestedLevel >= (int)logLevel;

        private void LogToFile(string message)
        {
            lock (SyncLock)
            {
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
                Console.WriteLine("Warning: Could not open log file for write. Logging will be disabled. Caused by: [{0}]", e.Message);
                return false;
            }
        }
    }
}