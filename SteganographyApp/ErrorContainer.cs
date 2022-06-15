namespace SteganographyApp
{
    using System;
    using System.Threading;

    /// <summary>
    /// Thread-safe method to hold onto an error object so it can be shared
    /// between threads.
    /// </summary>
    internal class ErrorContainer
    {
        private static readonly object SyncLock = new object();

        private readonly CancellationTokenSource source = new CancellationTokenSource();
        private Exception? exception;

        ~ErrorContainer()
        {
            Suppressed.TryRun(source.Dispose);
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return source.Token;
            }
        }

        /// <summary>
        /// Checks to see if the exception object is not null.
        /// </summary>
        public bool HasException()
        {
            lock (SyncLock)
            {
                return exception != null;
            }
        }

        /// <summary>
        /// Sets the exception reference so it can be read by the <see cref="Decoder" />.
        /// </summary>
        public void PutException(Exception exception)
        {
            lock (SyncLock)
            {
                if (this.exception != null)
                {
                    return;
                }
                this.exception = exception;
                source.Cancel();
            }
        }

        /// <summary>
        /// Retrieves the value of the exception. Does not take the exception out
        /// in the sense that it will the reference to null.
        /// </summary>
        public Exception TakeException()
        {
            lock (SyncLock)
            {
                return exception!;
            }
        }
    }
}