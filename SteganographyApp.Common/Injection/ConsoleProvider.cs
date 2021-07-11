namespace SteganographyApp.Common.Injection
{
    using System;

    /// <summary>
    /// Inteface that exists for testing the <see cref="ProgressTracker"/> class.
    /// <para>The interface will be stubbed and provided as a constructor
    /// argument when testing to verify the proper output is being sent to
    /// the console.</para>
    /// </summary>
    public interface IConsoleWriter
    {
        void Write(string line);

        void WriteLine(string line);
    }

    /// <summary>
    /// Interface that exists for testing the <see cref="ArgumentParser.ReadString"/> method from
    /// the ArgumentParser class.
    /// <para>This will allow the test to stub out the user input operations for some tests.</para>
    /// </summary>
    public interface IConsoleReader
    {
        ConsoleKeyInfo ReadKey(bool intercept);
    }

    /// <summary>
    /// The default IWriter used by the application in it's normal flow.
    /// <para>Simply proxies the Write and WriteLine methods of the
    /// console class.</para>
    /// </summary>
    [Injectable(typeof(IConsoleWriter))]
    public class ConsoleWriter : IConsoleWriter
    {
        public void Write(string line) => Console.Write(line);

        public void WriteLine(string line) => Console.WriteLine(line);
    }

    /// <summary>
    /// The default IInputReader instance that acts as a proxy to the <see cref="Console.ReadKey(bool)">
    /// method.
    /// </summary>
    [Injectable(typeof(IConsoleReader))]
    public class ConsoleKeyReader : IConsoleReader
    {
        public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    }
}