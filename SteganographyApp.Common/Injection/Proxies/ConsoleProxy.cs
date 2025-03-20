namespace SteganographyApp.Common.Injection.Proxies;

using System;

#pragma warning disable SA1402

/// <summary>
/// Inteface that exists for testing the <see cref="ProgressTracker"/> class.
/// <para>The interface will be stubbed and provided as a constructor
/// argument when testing to verify the proper output is being sent to
/// the console.</para>
/// </summary>
public interface IConsoleWriter
{
    /// <summary>
    /// Writes the specified string to output using Console.Write.
    /// </summary>
    /// <param name="line">The string to write to the console without an added new line ending.</param>
    void Write(string line);

    /// <summary>
    /// Writes the specified string to output using Console.WriteLine.
    /// </summary>
    /// <param name="line">The string to write to the console with an attached new line break.</param>
    void WriteLine(string line);
}

/// <summary>
/// Interface that exists for testing the sensitive argument parser.
/// <para>This will allow the test to stub out the user input operations for some tests.</para>
/// </summary>
public interface IConsoleReader
{
    /// <summary>
    /// Reads a key press from the console using Console.ReadKey.
    /// </summary>
    /// <param name="intercept">Intecept.</param>
    /// <returns>The info of the key that was pressed.</returns>
    ConsoleKeyInfo ReadKey(bool intercept);
}

/// <summary>
/// The default IWriter used by the application in it's normal flow.
/// <para>Simply proxies the Write and WriteLine methods of the
/// console class.</para>
/// </summary>
public class ConsoleWriter : IConsoleWriter
{
    /// <inheritdoc/>
    public void Write(string line) => Console.Write(line);

    /// <inheritdoc/>
    public void WriteLine(string line) => Console.WriteLine(line);
}

/// <summary>
/// The default IInputReader instance that acts as a proxy to the <see cref="Console.ReadKey(bool)"/>
/// method.
/// </summary>
public class ConsoleKeyReader : IConsoleReader
{
    /// <inheritdoc/>
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
}

#pragma warning restore SA1402