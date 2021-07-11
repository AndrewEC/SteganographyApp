namespace SteganographyApp.Common.Logging
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

        /// <summary>
        /// Attempts to open up the log file for write and use the specified log level to determine which incoming
        /// messages should be logged.
        /// <para>This method attempts to swallow all exceptions. If an exception ocurrs the log level will be set to None
        /// and a message will be logged to console.</para>
        /// </summary>
        public void EnableLoggingAtLevel(LogLevel level)
        {
            logLevel = TryOpenLogFileForWrite() ? level : LogLevel.None;
        }

        /// <summary>
        /// Attempts to write the specified message to the log file.
        /// </summary>
        /// <param name="typeName">The name of the concrete type that is attempting to log this message.</param>
        /// <param name="level">The level the message will be logged at.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="arguments">The option array of arguments to substitute into the message value before logging.</param>
        public void LogToFile(string typeName, LogLevel level, string message, params object[] arguments)
        {
            if (!CanLog(level))
            {
                return;
            }
            LogToFile(FormLogMessage(typeName, level, message, arguments));
        }

        /// <summary>
        /// Attempts to write the specified message to the log file.
        /// It is recommended to use this when the operations for determining the arguments that need to be logged are
        /// expensive.
        /// </summary>
        /// <param name="typeName">The name of the concrete type that is attempting to log this message.</param>
        /// <param name="level">The level the message will be logged at.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="provider">A producer function that takes in no arguments and will produce an array of arguments
        /// that will be substituded into the message parameter before logging. This function will only be invoked after
        /// it has been determined that the message should be logged based on the specified level.</param>
        public void LogToFile(string typeName, LogLevel level, string message, Func<object[]> provider)
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
                var fileIOProxy = Injector.Provide<IFileIOProxy>();
                if (fileIOProxy.IsExistingFile(LogFileName))
                {
                    fileIOProxy.Delete(LogFileName);
                }
                writeLogStream = fileIOProxy.OpenFileForWrite(LogFileName);
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