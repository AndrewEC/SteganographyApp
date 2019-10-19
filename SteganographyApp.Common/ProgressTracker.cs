using SteganographyApp.Common.Test;

namespace SteganographyApp.Common
{

    /// <summary>
    /// Utility class to help write messages to the console along with a
    /// percent representing the progress currently made for the current
    /// operation.
    /// </summary>
    public class ProgressTracker
    {

        private readonly double maxProgress;
        private readonly string progressMessage;
        private readonly string completeMessage;
        private readonly IWriter writer;
        private double currentProgress;

        public ProgressTracker(double maxProgress, string progressMessage, string completeMessage, IWriter writer)
        {
            this.maxProgress = maxProgress;
            this.progressMessage = progressMessage;
            this.completeMessage = completeMessage;
            this.writer = writer;
        }

        /// <summary>
        /// Initializes the progress tracker.
        /// </summary>
        /// <param name="maxProgress">The maximum amount of progress to be made for the current operation</param>
        /// <param name="progressMessage">The message to display with the progress percent so long
        /// as the current progress has not reached the maxProgress.</param>
        /// <param name="completeMessage">The message to display once the operation has completed. I.e. when
        /// the current progress has reached the maxProgress value.</param>
        public ProgressTracker(double maxProgress, string progressMessage, string completeMessage)
            : this(maxProgress, progressMessage, completeMessage, new ConsoleWriter()) {}

        /// <summary>
        /// Displays the progress message with a progress of 0.
        /// </summary>
        public void Display()
        {
            writer.Write(string.Format("{0} :: 0%\r", progressMessage));
        }

        /// <summary>
        /// Increments the current progress by one and reprints the progress message.
        /// <para>The progress percent is effectively maxProgress / currentProgress * 100.</para>
        /// <para>If the tick puts the progress at 100% or higher than the completeMessage provided
        /// in the constructor will be printed.</para>
        /// </summary>
        public void TickAndDisplay()
        {
            currentProgress = currentProgress + 1;
            double percent = currentProgress / maxProgress * 100.0;
            if(percent >= 100.0)
            {
                percent = 100.0;
                writer.WriteLine(completeMessage);
            }
            else
            {
                writer.Write(string.Format("{0} :: {1}%\r", progressMessage, (int)percent));
            }
        }

    }

}