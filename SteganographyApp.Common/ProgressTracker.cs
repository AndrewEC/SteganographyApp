namespace SteganographyApp.Common;

using SteganographyApp.Common.Injection;
using SteganographyApp.Common.Injection.Proxies;

/// <summary>
/// Utility class to help write messages to the console along with a
/// percent representing the progress currently made for the current
/// operation.
/// </summary>
/// <remarks>
/// Initializes a new ProgressTracker instance.
/// </remarks>
/// <param name="maxProgress">The maximum progress.</param>
/// <param name="progressMessage">The message to print while progress has not yet reached 100.</param>
/// <param name="completeMessage">The message to print after the progress has reached 100.</param>
/// <param name="consoleWriter">Represents the output stream where the output of this class
/// will be piped to.</param>
public sealed class ProgressTracker(
    double maxProgress,
    string progressMessage,
    string completeMessage,
    IConsoleWriter consoleWriter)
{
    private readonly double maxProgress = maxProgress;
    private readonly string progressMessage = progressMessage;
    private readonly string completeMessage = completeMessage;
    private readonly IConsoleWriter outputWriter = consoleWriter ?? ServiceContainer.GetService<IConsoleWriter>();
    private double currentProgress;
    private bool hasCompleted = false;

    /// <summary>
    /// Displays the progress message with a progress of 0.
    /// </summary>
    /// <returns>This instance.</returns>
    public ProgressTracker Display()
    {
        double percent = currentProgress / maxProgress * 100.0;
        if (percent >= 100.0)
        {
            if (hasCompleted)
            {
                return this;
            }

            hasCompleted = true;
            outputWriter.WriteLine(completeMessage);
        }
        else
        {
            outputWriter.Write($"{progressMessage} :: {(int)percent}%\r");
        }

        return this;
    }

    /// <summary>
    /// Increments the current progress by one and reprints the progress message.
    /// <para>The progress percent is effectively maxProgress / currentProgress * 100.</para>
    /// <para>If the tick puts the progress at 100% or higher than the completeMessage provided
    /// in the constructor will be printed.</para>
    /// </summary>
    public void UpdateAndDisplayProgress()
    {
        currentProgress++;
        Display();
    }
}