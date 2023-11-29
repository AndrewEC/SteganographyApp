namespace SteganographyApp.Common;

using SteganographyApp.Common.Injection;

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
public sealed class ProgressTracker(double maxProgress, string progressMessage, string completeMessage)
{
    private readonly double maxProgress = maxProgress;
    private readonly string progressMessage = progressMessage;
    private readonly string completeMessage = completeMessage;
    private readonly IConsoleWriter outputWriter = Injector.Provide<IConsoleWriter>();
    private double currentProgress;

    /// <summary>
    /// Initializes a ProgressTracker instance and prints the first progress message with a percentage of 0.
    /// </summary>
    /// <param name="maxProgress">The maximum progress.</param>
    /// <param name="progressMessage">The message to print while progress has not yet reached 100.</param>
    /// <param name="completeMessage">The message to print after the progress has reached 100.</param>
    /// <returns>Returns a new ProgressTracker instance instantiated with the provided values.</returns>
    public static ProgressTracker CreateAndDisplay(double maxProgress, string progressMessage, string completeMessage)
    {
        var tracker = new ProgressTracker(maxProgress, progressMessage, completeMessage);
        tracker.Display();
        return tracker;
    }

    /// <summary>
    /// Displays the progress message with a progress of 0.
    /// </summary>
    public void Display()
    {
        double percent = currentProgress / maxProgress * 100.0;
        if (percent >= 100.0)
        {
            outputWriter.WriteLine(completeMessage);
        }
        else
        {
            outputWriter.Write($"{progressMessage} :: {(int)percent}%\r");
        }
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