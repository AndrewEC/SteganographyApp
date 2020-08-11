using SteganographyApp.Common.Injection;

namespace SteganographyApp.Common
{

    /// <summary>
    /// Utility class to help write messages to the console along with a
    /// percent representing the progress currently made for the current
    /// operation.
    /// </summary>
    public sealed class ProgressTracker
    {

        private readonly double maxProgress;
        private readonly string progressMessage;
        private readonly string completeMessage;
        private readonly IConsoleWriter outputWriter;
        private double currentProgress;

        public ProgressTracker(double maxProgress, string progressMessage, string completeMessage)
        {
            this.maxProgress = maxProgress;
            this.progressMessage = progressMessage;
            this.completeMessage = completeMessage;
            outputWriter = Injector.Provide<IConsoleWriter>();
        }

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
            outputWriter.Write($"{progressMessage} :: 0%\r");
        }

        /// <summary>
        /// Increments the current progress by one and reprints the progress message.
        /// <para>The progress percent is effectively maxProgress / currentProgress * 100.</para>
        /// <para>If the tick puts the progress at 100% or higher than the completeMessage provided
        /// in the constructor will be printed.</para>
        /// </summary>
        public void UpdateAndDisplayProgress()
        {
            currentProgress = currentProgress + 1;
            double percent = currentProgress / maxProgress * 100.0;
            if(percent >= 100.0)
            {
                percent = 100.0;
                outputWriter.WriteLine(completeMessage);
            }
            else
            {
                outputWriter.Write($"{progressMessage} :: {(int)percent}%\r");
            }
        }

    }

}